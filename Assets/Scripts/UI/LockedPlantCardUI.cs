using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;

namespace NativeLens.UI
{
    /// <summary>
    /// UI component for an undiscovered (locked) plant card in the gallery.
    /// </summary>
    public class LockedPlantCardUI : MonoBehaviour
    {
        [SerializeField] private Image lockIcon;
        [SerializeField] private TextMeshProUGUI mysteryText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button cardButton;

        private Plant plant;
        private System.Action<Plant> onClick;

        public void Setup(Plant plant, System.Action<Plant> onClick)
        {
            this.plant = plant;
            this.onClick = onClick;

            if (mysteryText != null) mysteryText.text = "???";
            if (hintText != null) hintText.text = "NOT FOUND\nLook around the campus";
            
            // lockIcon.sprite = lockSprite; // Set lock icon

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