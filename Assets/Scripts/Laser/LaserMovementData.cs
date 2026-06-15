// LaserMovementData.cs
// ScriptableObject describing laser movement behaviour.
//
// DESIGN RATIONALE:
// LaserMovementComponent owns behaviour, while this asset owns tuning values.
// This allows multiple laser emitters to share behaviour without code changes.
//
// CREATE:
// Right-click → SciFiGame → Laser → Laser Movement Data

using UnityEngine;

namespace SciFiGame.Laser
{
    [CreateAssetMenu(
        fileName = "LaserMovement_New",
        menuName = "SciFiGame/Laser/Laser Movement Data")]
    public class LaserMovementData : ScriptableObject
    {
        // ---------------------------------------------------------------------------
        // Movement
        // ---------------------------------------------------------------------------

        [Header("Movement")]
        public LaserMovementType MovementType;

        // ---------------------------------------------------------------------------
        // Shared
        // ---------------------------------------------------------------------------

        [Header("Common")]
        [Min(0f)]
        public float Speed = 2f;

        [Min(0f)]
        public float Amplitude = 2f;

        public float PhaseOffset;

        // ---------------------------------------------------------------------------
        // Rotation
        // ---------------------------------------------------------------------------

        [Header("Rotation")]
        public Vector3 RotationAxis = Vector3.forward;

        // ---------------------------------------------------------------------------
        // Flashing
        // ---------------------------------------------------------------------------

        [Header("Flashing")]
        [Min(0f)]
        public float OnDuration = 1f;

        [Min(0f)]
        public float OffDuration = 1f;

        [Tooltip("Delay before the flashing cycle begins.")]
        [Min(0f)]
        public float InitialDelay = 0f;
    }
}

namespace SciFiGame.Laser
{
    public enum LaserMovementType
    {
        None,

        HorizontalPingPong,

        VerticalPingPong,

        YAxisPingPong,

        Rotation,

        Flashing
    }
}