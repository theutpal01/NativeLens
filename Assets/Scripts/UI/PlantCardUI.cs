using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;

namespace NativeLens.UI
{
    /// <summary>
    /// UI component for a discovered plant card in the gallery.
    /// </summary>
    public class PlantCardUI : MonoBehaviour
    {
        [SerializeField] private Image plantImage;
        [SerializeField] private TextMeshProUGUI commonNameText;
        [SerializeField] private TextMeshProUGUI scientificNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI discoveryDateText;
        [SerializeField] private Button cardButton;

        private Plant plant;
        private PlantDiscoveryState state;
        private System.Action<Plant> onClick;

        public void Setup(Plant plant, PlantDiscoveryState state, bool isDiscovered, System.Action<Plant> onClick)
        {
            this.plant = plant;
            this.state = state;
            this.onClick = onClick;

            if (commonNameText != null) commonNameText.text = plant.commonName;
            if (scientificNameText != null) scientificNameText.text = plant.scientificName;
            if (statusText != null) statusText.text = "✓ DISCOVERED";
            if (discoveryDateText != null && state.discoveredAt.HasValue)
                discoveryDateText.text = $"Discovered: {state.discoveredAt.Value:dd MMM yyyy}";

            // plantImage.sprite = LoadPlantSprite(plant.id); // Placeholder

            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(() => onClick?.Invoke(plant));
            }
        }

        private void OnDestroy()
        {
            if (cardButton != null) cardButton.onClick.RemoveAllListeners();
        }
    }
}