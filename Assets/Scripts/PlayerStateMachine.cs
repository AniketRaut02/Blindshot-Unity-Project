// PlayerStateMachine.cs
// Tracks the player's current PlayerState and raises GameEvents on transitions.
//
// DESIGN RATIONALE:
// A dedicated state machine component prevents state drift — without it, multiple
// components make independent decisions about what state the player is in, which
// leads to contradictions (MovementComponent thinks "walking", AnimationComponent
// thinks "idle"). Centralising state here means there is one authoritative answer.
//
// WHY NOT A FULL HSM OR BEHAVIOUR TREE?
// For Phase 1 the state space is small (Idle, Walking, Crouching, CrouchWalking,
// InQTE, Dead). A simple enum + transition guard is sufficient and vastly easier
// to reason about. If Phase 2 grows the state space significantly, migrate to a
// proper HSM at that point — not before.
//
// TRANSITION LOGIC:
// The state machine reads from sibling components (MovementComponent, InputComponent)
// each frame and determines the correct state. It does not hold references to
// QTEManager or CheckpointManager — it learns about QTE lifecycle through events.
//
// Raising OnPlayerStateChanged lets any system (UI, audio, VFX) react to state
// changes without needing to poll or reference this component.

using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Player
{
    public class PlayerStateMachine : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private PlayerMovementComponent _movement;
        [SerializeField] private PlayerInputComponent    _input;

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            GameEvents.OnQTEStarted      += OnQTEStarted;
            GameEvents.OnQTESucceeded    += OnQTEEnded;
            GameEvents.OnQTEFailed       += OnQTEFailed;
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
            GameEvents.OnLaserEntered += OnLaserEntered;
            GameEvents.OnLaserExited += OnLaserExited;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted      -= OnQTEStarted;
            GameEvents.OnQTESucceeded    -= OnQTEEnded;
            GameEvents.OnQTEFailed       -= OnQTEFailed;
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
            GameEvents.OnLaserEntered -= OnLaserEntered;
            GameEvents.OnLaserExited -= OnLaserExited;
        }

        private void Update()
        {
            // QTE and Dead states are driven by events, not frame polling.
            if (CurrentState == PlayerState.InQTE) return;
            if (CurrentState == PlayerState.Dead)  return;

            PlayerState next = EvaluateState();
            if (next != CurrentState) Transition(next);
        }

        // ---------------------------------------------------------------------------
        // State evaluation
        // ---------------------------------------------------------------------------

        private PlayerState EvaluateState()
        {
            bool crouching = _movement.IsCrouching;
            bool moving    = _movement.IsMoving;

            if (crouching && moving)  return PlayerState.CrouchWalking;
            if (crouching)            return PlayerState.Crouching;
            if (moving)               return PlayerState.Walking;
            return PlayerState.Idle;
        }

        // ---------------------------------------------------------------------------
        // Transition
        // ---------------------------------------------------------------------------

        private void Transition(PlayerState next)
        {
            PlayerState previous = CurrentState;
            CurrentState         = next;
            GameEvents.RaisePlayerStateChanged(new PlayerStateChangedPayload(previous, next));
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnQTEStarted(QTEZonePayload _)   => Transition(PlayerState.InQTE);
        private void OnQTEEnded(QTEZonePayload _)      => Transition(PlayerState.Idle);
        private void OnQTEFailed(QTEZonePayload _)     => Transition(PlayerState.Dead);
        private void OnPlayerRespawned(CheckpointPayload _) => Transition(PlayerState.Idle);


        private void OnLaserEntered()
        {
            Transition(PlayerState.InLaserZone);
        }

        private void OnLaserExited()
        {
            Transition(PlayerState.Idle);
        }
    }
}
