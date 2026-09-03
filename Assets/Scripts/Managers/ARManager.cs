using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using NativeLens.Models;
using System.Collections.Generic;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages AR session, plane detection, and AR information card placement.
    /// Phase 1 & 2: AR Foundation setup and AR UI.
    /// </summary>
    public class ARManager : MonoBehaviour
    {
        public static ARManager Instance { get; private set; }

        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;

        [Header("AR UI")]
        [SerializeField] private GameObject arInfoCardPrefab;
        [SerializeField] private Transform arUICanvas;
        [SerializeField] private GameObject scanInstructionPanel;
        [SerializeField] private TextMeshProUGUI scanInstructionText;
        [SerializeField] private GameObject analysingPanel;
        [SerializeField] private GameObject unableToIdentifyPanel;
        [SerializeField] private Button retryButton;

        [Header("AR Info Card Prefab References")]
        [SerializeField] private TextMeshProUGUI arCommonNameText;
        [SerializeField] private TextMeshProUGUI arScientificNameText;
        [SerializeField] private TextMeshProUGUI arFamilyText;
        [SerializeField] private TextMeshProUGUI arNativeStatusText;
        [SerializeField] private TextMeshProUGUI arEcologicalImportanceText;
        [SerializeField] private TextMeshProUGUI arConservationStatusText;
        [SerializeField] private TextMeshProUGUI arConfidenceText;
        [SerializeField] private Button arLearnMoreButton;
        [SerializeField] private Button arEcologyButton;
        [SerializeField] private Button arConservationButton;
        [SerializeField] private Button arAskGuideButton;
        [SerializeField] private Button arAddToGalleryButton;
        [SerializeField] private Button arSaveObservationButton;
        [SerializeField] private Button arCloseButton;

        private Plant currentARPlant;
        private GameObject currentARInfoCard;
        private ARAnchor currentARAnchor;
        private bool isARActive = false;

        public event System.Action<Plant> OnARInfoCardPlaced;
        public event System.Action OnARSessionReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to AR events
            if (arSession != null)
            {
                arSession.stateChanged += OnARSessionStateChanged;
            }

            if (planeManager != null)
            {
                planeManager.planesChanged += OnPlanesChanged;
            }

            // Setup UI buttons
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (arLearnMoreButton != null) arLearnMoreButton.onClick.AddListener(() => OnARActionClicked("learn"));
            if (arEcologyButton != null) arEcologyButton.onClick.AddListener(() => OnARActionClicked("ecology"));
            if (arConservationButton != null) arConservationButton.onClick.AddListener(() => OnARActionClicked("conservation"));
            if (arAskGuideButton != null) arAskGuideButton.onClick.AddListener(() => OnARActionClicked("guide"));
            if (arAddToGalleryButton != null) arAddToGalleryButton.onClick.AddListener(OnAddToGalleryClicked);
            if (arSaveObservationButton != null) arSaveObservationButton.onClick.AddListener(OnSaveObservationClicked);
            if (arCloseButton != null) arCloseButton.onClick.AddListener(CloseARInfoCard);

            // Initial UI state
            SetScanInstruction("Point camera at a plant\nTap to capture");
            ShowScanUI(true);
        }

        private void OnDestroy()
        {
            if (arSession != null) arSession.stateChanged -= OnARSessionStateChanged;
            if (planeManager != null) planeManager.planesChanged -= OnPlanesChanged;
        }

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            Debug.Log($"AR Session State: {args.state}");
            
            if (args.state == ARSessionState.SessionTracking)
            {
                OnARSessionReady?.Invoke();
                SetScanInstruction("AR Ready - Point at a plant");
            }
            else if (args.state == ARSessionState.None || args.state == ARSessionState.CheckingAvailability)
            {
                SetScanInstruction("Initializing AR...");
            }
            else if (args.state == ARSessionState.Ready)
            {
                SetScanInstruction("AR Ready - Tap to start");
            }
            else if (args.state == ARSessionState.SessionInitializing)
            {
                SetScanInstruction("Starting AR session...");
            }
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            // Planes detected - we can place content
            if (args.added.Count > 0 && scanInstructionPanel != null && scanInstructionPanel.activeSelf)
            {
                SetScanInstruction("Surface detected - Point at a plant");
            }
        }

        public void SetScanInstruction(string instruction)
        {
            if (scanInstructionText != null)
                scanInstructionText.text = instruction;
        }

        public void ShowScanUI(bool show)
        {
            if (scanInstructionPanel != null) scanInstructionPanel.SetActive(show);
            if (analysingPanel != null) analysingPanel.SetActive(!show);
            if (unableToIdentifyPanel != null) unableToIdentifyPanel.SetActive(false);
        }

        public void ShowAnalysingUI()
        {
            if (scanInstructionPanel != null) scanInstructionPanel.SetActive(false);
            if (analysingPanel != null) analysingPanel.SetActive(true);
            if (unableToIdentifyPanel != null) unableToIdentifyPanel.SetActive(false);
            SetScanInstruction("Analysing plant...");
        }

        public void ShowUnableToIdentify()
        {
            if (scanInstructionPanel != null) scanInstructionPanel.SetActive(false);
            if (analysingPanel != null) analysingPanel.SetActive(false);
            if (unableToIdentifyPanel != null) unableToIdentifyPanel.SetActive(true);
            SetScanInstruction("Unable to identify - Try again");
        }

        private void OnRetryClicked()
        {
            ShowScanUI(true);
            // Re-enable camera capture
            PlantIdentificationManager.Instance?.ResetCapture();
        }

        /// <summary>
        /// Places AR information card at the tapped screen position.
        /// Uses raycast against detected planes.
        /// </summary>
        public void PlaceARInfoCardAtScreenPosition(Vector2 screenPosition, Plant plant, float confidence)
        {
            if (raycastManager == null || arInfoCardPrefab == null) return;

            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = hits[0];
                PlaceARInfoCard(hit.pose, plant, confidence);
            }
            else
            {
                // Fallback: place at fixed distance from camera
                var cameraTransform = Camera.main.transform;
                Pose pose = new Pose(
                    cameraTransform.position + cameraTransform.forward * 1.5f,
                    Quaternion.LookRotation(-cameraTransform.forward)
                );
                PlaceARInfoCard(pose, plant, confidence);
            }
        }

        private void PlaceARInfoCard(Pose pose, Plant plant, float confidence)
        {
            currentARPlant = plant;

            // Remove existing card
            if (currentARInfoCard != null)
            {
                Destroy(currentARInfoCard);
            }
            if (currentARAnchor != null)
            {
                Destroy(currentARAnchor.gameObject);
            }

            // Create anchor at hit position
            currentARAnchor = anchorManager.AddAnchor(pose);
            if (currentARAnchor == null) return;

            // Instantiate info card
            currentARInfoCard = Instantiate(arInfoCardPrefab, currentARAnchor.transform);
            currentARInfoCard.transform.localPosition = Vector3.zero;
            currentARInfoCard.transform.localRotation = Quaternion.identity;

            // Scale appropriately
            currentARInfoCard.transform.localScale = Vector3.one * 0.01f; // Adjust for world scale

            // Populate card content
            PopulateARInfoCard(plant, confidence);

            // Make card face camera
            StartCoroutine(FaceCameraRoutine());

            isARActive = true;
            OnARInfoCardPlaced?.Invoke(plant);
        }

        private void PopulateARInfoCard(Plant plant, float confidence)
        {
            if (currentARInfoCard == null) return;

            // Get text components from the instantiated prefab
            var texts = currentARInfoCard.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                switch (text.name.ToLower())
                {
                    case "commonnametext":
                        text.text = plant.commonName;
                        break;
                    case "scientificnametext":
                        text.text = plant.scientificName;
                        break;
                    case "familytext":
                        text.text = $"Family: {plant.family}";
                        break;
                    case "nativetext":
                        text.text = $"🟢 {plant.nativeStatus} to {plant.nativeRegion}";
                        break;
                    case "ecologicaltext":
                        text.text = plant.ecologicalImportance;
                        break;
                    case "conservationtext":
                        text.text = $"Conservation: {plant.conservationStatus}";
                        break;
                    case "confidencetext":
                        text.text = $"AI Confidence: {confidence:P0}";
                        break;
                }
            }

            // Show card with animation
            var canvasGroup = currentARInfoCard.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = currentARInfoCard.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeInCanvasGroup(canvasGroup, 0.3f));
        }

        private System.Collections.IEnumerator FaceCameraRoutine()
        {
            var cameraTransform = Camera.main.transform;
            while (currentARInfoCard != null && isARActive)
            {
                if (cameraTransform != null)
                {
                    Vector3 direction = cameraTransform.position - currentARInfoCard.transform.position;
                    direction.y = 0; // Keep upright
                    if (direction != Vector3.zero)
                    {
                        currentARInfoCard.transform.rotation = Quaternion.LookRotation(-direction);
                    }
                }
                yield return null;
            }
        }

        private System.Collections.IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && cg != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }
            if (cg != null) cg.alpha = 1f;
        }

        public void CloseARInfoCard()
        {
            isARActive = false;
            if (currentARInfoCard != null)
            {
                Destroy(currentARInfoCard);
                currentARInfoCard = null;
            }
            if (currentARAnchor != null)
            {
                Destroy(currentARAnchor.gameObject);
                currentARAnchor = null;
            }
            currentARPlant = null;
            ShowScanUI(true);
        }

        private void OnARActionClicked(string action)
        {
            if (currentARPlant == null) return;

            switch (action)
            {
                case "learn":
                    // Show full plant details
                    GalleryManager.Instance?.ShowPlantDetail(currentARPlant);
                    break;
                case "ecology":
                    // Could open ecology detail panel
                    Debug.Log($"Show ecology for {currentARPlant.commonName}");
                    break;
                case "conservation":
                    // Could open conservation detail panel
                    Debug.Log($"Show conservation for {currentARPlant.commonName}");
                    break;
                case "guide":
                    BotanicalGuideManager.Instance?.OpenGuideForPlant(currentARPlant);
                    break;
            }
        }

        private void OnAddToGalleryClicked()
        {
            if (currentARPlant != null)
            {
                var plantDataManager = PlantDataManager.Instance;
                bool isNewDiscovery = plantDataManager.MarkDiscovered(currentARPlant.id);
                CloseARInfoCard();
            }
        }

        private void OnSaveObservationClicked()
        {
            if (currentARPlant != null)
            {
                ObservationManager.Instance?.SaveObservation(currentARPlant);
                CloseARInfoCard();
            }
        }

        /// <summary>
        /// Launch AR view for a specific plant (from Gallery).
        /// </summary>
        public void LaunchARForPlant(Plant plant)
        {
            // This would re-open the AR view with the plant pre-loaded
            // For now, just open the scanner with the plant info ready
            Debug.Log($"Launch AR for {plant.commonName}");
            // Could navigate to AR scanner screen with plant pre-selected
        }

        public bool IsARActive => isARActive;
        public Plant CurrentARPlant => currentARPlant;
    }
}