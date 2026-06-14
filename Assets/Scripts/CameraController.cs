// CameraController.cs
// Drives the CameraRoot transform for yaw (horizontal) and pitch (vertical) rotation.
//
// DESIGN RATIONALE:
// Cinemachine's Third Person Follow tracks the CameraRoot, not the player body.
// By rotating CameraRoot we get full over-shoulder control without fighting
// Cinemachine's internal position logic. The character body is rotated separately
// by RotationComponent, which uses the CameraRoot's Y-angle as its target yaw.
//
// WHY NOT USE CINEMACHINE'S POV OR ORBITAL FOLLOW?
// POV rotates the virtual camera itself, which makes it awkward to drive character
// rotation from it (you'd need to read back angles from the CM camera). Orbital Follow
// adds complexity we don't need. Owning the rotation ourselves keeps the contract clean:
//   CameraRoot owns yaw + pitch.
//   Cinemachine owns position offset + noise + collision.
//
// PITCH LIMITS:
// +20° up / -35° down matches RE4-style downward bias for corridor gameplay.
// Tighter upward range reduces the chance of the camera clipping into low ceilings.
//
// CURSOR LOCK:
// Locking happens here because the camera is the system that consumes mouse delta.
// If cursor management grows more complex, extract it to a CursorManager singleton.

using UnityEngine;
using SciFiGame.Input;
using SciFiGame.Core;

namespace SciFiGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Header("References")]
        [Tooltip("The CameraRoot transform that Cinemachine tracks.")]
        [SerializeField] private Transform _cameraRoot;

        [SerializeField] private InputReader _inputReader;

        [Header("Sensitivity")]
        [SerializeField, Range(0.01f, 5f)] private float _horizontalSensitivity = 0.2f;
        [SerializeField, Range(0.01f, 5f)] private float _verticalSensitivity   = 0.2f;

        [Header("Pitch Limits (degrees)")]
        [SerializeField] private float _pitchMax =  20f;   // upward limit
        [SerializeField] private float _pitchMin = -35f;   // downward limit

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private float _yaw;
        private float _pitch;
        private bool  _inputEnabled = true;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            // Initialise yaw/pitch from the current CameraRoot rotation so the
            // camera doesn't snap on the first frame.
            Vector3 euler = _cameraRoot.eulerAngles;
            _yaw   = euler.y;
            _pitch = euler.x > 180f ? euler.x - 360f : euler.x; // normalise to [-180,180]
        }

        private void OnEnable()
        {
            _inputReader.OnLookInput += HandleLookInput;
            GameEvents.OnQTEStarted += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed += OnQTEEnded;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void OnDisable()
        {
            _inputReader.OnLookInput -= HandleLookInput;
            GameEvents.OnQTEStarted -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed -= OnQTEEnded;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void LateUpdate()
        {
            // Applied in LateUpdate so all movement has already been committed this frame.
            ApplyRotation();
        }

        // ---------------------------------------------------------------------------
        // Input handler
        // ---------------------------------------------------------------------------

        private void HandleLookInput(Vector2 delta)
        {
            if (!_inputEnabled) return;

            _yaw   += delta.x * _horizontalSensitivity;
            _pitch -= delta.y * _verticalSensitivity;   // invert Y: mouse up = pitch down
            _pitch  = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
        }

        // ---------------------------------------------------------------------------
        // Rotation application
        // ---------------------------------------------------------------------------

        private void ApplyRotation()
        {
            _cameraRoot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        // ---------------------------------------------------------------------------
        // Event handlers — disable look during Timeline/QTE
        // ---------------------------------------------------------------------------

        private void OnQTEStarted(QTEZonePayload _)   => _inputEnabled = false;
        private void OnQTEEnded(QTEZonePayload _)      => _inputEnabled = true;

        // ---------------------------------------------------------------------------
        // Public accessors — RotationComponent reads yaw to rotate the character.
        // ---------------------------------------------------------------------------

        public float CurrentYaw   => _yaw;
        public float CurrentPitch => _pitch;
    }
}
