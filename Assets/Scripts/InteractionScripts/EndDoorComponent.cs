using UnityEngine;
using SciFiGame.Core; // Required to access GameEvents

namespace SciFiGame.Interaction
{
    public class EndDoorComponent : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private string _promptText = "Open Door (Escape)";

        public string GetInteractionPrompt()
        {
            return _promptText;
        }

        public void Interact()
        {
            // Safely raise the event through your helper method
            GameEvents.RaiseGameOver();
        }
    }
}