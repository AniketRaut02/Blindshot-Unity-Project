using UnityEngine;

namespace SciFiGame.Laser
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways] // This allows you to see the laser length in the Editor without hitting Play!
    public class LaserBeamVisual : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Header("Laser Visuals")]
        [Tooltip("Maximum length of the laser if it doesn't hit a wall.")]
        [SerializeField] private float _maxLength = 20f;

        [Tooltip("Thickness of the visual beam.")]
        [SerializeField] private float _laserWidth = 0.05f;

        [Tooltip("Layers that block the laser visually (e.g., Environment, Walls). DO NOT include the Player layer!")]
        [SerializeField] private LayerMask _environmentMask;

        [Header("Collider Sync")]
        [Tooltip("Assign the BoxCollider used by LaserBeam.cs here. This script will automatically resize it to match the visual beam length.")]
        [SerializeField] private BoxCollider _triggerCollider;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private LineRenderer _lineRenderer;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
        }

        private void Update()
        {
            UpdateLaserVisuals();
        }

        // ---------------------------------------------------------------------------
        // Logic
        // ---------------------------------------------------------------------------

        private void UpdateLaserVisuals()
        {
            if (_lineRenderer == null) return;

            // 1. Set Width
            _lineRenderer.startWidth = _laserWidth;
            _lineRenderer.endWidth = _laserWidth;

            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;
            float currentLength = _maxLength;

            // 2. Raycast to see if we hit a wall. 
            // We only cast against the environment mask so the player passing through doesn't shorten the beam.
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, _maxLength, _environmentMask))
            {
                currentLength = hit.distance;
            }

            // 3. Draw the Line
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, startPos + (direction * currentLength));

            // 4. Sync the Trigger Collider
            // This ensures the player can't be killed by a laser on the *other* side of a wall.
            if (_triggerCollider != null)
            {
                // Make the collider a box that matches the line's width and dynamically changing length
                _triggerCollider.size = new Vector3(_laserWidth, _laserWidth, currentLength);

                // Shift the center of the box forward so it starts at the transform and ends at the wall
                _triggerCollider.center = new Vector3(0, 0, currentLength / 2f);
            }
        }
    }
}