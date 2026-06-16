// PlayerMovementComponent.cs
// Handles camera-relative translation and CharacterController collider management.
//
// DESIGN RATIONALE:
// Movement is intentionally separated from rotation. This component only cares
// about "how fast and in which world-space direction does the character move".
// It does NOT rotate the character body — that is RotationComponent's job.
//
// WHY CAMERA-RELATIVE?
// RE4-style movement is camera-relative: pressing forward moves toward the camera's
// forward direction projected onto the XZ plane. This feels natural over the
// shoulder and does not require input remapping when the player rotates the camera.
//
// WHY CHARACTERCONTROLLER AND NOT RIGIDBODY?
// CharacterController gives deterministic step/slope handling with no physics
// tunnelling risk at the speeds used here. It avoids the complexity of continuous
// collision detection tuning and friction materials. If physics interactions with
// dynamic objects become important in Phase 2, revisit this decision.
//
// Crouch transitions lerp the collider height and center simultaneously to
// prevent the character from clipping through ceilings when standing up.
// An additional ceiling raycast guards against standing inside geometry.

using UnityEngine;
using SciFiGame.Core;
using SciFiGame.Laser;

namespace SciFiGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private MovementData       _data;
        [SerializeField] private PlayerInputComponent _input;
        [SerializeField] private Transform          _cameraRoot;

        // ---------------------------------------------------------------------------
        // Public state (read by AnimationComponent, StateMachine)
        // ---------------------------------------------------------------------------

        public Vector3 WorldVelocity    { get; private set; }
        public float   HorizontalSpeed  { get; private set; }
        public bool    IsCrouching      { get; private set; }
        public bool    IsMoving         => HorizontalSpeed > 0.05f;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private CharacterController _cc;
        private Vector3 _currentVelocity;
        private float   _verticalVelocity;
        private float   _crouchBlend;           // 0 = stand, 1 = crouch
        private bool    _movementEnabled = true;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.height = _data.StandHeight;
            _cc.center = new Vector3(0f, _data.StandCenterY, 0f);
        }

        private void OnEnable()
        {
            GameEvents.OnQTEStarted   += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed    += OnQTEEnded;
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted   -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed    -= OnQTEEnded;
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
        }

        private void Update()
        {
            UpdateCrouchBlend();
            UpdateCollider();

            if (!_movementEnabled)
            {
                ApplyGravityOnly();
                return;
            }

            Vector3 targetVelocity = ComputeTargetVelocity();
            SmoothVelocity(targetVelocity);
            ApplyMovement();
        }

        // ---------------------------------------------------------------------------
        // Velocity computation
        // ---------------------------------------------------------------------------

        private Vector3 ComputeTargetVelocity()
        {
            Vector2 raw = _input.MoveInput;
            if (raw.sqrMagnitude < 0.01f) return Vector3.zero;

            // Project camera forward and right onto the XZ plane so vertical camera
            // angle does not tilt the movement direction into the floor.
            Vector3 camForward = _cameraRoot.forward;
            Vector3 camRight   = _cameraRoot.right;
            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 direction = (camForward * raw.y + camRight * raw.x).normalized;
            float   speed     = IsCrouching ? _data.CrouchSpeed : _data.WalkSpeed;

            return direction * speed;
        }

        private void SmoothVelocity(Vector3 target)
        {
            float rate = target.sqrMagnitude > 0.01f ? _data.Acceleration : _data.Deceleration;
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, target, rate * Time.deltaTime);

            HorizontalSpeed = _currentVelocity.magnitude;
            WorldVelocity   = _currentVelocity;
        }

        // ---------------------------------------------------------------------------
        // Gravity & movement application
        // ---------------------------------------------------------------------------

        private void ApplyMovement()
        {
            ApplyGravity();
            Vector3 move = _currentVelocity + Vector3.up * _verticalVelocity;
            _cc.Move(move * Time.deltaTime);
        }

        private void ApplyGravityOnly()
        {
            // Even when movement input is locked, gravity must still act.
            _currentVelocity = Vector3.zero;
            HorizontalSpeed  = 0f;
            ApplyGravity();
            _cc.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded)
                _verticalVelocity = _data.GroundedGravity;
            else
                _verticalVelocity += _data.AirGravity * Time.deltaTime;
        }

        // ---------------------------------------------------------------------------
        // Crouch
        // ---------------------------------------------------------------------------

        private void UpdateCrouchBlend()
        {
            bool wantCrouch = _input.CrouchHeld;

            // Cannot stand up if ceiling is too low.
            if (!wantCrouch && IsCrouching && CeilingDetected())
                wantCrouch = true;

            IsCrouching = wantCrouch;

            float target = wantCrouch ? 1f : 0f;
            float speed  = 1f / Mathf.Max(_data.CrouchTransitionTime, 0.01f);
            _crouchBlend = Mathf.MoveTowards(_crouchBlend, target, speed * Time.deltaTime);
        }

        private void UpdateCollider()
        {
            float height  = Mathf.Lerp(_data.StandHeight,   _data.CrouchHeight,   _crouchBlend);
            float centerY = Mathf.Lerp(_data.StandCenterY,  _data.CrouchCenterY,  _crouchBlend);
            _cc.height    = height;
            _cc.center    = new Vector3(0f, centerY, 0f);
        }

        private bool CeilingDetected()
        {
            // Cast upward from the character center to check for overhead geometry.
            float castDistance = _data.StandHeight - _data.CrouchHeight + 0.05f;
            Vector3 origin     = transform.position + Vector3.up * (_data.CrouchHeight - 0.1f);
            return Physics.Raycast(origin, Vector3.up, castDistance);
        }

        // ---------------------------------------------------------------------------
        // Accessors
        // ---------------------------------------------------------------------------

        /// <summary>Normalised crouch blend (0 = stand, 1 = full crouch). Used by AnimationComponent.</summary>
        public float CrouchBlend => _crouchBlend;

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnQTEStarted(QTEZonePayload _)   => _movementEnabled = false;
        private void OnQTEEnded(QTEZonePayload _)      => _movementEnabled = true;

        private void OnPlayerRespawned(CheckpointPayload p)
        {
            _currentVelocity   = Vector3.zero;
            _verticalVelocity  = 0f;
            _movementEnabled   = true;

            // Teleport the CharacterController (disable/enable to clear internal state).
            _cc.enabled = false;
            transform.SetPositionAndRotation(p.RespawnPosition, p.RespawnRotation);
            _cc.enabled = true;
        }

       
    }
}
