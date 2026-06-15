// PlayerStateMachine.cs
// Tracks the player's current PlayerState and raises GameEvents on transitions.

using UnityEngine;
using SciFiGame.Core;
using SciFiGame.Laser;

namespace SciFiGame.Player
{
    public class PlayerStateMachine : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private PlayerMovementComponent _movement;
        [SerializeField] private PlayerInputComponent _input;

        [Tooltip("The GameObject containing the CharacterController/Collider to be protected during QTEs.")]
        [SerializeField] private GameObject _playerRoot;

        private int _defaultLayer;
        private int _invincibleLayer;

        // Tracking variable to keep the shield up during cutscenes
        private bool _isTimelinePlaying = false;

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            if (_playerRoot != null)
            {
                _defaultLayer = _playerRoot.layer;
            }
            else
            {
                Debug.LogWarning("[PlayerStateMachine] _playerRoot is not assigned! Defaulting to layer 0.");
                _defaultLayer = 0;
            }

            _invincibleLayer = LayerMask.NameToLayer("Invincible");

            if (_invincibleLayer == -1)
            {
                Debug.LogError("[PlayerStateMachine] The 'Invincible' layer does not exist. Please add it in Tags and Layers.");
            }
        }

        private void OnEnable()
        {
            GameEvents.OnQTEStarted += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
            GameEvents.OnLaserEntered += OnLaserEntered;
            GameEvents.OnLaserExited += OnLaserExited;

            // FIX: Listen for timeline start/end to hold the invincibility shield
            GameEvents.OnTimelineStarted += OnTimelineStarted;
            GameEvents.OnTimelineFinished += OnTimelineFinished;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
            GameEvents.OnLaserEntered -= OnLaserEntered;
            GameEvents.OnLaserExited -= OnLaserExited;

            GameEvents.OnTimelineStarted -= OnTimelineStarted;
            GameEvents.OnTimelineFinished -= OnTimelineFinished;
        }

        private void Update()
        {
            // Lock out state polling if we are in a QTE, Dead, OR playing a cinematic Timeline
            if (CurrentState == PlayerState.InQTE) return;
            if (CurrentState == PlayerState.Dead) return;
            if (_isTimelinePlaying) return;

            PlayerState next = EvaluateState();
            if (next != CurrentState) Transition(next);
        }

        // ---------------------------------------------------------------------------
        // State evaluation
        // ---------------------------------------------------------------------------

        private PlayerState EvaluateState()
        {
            bool crouching = _movement.IsCrouching;
            bool moving = _movement.IsMoving;

            if (crouching && moving) return PlayerState.CrouchWalking;
            if (crouching) return PlayerState.Crouching;
            if (moving) return PlayerState.Walking;
            return PlayerState.Idle;
        }

        // ---------------------------------------------------------------------------
        // Transition & Protection Logic
        // ---------------------------------------------------------------------------

        private void Transition(PlayerState next)
        {
            PlayerState previous = CurrentState;
            CurrentState = next;

            UpdatePhysicsLayer();

            GameEvents.RaisePlayerStateChanged(new PlayerStateChangedPayload(previous, next));
        }

        // Extracted so we can call it when Timelines start/stop without changing PlayerState
        private void UpdatePhysicsLayer()
        {
            if (_playerRoot != null && _invincibleLayer != -1)
            {
                // Shield stays up if in a QTE, playing a Timeline, or Dead.
                if (CurrentState == PlayerState.InQTE || _isTimelinePlaying || CurrentState == PlayerState.Dead)
                {
                    _playerRoot.layer = _invincibleLayer;
                }
                else
                {
                    _playerRoot.layer = _defaultLayer;
                }
            }
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnQTEStarted(QTEZonePayload _) => Transition(PlayerState.InQTE);
        private void OnQTEEnded(QTEZonePayload _) => Transition(PlayerState.Idle);
        private void OnPlayerRespawned(CheckpointPayload _) => Transition(PlayerState.Idle);

        private void OnLaserEntered() => Transition(PlayerState.InLaserZone);
        private void OnLaserExited() => Transition(PlayerState.Idle);
        private void OnPlayerDied(PlayerDeathPayload _) => Transition(PlayerState.Dead);

        // Timeline Handlers
        private void OnTimelineStarted(TimelinePayload _)
        {
            _isTimelinePlaying = true;
            UpdatePhysicsLayer();
        }

        private void OnTimelineFinished(TimelinePayload _)
        {
            _isTimelinePlaying = false;
            UpdatePhysicsLayer();

            // Just in case they finished a timeline but are floating in the air, let the next Update() figure out the state.
        }
    }
}