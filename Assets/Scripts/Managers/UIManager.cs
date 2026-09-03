using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;
using NativeLens.Managers;

namespace NativeLens.Managers
{
    /// <summary>
    /// Main UI Manager handling navigation between Home, Scan, and Gallery screens.
    /// Implements the 3-tab navigation from the spec.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screen Panels")]
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject scanScreen;
        [SerializeField] private GameObject galleryScreen;

        [Header("Navigation Bar")]
        [SerializeField] private Button homeTabButton;
        [SerializeField] private Button scanTabButton;
        [SerializeField] private Button galleryTabButton;
        [SerializeField] private Image homeTabIcon;
        [SerializeField] private Image scanTabIcon;
        [SerializeField] private Image galleryTabIcon;
        [SerializeField] private Color activeTabColor = Color.white;
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("Home Screen")]
        [SerializeField] private TextMeshProUGUI homeProgressText;
        [SerializeField>] private Slider homeProgressBar;
        [SerializeField] private Button homeStartScanButton;
        [SerializeField] private Button homeOpenGalleryButton;

        [Header("Scan Screen")]
        [SerializeField] private Button scanBackButton;

        [Header("Gallery Screen")]
        [SerializeField] private Button galleryBackButton;

        private NavigationTab currentTab = NavigationTab.Home;
        private PlantDataManager plantDataManager;

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

            // Setup navigation buttons
            if (homeTabButton != null) homeTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Home));
            if (scanTabButton != null) scanTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Scan));
            if (galleryTabButton != null) galleryTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Gallery));

            // Home screen buttons
            if (homeStartScanButton != null) homeStartScanButton.onClick.AddListener(() => SwitchTab(NavigationTab.Scan));
            if (homeOpenGalleryButton != null) homeOpenGalleryButton.onClick.AddListener(() => SwitchTab(NavigationTab.Gallery));

            // Back buttons
            if (scanBackButton != null) scanBackButton.onClick.AddListener(() => SwitchTab(NavigationTab.Home));
            if (galleryBackButton != null) galleryBackButton.onClick.AddListener(() => SwitchTab(NavigationTab.Home));

            // Subscribe to discovery updates
            if (plantDataManager != null)
            {
                plantDataManager.OnDiscoveryProgressChanged += UpdateHomeProgress;
            }

            // Initial state
            SwitchTab(NavigationTab.Home);
        }

        private void OnDestroy()
        {
            if (plantDataManager != null)
            {
                plantDataManager.OnDiscoveryProgressChanged -= UpdateHomeProgress;
            }
        }

        public void SwitchTab(NavigationTab tab)
        {
            currentTab = tab;

            // Update screen visibility
            if (homeScreen != null) homeScreen.SetActive(tab == NavigationTab.Home);
            if (scanScreen != null) scanScreen.SetActive(tab == NavigationTab.Scan);
            if (galleryScreen != null) galleryScreen.SetActive(tab == NavigationTab.Gallery);

            // Update tab icons
            UpdateTabIcons();

            // Handle screen-specific logic
            switch (tab)
            {
                case NavigationTab.Home:
                    UpdateHomeProgress(plantDataManager?.DiscoveredCount ?? 0, plantDataManager?.TotalPlants ?? 0);
                    break;
                case NavigationTab.Scan:
                    ARManager.Instance?.ShowScanUI(true);
                    PlantIdentificationManager.Instance?.ShowCameraPanel(true);
                    break;
                case NavigationTab.Gallery:
                    GalleryManager.Instance?.OpenGallery();
                    break;
            }
        }

        private void UpdateTabIcons()
        {
            if (homeTabIcon != null) homeTabIcon.color = currentTab == NavigationTab.Home ? activeTabColor : inactiveTabColor;
            if (scanTabIcon != null) scanTabIcon.color = currentTab == NavigationTab.Scan ? activeTabColor : inactiveTabColor;
            if (galleryTabIcon != null) galleryTabIcon.color = currentTab == NavigationTab.Gallery ? activeTabColor : inactiveTabColor;
        }

        private void UpdateHomeProgress(int discovered, int total)
        {
            if (homeProgressText != null)
                homeProgressText.text = $"{discovered} / {total} Species Discovered";
            
            if (homeProgressBar != null)
                homeProgressBar.value = total > 0 ? (float)discovered / total : 0f;
        }

        public NavigationTab CurrentTab => currentTab;
    }
}