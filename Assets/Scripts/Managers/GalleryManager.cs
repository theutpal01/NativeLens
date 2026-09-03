using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;
using NativeLens.Managers;
using System.Collections.Generic;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages the Discovery Gallery UI and interactions.
    /// Handles discovered/undiscovered plant cards, progress tracking, and discovery events.
    /// </summary>
    public class GalleryManager : MonoBehaviour
    {
        public static GalleryManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject galleryPanel;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Transform discoveredContainer;
        [SerializeField] private Transform undiscoveredContainer;
        [SerializeField] private GameObject plantCardPrefab;
        [SerializeField] private GameObject lockedCardPrefab;
        [SerializeField] private GameObject discoveryAnimationPanel;
        [SerializeField] private TextMeshProUGUI discoveryPlantNameText;
        [SerializeField] private TextMeshProUGUI discoveryPlantScientificText;
        [SerializeField] private TextMeshProUGUI discoveryProgressText;
        [SerializeField] private Image discoveryPlantImage;

        [Header("Plant Detail Panel")]
        [SerializeField] private GameObject plantDetailPanel;
        [SerializeField] private Image detailPlantImage;
        [SerializeField] private TextMeshProUGUI detailCommonNameText;
        [SerializeField] private TextMeshProUGUI detailScientificNameText;
        [SerializeField] private TextMeshProUGUI detailTamilNameText;
        [SerializeField] private TextMeshProUGUI detailFamilyText;
        [SerializeField] private TextMeshProUGUI detailNativeRegionText;
        [SerializeField] private TextMeshProUGUI detailNativeStatusText;
        [SerializeField] private TextMeshProUGUI detailEcologicalImportanceText;
        [SerializeField] private TextMeshProUGUI detailConservationStatusText;
        [SerializeField] private TextMeshProUGUI detailThreatsText;
        [SerializeField] private TextMeshProUGUI detailConservationActionsText;
        [SerializeField] private TextMeshProUGUI detailIdentifyingFeaturesText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailDiscoveryDateText;
        [SerializeField] private TextMeshProUGUI detailDiscoveryLocationText;
        [SerializeField] private Button detailViewARButton;
        [SerializeField] private Button detailAskGuideButton;
        [SerializeField] private Button detailCloseButton;

        private PlantDataManager plantDataManager;
        private Plant currentDetailPlant;

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
            plantDataManager = PlantDataManager.Instance;
            if (plantDataManager != null)
            {
                plantDataManager.OnDiscoveryProgressChanged += UpdateProgressUI;
                plantDataManager.OnPlantDiscovered += ShowDiscoveryAnimation;
            }

            if (galleryPanel != null) galleryPanel.SetActive(false);
            if (discoveryAnimationPanel != null) discoveryAnimationPanel.SetActive(false);
            if (plantDetailPanel != null) plantDetailPanel.SetActive(false);

            if (detailCloseButton != null) detailCloseButton.onClick.AddListener(ClosePlantDetail);
            if (detailViewARButton != null) detailViewARButton.onClick.AddListener(OnViewARClicked);
            if (detailAskGuideButton != null) detailAskGuideButton.onClick.AddListener(OnAskGuideClicked);
        }

        private void OnDestroy()
        {
            if (plantDataManager != null)
            {
                plantDataManager.OnDiscoveryProgressChanged -= UpdateProgressUI;
                plantDataManager.OnPlantDiscovered -= ShowDiscoveryAnimation;
            }
        }

        public void OpenGallery()
        {
            if (galleryPanel != null) galleryPanel.SetActive(true);
            RefreshGallery();
        }

        public void CloseGallery()
        {
            if (galleryPanel != null) galleryPanel.SetActive(false);
            ClosePlantDetail();
        }

        public void RefreshGallery()
        {
            if (plantDataManager == null) return;

            // Clear existing cards
            ClearContainer(discoveredContainer);
            ClearContainer(undiscoveredContainer);

            // Update progress
            UpdateProgressUI(plantDataManager.DiscoveredCount, plantDataManager.TotalPlants);

            // Create discovered plant cards
            var discoveredPlants = plantDataManager.GetDiscoveredPlants();
            foreach (var plant in discoveredPlants)
            {
                CreateDiscoveredCard(plant);
            }

            // Create undiscovered plant cards
            var undiscoveredPlants = plantDataManager.GetUndiscoveredPlants();
            foreach (var plant in undiscoveredPlants)
            {
                CreateUndiscoveredCard(plant);
            }
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateDiscoveredCard(Plant plant)
        {
            GameObject card = Instantiate(plantCardPrefab, discoveredContainer);
            if (card == null) return;

            var cardUI = card.GetComponent<PlantCardUI>();
            if (cardUI != null)
            {
                var state = plantDataManager.GetDiscoveryState(plant.id);
                cardUI.Setup(plant, state, true, OnPlantCardClicked);
            }
        }

        private void CreateUndiscoveredCard(Plant plant)
        {
            GameObject card = Instantiate(lockedCardPrefab, undiscoveredContainer);
            if (card == null) return;

            var cardUI = card.GetComponent<LockedPlantCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(plant, OnLockedCardClicked);
            }
        }

        private void OnPlantCardClicked(Plant plant)
        {
            ShowPlantDetail(plant);
        }

        private void OnLockedCardClicked(Plant plant)
        {
            // Show hint for undiscovered plant
            ShowLockedPlantHint(plant);
        }

        private void ShowPlantDetail(Plant plant)
        {
            currentDetailPlant = plant;
            if (plantDetailPanel == null) return;

            var state = plantDataManager.GetDiscoveryState(plant.id);

            // Load plant image (placeholder - would load from Resources or Addressables)
            // detailPlantImage.sprite = LoadPlantSprite(plant.id);

            detailCommonNameText.text = plant.commonName;
            detailScientificNameText.text = plant.scientificName;
            detailTamilNameText.text = plant.tamilName;
            detailFamilyText.text = $"Family: {plant.family}";
            detailNativeRegionText.text = $"Native to: {plant.nativeRegion}";
            detailNativeStatusText.text = $"Status: {plant.nativeStatus}";
            detailEcologicalImportanceText.text = plant.ecologicalImportance;
            detailConservationStatusText.text = $"Conservation: {plant.conservationStatus}";
            detailThreatsText.text = plant.threats;
            detailConservationActionsText.text = plant.conservationActions;
            detailIdentifyingFeaturesText.text = plant.identifyingFeatures;
            detailDescriptionText.text = plant.description;

            if (state.IsDiscovered && state.discoveredAt.HasValue)
            {
                detailDiscoveryDateText.text = $"Discovered: {state.discoveredAt.Value:dd MMM yyyy}";
                detailDiscoveryLocationText.text = string.IsNullOrEmpty(state.discoveryLocation) 
                    ? "Location: Not recorded" 
                    : $"Location: {state.discoveryLocation}";
            }
            else
            {
                detailDiscoveryDateText.text = "";
                detailDiscoveryLocationText.text = "";
            }

            // Show AR/Guide buttons only for discovered plants
            if (detailViewARButton != null) detailViewARButton.gameObject.SetActive(state.IsDiscovered);
            if (detailAskGuideButton != null) detailAskGuideButton.gameObject.SetActive(state.IsDiscovered);

            plantDetailPanel.SetActive(true);
        }

        private void ClosePlantDetail()
        {
            if (plantDetailPanel != null) plantDetailPanel.SetActive(false);
            currentDetailPlant = null;
        }

        private void OnViewARClicked()
        {
            if (currentDetailPlant != null)
            {
                ClosePlantDetail();
                CloseGallery();
                ARManager.Instance?.LaunchARForPlant(currentDetailPlant);
            }
        }

        private void OnAskGuideClicked()
        {
            if (currentDetailPlant != null)
            {
                ClosePlantDetail();
                BotanicalGuideManager.Instance?.OpenGuideForPlant(currentDetailPlant);
            }
        }

        private void ShowLockedPlantHint(Plant plant)
        {
            // Simple hint - could be a toast or modal
            Debug.Log($"Hint for {plant.commonName}: Look around the campus!");
            // Could show a small toast: "Look around the campus to discover this species!"
        }

        private void UpdateProgressUI(int discovered, int total)
        {
            if (progressText != null)
                progressText.text = $"NATIVE SPECIES DISCOVERED\n{discovered} / {total}";

            if (progressBar != null)
                progressBar.value = total > 0 ? (float)discovered / total : 0f;
        }

        private void ShowDiscoveryAnimation(string plantId)
        {
            var plant = plantDataManager.GetPlant(plantId);
            if (plant == null || discoveryAnimationPanel == null) return;

            discoveryPlantNameText.text = plant.commonName;
            discoveryPlantScientificText.text = plant.scientificName;
            discoveryProgressText.text = $"{plantDataManager.DiscoveredCount} / {plantDataManager.TotalPlants}";
            // discoveryPlantImage.sprite = LoadPlantSprite(plantId);

            discoveryAnimationPanel.SetActive(true);
            
            // Animate in
            StartCoroutine(AnimateDiscovery());
        }

        private System.Collections.IEnumerator AnimateDiscovery()
        {
            var canvasGroup = discoveryAnimationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = discoveryAnimationPanel.AddComponent<CanvasGroup>();

            // Fade in
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }

            // Wait
            yield return new WaitForSeconds(3f);

            // Fade out
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
                yield return null;
            }

            discoveryAnimationPanel.SetActive(false);
        }
    }
}