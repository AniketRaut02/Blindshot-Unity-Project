using UnityEngine;
using SciFiGame.Core; // Required for GameEvents

namespace SciFiGame.Environment
{
    [RequireComponent(typeof(Collider))]
    public class TutorialTriggerComponent : MonoBehaviour
    {
        [Header("Tutorial Settings")]
        [TextArea]
        [Tooltip("The text to display on the HUD.")]
        [SerializeField] private string _tutorialMessage = "Hold [C] to crouch under the lasers.";

        [Tooltip("How long (in seconds) the UI should stay on screen.")]
        [SerializeField] private float _displayDuration = 4.0f;

        [Header("Configuration")]
        [Tooltip("Check against this tag to ensure only the player triggers it.")]
        [SerializeField] private string _playerTag = "Player";

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // Failsafe so it absolutely only fires once
            if (_hasTriggered) return;

            if (other.CompareTag(_playerTag))
            {
                _hasTriggered = true;

                // Broadcast the message to the UI
                GameEvents.RaiseTutorialPromptTriggered(
                    new TutorialPromptPayload(_tutorialMessage, _displayDuration)
                );

                // Shut off the trigger permanently
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}