using UnityEngine;
using UnityEngine.UI;
using NativeLens.Models;
using NativeLens.Managers;
using System.Collections;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages plant identification - camera capture and AI inference.
    /// Phase 4: Connect camera/image input to identification system.
    /// </summary>
    public class PlantIdentificationManager : MonoBehaviour
    {
        public static PlantIdentificationManager Instance { get; private set; }

        [Header("Camera")]
        [SerializeField] private RawImage cameraPreview;
        [SerializeField] private AspectRatioFitter cameraFitter;
        [SerializeField] private Button captureButton;
        [SerializeField] private GameObject cameraPanel;

        [Header("Identification Settings")]
        [SerializeField] private float minConfidenceThreshold = 0.7f;
        [SerializeField] private bool useMockIdentification = true; // For development without AI backend

        private WebCamTexture webCamTexture;
        private bool isCameraActive = false;
        private bool isProcessing = false;

        public event System.Action<IdentificationResult> OnIdentificationComplete;

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
            if (captureButton != null)
                captureButton.onClick.AddListener(CaptureAndIdentify);

            // Start camera automatically
            StartCamera();
        }

        private void OnDestroy()
        {
            StopCamera();
        }

        public void StartCamera()
        {
            if (isCameraActive) return;

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("No camera devices found!");
                return;
            }

            // Use back camera (index 0 is usually back on mobile)
            string deviceName = WebCamTexture.devices[0].name;
            webCamTexture = new WebCamTexture(deviceName, Screen.width, Screen.height, 30);
            
            if (cameraPreview != null)
            {
                cameraPreview.texture = webCamTexture;
                cameraPreview.material.mainTexture = webCamTexture;
            }

            webCamTexture.Play();
            isCameraActive = true;

            StartCoroutine(UpdateCameraPreview());
        }

        public void StopCamera()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
            isCameraActive = false;
        }

        private IEnumerator UpdateCameraPreview()
        {
            yield return new WaitForEndOfFrame();
            
            while (isCameraActive && webCamTexture != null && webCamTexture.isPlaying)
            {
                if (cameraPreview != null && cameraFitter != null)
                {
                    float videoRatio = (float)webCamTexture.width / webCamTexture.height;
                    cameraFitter.aspectRatio = videoRatio;
                    
                    // Handle rotation
                    float rotation = -webCamTexture.videoRotationAngle;
                    cameraPreview.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public void CaptureAndIdentify()
        {
            if (isProcessing || webCamTexture == null || !webCamTexture.isPlaying) return;

            isProcessing = true;
            ARManager.Instance?.ShowAnalysingUI();

            // Capture current frame
            Texture2D capturedImage = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            capturedImage.SetPixels(webCamTexture.GetPixels());
            capturedImage.Apply();

            // Process identification
            if (useMockIdentification)
            {
                StartCoroutine(MockIdentificationRoutine(capturedImage));
            }
            else
            {
                StartCoroutine(RealIdentificationRoutine(capturedImage));
            }
        }

        private IEnumerator MockIdentificationRoutine(Texture2D image)
        {
            // Simulate network delay
            yield return new WaitForSeconds(1.5f);

            // Mock: randomly select one of the 7 plants
            var plantDatabase = PlantDataManager.Instance;
            var plants = plantDatabase?.GetUndiscoveredPlants();
            
            Plant selectedPlant;
            if (plants != null && plants.Count > 0)
            {
                selectedPlant = plants[Random.Range(0, plants.Count)];
            }
            else
            {
                // All discovered, pick random
                var allPlants = plantDatabase.Plants;
                selectedPlant = allPlants[Random.Range(0, allPlants.Count)];
            }

            float confidence = Random.Range(0.85f, 0.98f);

            var result = new IdentificationResult
            {
                speciesId = selectedPlant.id,
                scientificName = selectedPlant.scientificName,
                confidence = confidence,
                success = confidence >= minConfidenceThreshold,
                errorMessage = confidence < minConfidenceThreshold ? "Low confidence" : ""
            };

            OnIdentificationComplete?.Invoke(result);
            isProcessing = false;
        }

        private IEnumerator RealIdentificationRoutine(Texture2D image)
        {
            // TODO: Implement actual AI inference call
            // This would send the image to your AI service (custom model, cloud API, etc.)
            // For now, fall back to mock
            yield return MockIdentificationRoutine(image);
        }

        public void ResetCapture()
        {
            isProcessing = false;
            ARManager.Instance?.ShowScanUI(true);
        }

        public void ShowCameraPanel(bool show)
        {
            if (cameraPanel != null) cameraPanel.SetActive(show);
        }
    }
}