// LaserMovementComponent.cs
//
// DESIGN RATIONALE:
// LaserBeam handles collision.
// LaserMovementComponent handles transform animation and flashing.
//
// The behaviour is fully driven by LaserMovementData assets.
// Instance-level timing offsets are exposed here to allow identical lasers
// to run out of sync (creating staggered patterns).

using UnityEngine;

namespace SciFiGame.Laser
{
    public class LaserMovementComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Header("Data Profile")]
        [SerializeField]
        private LaserMovementData _movementData;

        [Header("Instance Settings")]
        [Tooltip("Add a unique number here (e.g., 1, 2, 3.14) to stagger this laser's movement so it doesn't sync perfectly with others using the same Data Profile.")]
        [SerializeField]
        private float _localPhaseOffset = 0f;

        [Header("References")]
        [SerializeField]
        private Collider _laserCollider;

        [SerializeField]
        private Renderer[] _renderers;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private Vector3 _startLocalPosition;

        private bool _flashVisible = true;

        private float _flashTimer;

        private float _delayTimer;

        private bool _delayFinished;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _startLocalPosition = transform.localPosition;
            _delayTimer = 0f;
            _delayFinished = false;
        }

        private void Update()
        {
            if (_movementData == null)
                return;

            switch (_movementData.MovementType)
            {
                case LaserMovementType.None:
                    break;

                case LaserMovementType.HorizontalPingPong:
                    HandleHorizontalMovement();
                    break;

                case LaserMovementType.VerticalPingPong:
                    HandleVerticalMovement();
                    break;

                case LaserMovementType.YAxisPingPong:
                    HandleYAxisMovement();
                    break;

                case LaserMovementType.Rotation:
                    HandleRotation();
                    break;

                case LaserMovementType.Flashing:
                    HandleFlashing();
                    break;
            }
        }

        // ---------------------------------------------------------------------------
        // Translation
        // --------------------------------------------------------------------------
        private void HandleHorizontalMovement()
        {
            // Combine the shared SO offset with this instance's unique offset
            float time = Time.time + _movementData.PhaseOffset + _localPhaseOffset;

            Vector3 position = _startLocalPosition;

            position.x +=
                Mathf.Sin(time * _movementData.Speed)
                * _movementData.Amplitude;

            transform.localPosition = position;
        }

        private void HandleVerticalMovement()
        {
            // Combine the shared SO offset with this instance's unique offset
            float time = Time.time + _movementData.PhaseOffset + _localPhaseOffset;

            Vector3 position = _startLocalPosition;

            position.z +=
                Mathf.Sin(time * _movementData.Speed)
                * _movementData.Amplitude;

            transform.localPosition = position;
        }

        private void HandleRotation()
        {
            transform.Rotate(
                _movementData.RotationAxis.normalized,
                _movementData.Speed * Time.deltaTime,
                Space.Self);
        }

        // ---------------------------------------------------------------------------
        // Flashing
        // ---------------------------------------------------------------------------

        private void HandleFlashing()
        {
            // -------------------------------------------------------
            // Initial delay
            // -------------------------------------------------------

            if (!_delayFinished)
            {
                // We can also use _localPhaseOffset here to stagger flashing delays!
                float totalDelay = _movementData.InitialDelay + _localPhaseOffset;

                _delayTimer += Time.deltaTime;

                if (_delayTimer < totalDelay)
                    return;

                _delayFinished = true;
            }

            // -------------------------------------------------------
            // Flash cycle
            // -------------------------------------------------------

            _flashTimer += Time.deltaTime;

            float duration =
                _flashVisible
                    ? _movementData.OnDuration
                    : _movementData.OffDuration;

            if (_flashTimer < duration)
                return;

            _flashTimer = 0f;

            _flashVisible = !_flashVisible;

            foreach (Renderer rendererComponent in _renderers)
            {
                if (rendererComponent != null)
                    rendererComponent.enabled = _flashVisible;
            }

            if (_laserCollider != null)
            {
                _laserCollider.enabled = _flashVisible;
            }
        }

        private void HandleYAxisMovement()
        {
            // Combine the shared SO offset with this instance's unique offset
            float time = Time.time + _movementData.PhaseOffset + _localPhaseOffset;

            Vector3 position = _startLocalPosition;

            // Apply the sine wave to the Y axis
            position.y +=
                Mathf.Sin(time * _movementData.Speed)
                * _movementData.Amplitude;

            transform.localPosition = position;
        }
    }
}