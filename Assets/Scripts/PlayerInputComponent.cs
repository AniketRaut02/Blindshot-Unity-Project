// PlayerInputComponent.cs
// Translates InputReader events into player-local input state.
//
// DESIGN RATIONALE:
// This component is the only part of the player hierarchy that knows about the
// InputReader. All other player components (Movement, Rotation, Animation) read
// their data from this one, not from the InputReader directly. This means:
//   - Replacing the input source (replay system, AI) only requires changing this file.
//   - Other components are pure data processors with no input coupling.
//
// The component stores a frame-coherent snapshot of input (MoveInput, LookInput)
// rather than forwarding raw events. Components that run in FixedUpdate (physics)
// and those that run in Update (animation) both read the same snapshot safely.
//
// Input is disabled during QTEs by listening to the event bus and clearing the
// snapshot. This is the authoritative disable point — we do NOT change the
// InputReader's active map here; that is the QTEManager's responsibility.

using UnityEngine;
using SciFiGame.Input;
using SciFiGame.Core;

namespace SciFiGame.Player
{
    public class PlayerInputComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private InputReader _inputReader;

        // ---------------------------------------------------------------------------
        // Public state (read by sibling components each frame)
        // ---------------------------------------------------------------------------

        public Vector2 MoveInput       { get; private set; }
        public Vector2 LookInput       { get; private set; }
        public bool    CrouchHeld      { get; private set; }
        public bool    InteractPressed { get; private set; }  // consumed in same frame

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private bool _inputEnabled = true;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            _inputReader.OnMoveInput     += OnMove;
            _inputReader.OnLookInput     += OnLook;
            _inputReader.OnCrouchInput   += OnCrouch;
            _inputReader.OnInteractInput += OnInteract;

            GameEvents.OnQTEStarted   += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed    += OnQTEEnded;
        }

        private void OnDisable()
        {
            _inputReader.OnMoveInput     -= OnMove;
            _inputReader.OnLookInput     -= OnLook;
            _inputReader.OnCrouchInput   -= OnCrouch;
            _inputReader.OnInteractInput -= OnInteract;

            GameEvents.OnQTEStarted   -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed    -= OnQTEEnded;
        }

        private void LateUpdate()
        {
            // Clear single-frame flags after all components have had a chance to read them.
            InteractPressed = false;
        }

        // ---------------------------------------------------------------------------
        // Input callbacks
        // ---------------------------------------------------------------------------

        private void OnMove(Vector2 value)     => MoveInput  = _inputEnabled ? value : Vector2.zero;
        private void OnLook(Vector2 value)     => LookInput  = _inputEnabled ? value : Vector2.zero;
        private void OnCrouch(bool held)       => CrouchHeld = _inputEnabled && held;

        private void OnInteract()
        {
            if (_inputEnabled) InteractPressed = true;
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnQTEStarted(QTEZonePayload _)
        {
            _inputEnabled = false;
            MoveInput     = Vector2.zero;
            LookInput     = Vector2.zero;
            CrouchHeld    = false;
        }

        private void OnQTEEnded(QTEZonePayload _) => _inputEnabled = true;
    }
}
