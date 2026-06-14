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
            // Normalise speed against walk speed so the blend tree range is always 0–1
            // regardless of the MovementData values a designer chooses.
            float normSpeed = _movement.HorizontalSpeed > 0.01f ? 1f : 0f;

            _animator.SetFloat(SpeedHash,       normSpeed);
            _animator.SetFloat(CrouchBlendHash, _movement.CrouchBlend);
            _animator.SetBool(IsGroundedHash,   true); // Phase 1: always grounded
        }

        // ---------------------------------------------------------------------------
        // Public — called by PlayerStateMachine for state-driven animation triggers
        // ---------------------------------------------------------------------------

        public void SetTrigger(string triggerName)
        {
            _animator.SetTrigger(triggerName);
        }

        public Animator GetAnimator() => _animator;
    }
}
