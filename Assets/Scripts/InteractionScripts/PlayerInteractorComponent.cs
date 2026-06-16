using UnityEngine;
using SciFiGame.Interaction;
using SciFiGame.Core; // For GameEvents

namespace SciFiGame.Player
{
    [RequireComponent(typeof(PlayerInputComponent))]
    public class PlayerInteractorComponent : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _interactionRange = 3.0f;
        [SerializeField] private LayerMask _interactableLayer;

        private PlayerInputComponent _input;
        private IInteractable _currentTarget;

        private void Awake()
        {
            _input = GetComponent<PlayerInputComponent>();
            if (_cameraTransform == null) Debug.Log("CameraNotAssigned");
        }

        private void Update()
        {
            HandleRaycast();

            // Only attempt interaction if we are actually looking at a valid target
            if (_input.InteractPressed && _currentTarget != null)
            {
                _currentTarget.Interact();

                // Clear the prompt immediately after interacting (crucial for items that destroy themselves)
                ClearTarget();
            }
        }

        private void HandleRaycast()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    // If we are looking at a NEW interactable, update the UI
                    if (interactable != _currentTarget)
                    {
                        _currentTarget = interactable;
                        GameEvents.RaiseInteractionPromptChanged(
                            new InteractionPromptPayload(_currentTarget.GetInteractionPrompt(), true)
                        );
                    }
                    return; // Exit out so we don't clear the target below
                }
            }

            // If the raycast hit nothing, or hit something that isn't IInteractable, clear it
            if (_currentTarget != null)
            {
                ClearTarget();
            }
        }

        private void ClearTarget()
        {
            _currentTarget = null;
            // Tell the UI to hide the prompt
            GameEvents.RaiseInteractionPromptChanged(
                new InteractionPromptPayload(string.Empty, false)
            );
        }
    }
}