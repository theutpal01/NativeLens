using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;
using System.Collections.Generic;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages field observations - saving species, location, date, photo.
    /// Phase 8: Field observation saving.
    /// </summary>
    public class ObservationManager : MonoBehaviour
    {
        public static ObservationManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private GameObject observationPanel;
        [SerializeField] private TextMeshProUGUI obsSpeciesText;
        [SerializeField] private TextMeshProUGUI obsDateText;
        [SerializeField] private TextMeshProUGUI obsLocationText;
        [SerializeField] private TextMeshProUGUI obsCoordinatesText;
        [SerializeField] private RawImage obsPhotoImage;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button retakePhotoButton;

        [Header("GPS")]
        [SerializeField] private bool useGPS = true;
        [SerializeField] private float gpsUpdateInterval = 10f;

        private Plant currentPlant;
        private Texture2D currentPhoto;
        private LocationInfo currentLocation;
        private bool isGPSReady = false;

        private const string OBSERVATIONS_PREFS_KEY = "nativelens_observations";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (saveButton != null) saveButton.onClick.AddListener(SaveObservation);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelObservation);
            if (retakePhotoButton != null) retakePhotoButton.onClick.AddListener(RetakePhoto);

            if (observationPanel != null) observationPanel.SetActive(false);
        }

        private void Start()
        {
            if (useGPS)
            {
                StartCoroutine(StartGPS());
            }
        }

        private System.Collections.IEnumerator StartGPS()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("GPS not enabled by user");
                yield break;
            }

            Input.location.Start();
            
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (Input.location.status == LocationServiceStatus.Running)
            {
                isGPSReady = true;
                currentLocation = Input.location.lastData;
                Debug.Log($"GPS Ready: {currentLocation.latitude}, {currentLocation.longitude}");
            }
            else
            {
                Debug.LogWarning($"GPS failed: {Input.location.status}");
            }
        }

        public void SaveObservation(Plant plant)
        {
            currentPlant = plant;
            currentPhoto = null; // Would capture from camera in real implementation
            
            if (observationPanel == null) return;

            // Populate observation preview
            if (obsSpeciesText != null) obsSpeciesText.text = $"{plant.commonName} ({plant.scientificName})";
            if (obsDateText != null) obsDateText.text = $"Date: {System.DateTime.Now:dd MMM yyyy HH:mm}";
            
            string locationName = "VIT Vellore Campus"; // Could reverse geocode
            if (obsLocationText != null) obsLocationText.text = $"Location: {locationName}";
            
            if (isGPSReady)
            {
                currentLocation = Input.location.lastData;
                if (obsCoordinatesText != null)
                    obsCoordinatesText.text = $"Coordinates: {currentLocation.latitude:F6}, {currentLocation.longitude:F6}";
            }
            else
            {
                if (obsCoordinatesText != null)
                    obsCoordinatesText.text = "Coordinates: GPS unavailable";
            }

            observationPanel.SetActive(true);
        }

        private void SaveObservation()
        {
            if (currentPlant == null) return;

            var observation = new FieldObservation
            {
                id = System.Guid.NewGuid().ToString(),
                plantId = currentPlant.id,
                observedAt = System.DateTime.Now,
                latitude = isGPSReady ? currentLocation.latitude : 0,
                longitude = isGPSReady ? currentLocation.longitude : 0,
                locationName = "VIT Vellore Campus",
                photoPath = "", // Would save photo to persistent storage
                notes = ""
            };

            // Save to local storage
            SaveObservationToStorage(observation);

            Debug.Log($"Observation saved for {currentPlant.commonName} at {observation.latitude}, {observation.longitude}");
            
            // Show confirmation
            ShowSaveConfirmation();
            CloseObservationPanel();
        }

        private void SaveObservationToStorage(FieldObservation observation)
        {
            string json = PlayerPrefs.GetString(OBSERVATIONS_PREFS_KEY, "[]");
            List<FieldObservation> observations = new List<FieldObservation>();
            
            if (!string.IsNullOrEmpty(json) && json != "[]")
            {
                try
                {
                    // Unity's JsonUtility doesn't support List<T> directly, so we wrap it
                    var wrapper = JsonUtility.FromJson<ObservationWrapper>("{\"observations\":" + json + "}");
                    if (wrapper != null) observations = wrapper.observations;
                }
                catch
                {
                    observations = new List<FieldObservation>();
                }
            }

            observations.Add(observation);
            
            var newWrapper = new ObservationWrapper { observations = observations };
            string newJson = JsonUtility.ToJson(newWrapper);
            // Remove wrapper object syntax for storage
            newJson = newJson.Substring(15, newJson.Length - 16); // Remove {"observations": and }
            
            PlayerPrefs.SetString(OBSERVATIONS_PREFS_KEY, newJson);
            PlayerPrefs.Save();
        }

        private void ShowSaveConfirmation()
        {
            // Could show a toast or brief message
            Debug.Log("Observation saved successfully!");
        }

        private void CancelObservation()
        {
            CloseObservationPanel();
        }

        private void RetakePhoto()
        {
            // Would re-open camera for photo capture
            Debug.Log("Retake photo - not implemented yet");
        }

        private void CloseObservationPanel()
        {
            if (observationPanel != null) observationPanel.SetActive(false);
            currentPlant = null;
            currentPhoto = null;
        }

        public List<FieldObservation> GetAllObservations()
        {
            string json = PlayerPrefs.GetString(OBSERVATIONS_PREFS_KEY, "[]");
            if (string.IsNullOrEmpty(json) || json == "[]") return new List<FieldObservation>();

            try
            {
                var wrapper = JsonUtility.FromJson<ObservationWrapper>("{\"observations\":" + json + "}");
                return wrapper?.observations ?? new List<FieldObservation>();
            }
            catch
            {
                return new List<FieldObservation>();
            }
        }

        [System.Serializable]
        private class ObservationWrapper
        {
            public List<FieldObservation> observations = new List<FieldObservation>();
        }
    }
}