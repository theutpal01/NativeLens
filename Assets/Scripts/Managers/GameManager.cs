using UnityEngine;
using NativeLens.Models;
using NativeLens.Managers;
using NativeLens.Data;

namespace NativeLens.Managers
{
    /// <summary>
    /// Main game manager - coordinates application state machine and initializes all systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PlantDatabase plantDatabase;
        [SerializeField] private bool debugMode = true;

        public AppState CurrentState { get; private set; } = AppState.Home;

        public event System.Action<AppState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize plant database
            if (plantDatabase != null)
            {
                plantDatabase.InitializeMVPPlants();
            }

            // Initialize managers in order
            InitializeManagers();
        }

        private void Start()
        {
            ChangeState(AppState.Home);
        }

        private void InitializeManagers()
        {
            // Managers initialize themselves via Awake/Start
            // But we can ensure they exist
            var _ = PlantDataManager.Instance;
            var _ = ARManager.Instance;
            var _ = GalleryManager.Instance;
            var _ = PlantIdentificationManager.Instance;
            var _ = BotanicalGuideManager.Instance;
            var _ = ObservationManager.Instance;
            var _ = UIManager.Instance;

            // Wire up identification result handler
            if (PlantIdentificationManager.Instance != null)
            {
                PlantIdentificationManager.Instance.OnIdentificationComplete += HandleIdentificationResult;
            }
        }

        private void HandleIdentificationResult(IdentificationResult result)
        {
            if (!result.success)
            {
                ChangeState(AppState.UnableToIdentify);
                ARManager.Instance?.ShowUnableToIdentify();
                return;
            }

            // Get plant from database
            var plant = PlantDataManager.Instance.GetPlant(result.speciesId);
            if (plant == null)
            {
                // Try by scientific name
                plant = PlantDataManager.Instance.GetPlantByScientificName(result.scientificName);
            }

            if (plant == null)
            {
                Debug.LogError($"Plant not found for ID: {result.speciesId}");
                ChangeState(AppState.UnableToIdentify);
                return;
            }

            // Mark as identified
            PlantDataManager.Instance.MarkIdentified(plant.id);

            // Show AR info card
            ChangeState(AppState.Identified);
            ARManager.Instance?.PlaceARInfoCardAtScreenPosition(
                new Vector2(Screen.width / 2, Screen.height / 2), 
                plant, 
                result.confidence
            );
            ChangeState(AppState.ARInformation);
        }

        public void ChangeState(AppState newState)
        {
            if (CurrentState == newState) return;

            AppState previousState = CurrentState;
            CurrentState = newState;

            if (debugMode)
            {
                Debug.Log($"State Change: {previousState} -> {newState}");
            }

            OnStateChanged?.Invoke(newState);

            // Handle state-specific logic
            HandleStateEntry(newState);
        }

        private void HandleStateEntry(AppState state)
        {
            switch (state)
            {
                case AppState.Home:
                    UIManager.Instance?.SwitchTab(NavigationTab.Home);
                    break;
                case AppState.Scan:
                    UIManager.Instance?.SwitchTab(NavigationTab.Scan);
                    break;
                case AppState.Analysing:
                    ARManager.Instance?.ShowAnalysingUI();
                    break;
                case AppState.Gallery:
                    UIManager.Instance?.SwitchTab(NavigationTab.Gallery);
                    break;
            }
        }

        // Public methods for UI to trigger state changes
        public void StartScanning()
        {
            ChangeState(AppState.Scan);
        }

        public void OpenGallery()
        {
            ChangeState(AppState.Gallery);
        }

        public void ReturnHome()
        {
            ChangeState(AppState.Home);
        }

        public void TriggerDiscovery(Plant plant)
        {
            ChangeState(AppState.Discovery);
            // GalleryManager handles the animation
        }
    }
}