using UnityEngine;
using TMPro; // Required for TextMeshPro
using SciFiGame.Core;

namespace SciFiGame.UI
{
    public class InteractionUIComponent : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The parent GameObject holding the text background/container")]
        [SerializeField] private GameObject _promptContainer;
        [Tooltip("The actual text element to display 'Press E to...'")]
        [SerializeField] private TextMeshProUGUI _promptText;

        private void OnEnable()
        {
            GameEvents.OnInteractionPromptChanged += OnPromptChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnInteractionPromptChanged -= OnPromptChanged;
        }

        private void Start()
        {
            // Make sure it starts hidden
            if (_promptContainer != null)
            {
                _promptContainer.SetActive(false);
            }
        }

        private void OnPromptChanged(InteractionPromptPayload payload)
        {
            if (_promptContainer == null || _promptText == null) return;

            if (payload.IsVisible)
            {
                // Format the text. You can hardcode the "E" here or pull it from an input manager.
                _promptText.text = $"[E] {payload.PromptText}";
                _promptContainer.SetActive(true);
            }
            else
            {
                _promptContainer.SetActive(false);
            }
        }
    }
}