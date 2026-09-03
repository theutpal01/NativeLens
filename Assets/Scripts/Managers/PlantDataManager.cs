using UnityEngine;
using NativeLens.Models;
using NativeLens.Data;
using System.Collections.Generic;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages plant data access and discovery state.
    /// Keeps plant data separate from UI code as per agent rules.
    /// </summary>
    public class PlantDataManager : MonoBehaviour
    {
        public static PlantDataManager Instance { get; private set; }

        [Header("Data")]
        [SerializeField] private PlantDatabase plantDatabase;

        private Dictionary<string, Plant> plantLookup = new Dictionary<string, Plant>();
        private Dictionary<string, PlantDiscoveryState> discoveryStates = new Dictionary<string, PlantDiscoveryState>();

        public IReadOnlyDictionary<string, PlantDiscoveryState> DiscoveryStates => discoveryStates;
        public int TotalPlants => plantDatabase?.TotalPlantCount ?? 0;
        public int DiscoveredCount { get; private set; }

        public event System.Action<int, int> OnDiscoveryProgressChanged; // discovered, total
        public event System.Action<string> OnPlantDiscovered; // plantId

        private const string DISCOVERY_PREFS_KEY = "nativelens_discovery_state";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePlantLookup();
            LoadDiscoveryStates();
        }

        private void InitializePlantLookup()
        {
            plantLookup.Clear();
            if (plantDatabase != null)
            {
                foreach (var plant in plantDatabase.Plants)
                {
                    plantLookup[plant.id] = plant;
                }
            }
        }

        private void LoadDiscoveryStates()
        {
            discoveryStates.Clear();
            DiscoveredCount = 0;

            if (plantDatabase == null) return;

            string json = PlayerPrefs.GetString(DISCOVERY_PREFS_KEY, "{}");
            DiscoveryStateWrapper wrapper = JsonUtility.FromJson<DiscoveryStateWrapper>(json);

            if (wrapper != null && wrapper.states != null)
            {
                foreach (var state in wrapper.states)
                {
                    discoveryStates[state.plantId] = state;
                    if (state.IsDiscovered) DiscoveredCount++;
                }
            }

            // Ensure all plants have a state entry
            foreach (var plant in plantDatabase.Plants)
            {
                if (!discoveryStates.ContainsKey(plant.id))
                {
                    discoveryStates[plant.id] = new PlantDiscoveryState { plantId = plant.id, state = DiscoveryState.Locked };
                }
            }

            OnDiscoveryProgressChanged?.Invoke(DiscoveredCount, TotalPlants);
        }

        private void SaveDiscoveryStates()
        {
            var wrapper = new DiscoveryStateWrapper();
            wrapper.states = new List<PlantDiscoveryState>(discoveryStates.Values);
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(DISCOVERY_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        public Plant GetPlant(string id)
        {
            plantLookup.TryGetValue(id, out Plant plant);
            return plant;
        }

        public Plant GetPlantByScientificName(string scientificName)
        {
            return plantDatabase?.GetPlantByScientificName(scientificName);
        }

        public PlantDiscoveryState GetDiscoveryState(string plantId)
        {
            discoveryStates.TryGetValue(plantId, out PlantDiscoveryState state);
            return state ?? new PlantDiscoveryState { plantId = plantId, state = DiscoveryState.Locked };
        }

        public bool IsDiscovered(string plantId)
        {
            return GetDiscoveryState(plantId).IsDiscovered;
        }

        public bool IsIdentified(string plantId)
        {
            return GetDiscoveryState(plantId).IsIdentified;
        }

        /// <summary>
        /// Marks a plant as identified (AI recognition successful, but not yet confirmed as discovered by user)
        /// </summary>
        public void MarkIdentified(string plantId)
        {
            if (discoveryStates.TryGetValue(plantId, out var state))
            {
                if (state.state < DiscoveryState.Identified)
                {
                    state.state = DiscoveryState.Identified;
                    SaveDiscoveryStates();
                }
            }
        }

        /// <summary>
        /// Marks a plant as fully discovered (user confirmed/added to gallery)
        /// </summary>
        public bool MarkDiscovered(string plantId, string location = "", float lat = 0, float lon = 0, string photoPath = "")
        {
            if (discoveryStates.TryGetValue(plantId, out var state))
            {
                bool wasDiscovered = state.IsDiscovered;
                state.state = DiscoveryState.Discovered;
                state.discoveredAt = System.DateTime.Now;
                state.discoveryLocation = location;
                state.discoveryLatitude = lat;
                state.discoveryLongitude = lon;
                state.discoveryPhotoPath = photoPath;

                if (!wasDiscovered)
                {
                    DiscoveredCount++;
                    OnDiscoveryProgressChanged?.Invoke(DiscoveredCount, TotalPlants);
                    OnPlantDiscovered?.Invoke(plantId);
                }

                SaveDiscoveryStates();
                return !wasDiscovered; // Returns true if this was a NEW discovery
            }
            return false;
        }

        public float GetDiscoveryProgress()
        {
            return TotalPlants > 0 ? (float)DiscoveredCount / TotalPlants : 0f;
        }

        public List<Plant> GetDiscoveredPlants()
        {
            var list = new List<Plant>();
            foreach (var kvp in discoveryStates)
            {
                if (kvp.Value.IsDiscovered && plantLookup.TryGetValue(kvp.Key, out var plant))
                {
                    list.Add(plant);
                }
            }
            return list;
        }

        public List<Plant> GetUndiscoveredPlants()
        {
            var list = new List<Plant>();
            foreach (var plant in plantDatabase.Plants)
            {
                if (!discoveryStates[plant.id].IsDiscovered)
                {
                    list.Add(plant);
                }
            }
            return list;
        }

        [System.Serializable]
        private class DiscoveryStateWrapper
        {
            public List<PlantDiscoveryState> states = new List<PlantDiscoveryState>();
        }
    }
}