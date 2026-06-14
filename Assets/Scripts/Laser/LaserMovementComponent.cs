// LaserMovementComponent.cs
//
// DESIGN RATIONALE:
// LaserBeam handles collision.
// LaserMovementComponent handles transform animation and flashing.
//
// The behaviour is fully driven by LaserMovementData assets.

using UnityEngine;

namespace SciFiGame.Laser
{
    public class LaserMovementComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField]
        private LaserMovementData _movementData;

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
            float time = Time.time + _movementData.PhaseOffset;

            Vector3 position = _startLocalPosition;

            position.x +=
                Mathf.Sin(time * _movementData.Speed)
                * _movementData.Amplitude;

            transform.localPosition = position;
        }

        private void HandleVerticalMovement()
        {
            float time = Time.time + _movementData.PhaseOffset;

            Vector3 position = _startLocalPosition;

            position.y +=
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
                _delayTimer += Time.deltaTime;

                if (_delayTimer < _movementData.InitialDelay)
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
    }
}