// PlayerAnimationComponent.cs
// Translates movement state into Animator parameters.
//
// DESIGN RATIONALE:
// Animation logic belongs in its own component, not scattered across movement or
// the state machine. The Animator is treated as a black box that receives float and
// bool parameters; this component is the only place that knows their string names.
//
// WHY HASHED PARAMETER IDS?
// Animator.SetFloat("Speed") does a dictionary lookup on every call. Using
// Animator.StringToHash at startup converts the string once and stores an int,
// which is a direct array index into the Animator's parameter table. This is a
// standard Unity optimisation and has no downside.
//
// CROUCH BLEND:
// We pass CrouchBlend as a float (0–1) so the Animator can drive a blend tree
// between stand and crouch idle/walk animations. This gives smooth transitions
// without requiring separate Animator states for "transitioning into crouch".
//
// The Animator Controller is not created by this script; it is set up in the
// Unity Editor. Parameter names must match exactly what is in the controller.
//
// EXPECTED ANIMATOR PARAMETERS:
//   float Speed         — normalised horizontal speed (0 = idle, 1 = full walk)
//   float CrouchBlend   — 0 = standing, 1 = fully crouched
//   bool  IsGrounded    — reserved for Phase 2 jump/fall states

using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private PlayerMovementComponent _movement;

        [Header("Dampening Settings")]
        [Tooltip("Time it takes to smooth the speed value. Higher is slower.")]
        [SerializeField] private float _speedDampTime = 0.1f;

        [Tooltip("Time it takes to smooth the crouch blend. Higher is slower.")]
        [SerializeField] private float _crouchDampTime = 0.1f;

        // ---------------------------------------------------------------------------
        // Private — cached hashes
        // ---------------------------------------------------------------------------

        private Animator _animator;

        private static readonly int SpeedHash       = Animator.StringToHash("Speed");
        private static readonly int CrouchBlendHash = Animator.StringToHash("CrouchBlend");
        private static readonly int IsGroundedHash  = Animator.StringToHash("IsGrounded");

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Determine the target speed (0 or 1)
            float targetSpeed = _movement.HorizontalSpeed > 0.01f ? 1f : 0f;

            // Apply the values using the built-in dampening overload
            _animator.SetFloat(SpeedHash, targetSpeed, _speedDampTime, Time.deltaTime);
            _animator.SetFloat(CrouchBlendHash, _movement.CrouchBlend, _crouchDampTime, Time.deltaTime);

            _animator.SetBool(IsGroundedHash, true); // Phase 1: always grounded
        }
/*
        // ---------------------------------------------------------------------------
        // Public — called by PlayerStateMachine for state-driven animation triggers
        // ---------------------------------------------------------------------------

        public void SetTrigger(string triggerName)
        {
            _animator.SetTrigger(triggerName);
        }

        public Animator GetAnimator() => _animator;*/
    }
}
