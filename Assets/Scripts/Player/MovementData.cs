// MovementData.cs
// ScriptableObject containing all movement tuning parameters.
//
// DESIGN RATIONALE:
// Movement feel is almost entirely a tuning problem. Keeping every speed, 
// acceleration, and collider value in an asset means you can:
//   - Create alternate profiles (e.g., "Movement_SlipperyFloor") without code changes.
//   - Revert to a known-good state by swapping assets.
//   - Expose the asset to non-programmers safely.
//
// These defaults target the RE4-Remake feel: deliberate, weight-forward movement
// with a noticeable deceleration when stopping — not floaty or instant-response.
//
// CREATE: Right-click in Project → SciFiGame → Player → Movement Data

using UnityEngine;

namespace SciFiGame.Player
{
    [CreateAssetMenu(fileName = "MovementData_Default",
                     menuName  = "SciFiGame/Player/Movement Data")]
    public class MovementData : ScriptableObject
    {
        [Header("Walk")]
        [Tooltip("Maximum walk speed in m/s.")]
        public float WalkSpeed         = 3.5f;

        [Tooltip("How quickly the character accelerates toward target velocity (higher = snappier).")]
        public float Acceleration      = 10f;

        [Tooltip("How quickly the character decelerates when no input is given.")]
        public float Deceleration      = 12f;

        [Header("Crouch")]
        [Tooltip("Maximum crouch walk speed in m/s.")]
        public float CrouchSpeed       = 1.8f;

        [Tooltip("Time (seconds) to transition into/out of crouch.")]
        public float CrouchTransitionTime = 0.25f;

        [Header("Gravity")]
        [Tooltip("Downward force applied when grounded. Keeps the character on slopes.")]
        public float GroundedGravity   = -2f;

        [Tooltip("Full gravity when airborne. Currently unused until jump is added.")]
        public float AirGravity        = -20f;

        [Header("Collider — Stand")]
        [Tooltip("CharacterController height when standing.")]
        public float StandHeight       = 1.8f;

        [Tooltip("CharacterController center Y when standing (half of height).")]
        public float StandCenterY      = 0.9f;

        [Header("Collider — Crouch")]
        [Tooltip("CharacterController height when crouching.")]
        public float CrouchHeight      = 1.0f;

        [Tooltip("CharacterController center Y when crouching.")]
        public float CrouchCenterY     = 0.5f;

        [Header("Rotation")]
        [Tooltip("Angular speed (degrees/second) for character body rotation toward move direction.")]
        public float RotationSpeed     = 720f;
    }
}
