using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.Heat
{
    public class CreditsMentionItem : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        public VerticalLayoutGroup listLayout;

        // Helpers
        [HideInInspector] public CreditsPreset preset;

        public void UpdateLayout(int paddingValue, int spacingValue)
        {
            listLayout.padding.top = paddingValue / 2;
            listLayout.padding.bottom = paddingValue / 2;
            listLayout.spacing = spacingValue;
        }

        public void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                iconImage.gameObject.SetActive(false);
                return;
            }

            iconImage.sprite = icon;
        }

        public void SetDescription(string text)
        {
            if (string.IsNullOrEmpty(text)) 
            {
                descriptionText.gameObject.SetActive(false);
                return;
            }

            descriptionText.text = text;
        }
    }
}