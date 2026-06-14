// PlayerRotationComponent.cs
// Rotates the character body toward the current world-space movement direction.
//
// DESIGN RATIONALE:
// Character rotation is separated from movement for the same reason camera
// rotation is separated: each is an independent concern and changes independently.
// For example, during a dodge QTE animation, root motion drives position but
// rotation may or may not be overridden — having them in separate components
// lets the Timeline (or a future lock-on system) disable rotation alone.
//
// WHY MOVEMENT DIRECTION AND NOT CAMERA FORWARD?
// RE4-style movement means the character always turns to face where they walk.
// If we used camera forward the character would snap to the camera's yaw even
// when standing still, which feels wrong over the shoulder. Using the velocity
// direction from MovementComponent means the character rotates only while moving,
// which feels natural.
//
// EXCEPTION: if no movement is occurring (player is idle), we preserve the last
// known rotation so the character doesn't snap when input is released.

using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Player
{
    public class PlayerRotationComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private MovementData          _data;
        [SerializeField] private PlayerMovementComponent _movement;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private bool _rotationEnabled = true;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            GameEvents.OnQTEStarted   += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed    += OnQTEEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted   -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed    -= OnQTEEnded;
        }

        private void Update()
        {
            if (!_rotationEnabled) return;

            Vector3 moveDir = _movement.WorldVelocity;
            moveDir.y = 0f;

            if (moveDir.sqrMagnitude < 0.001f) return;   // idle — preserve current rotation

            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation   = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                _data.RotationSpeed * Time.deltaTime);
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        // During QTEs the Timeline owns root motion and rotation — we step aside.
        private void OnQTEStarted(QTEZonePayload _)   => _rotationEnabled = false;
        private void OnQTEEnded(QTEZonePayload _)      => _rotationEnabled = true;
    }
}
