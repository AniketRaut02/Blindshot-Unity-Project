// CinemachineConfigurator.cs
// Applies the suggested Cinemachine Third Person Follow values at runtime
// and documents why each setting exists.
//
// DESIGN RATIONALE:
// Storing camera feel values in a ScriptableObject (CameraSettingsData) means
// a designer can tune them per-zone without touching code. The configurator
// reads the asset and pushes values to the Cinemachine Camera component.
//
// WHY RUNTIME CONFIGURATION INSTEAD OF JUST SETTING VALUES IN THE INSPECTOR?
// The Inspector values ARE the authoritative source during normal development.
// This class is an optional override layer that lets data-driven camera profiles
// swap in (e.g., a tighter FOV in vent sections). In Phase 1 it simply validates
// that the required components are present and logs any misconfiguration early.
//
// Cinemachine 3.x API note:
// CinemachineThirdPersonFollow replaced the old Cinemachine3rdPersonFollow.
// All property names below match the Cinemachine 3.1.x public API.

using UnityEngine;
using Unity.Cinemachine;

namespace SciFiGame.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineConfigurator : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Header("Optional Data Override")]
        [Tooltip("Leave null to keep Inspector values. Assign a profile to override at runtime.")]
        [SerializeField] private CameraSettingsData _settingsOverride;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private CinemachineCamera             _cmCamera;
        private CinemachineThirdPersonFollow  _thirdPersonFollow;
        private CinemachineBasicMultiChannelPerlin _noise;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _cmCamera = GetComponent<CinemachineCamera>();

            // Cinemachine 3.x uses a component-based model on the virtual camera.
            _thirdPersonFollow = _cmCamera.GetComponent<CinemachineThirdPersonFollow>();
            _noise             = _cmCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            ValidateComponents();

            if (_settingsOverride != null)
                ApplySettings(_settingsOverride);
        }

        // ---------------------------------------------------------------------------
        // Settings application
        // ---------------------------------------------------------------------------

        private void ApplySettings(CameraSettingsData data)
        {
            if (_thirdPersonFollow != null)
            {
                _thirdPersonFollow.ShoulderOffset    = data.ShoulderOffset;
                _thirdPersonFollow.VerticalArmLength = data.VerticalArmLength;
                _thirdPersonFollow.CameraDistance    = data.CameraDistance;
                _thirdPersonFollow.CameraSide        = data.CameraSide;
            }

            _cmCamera.Lens.FieldOfView = data.FieldOfView;

            if (_noise != null && data.NoiseProfile != null)
            {
                _noise.NoiseProfile = data.NoiseProfile;
                _noise.AmplitudeGain = data.NoiseAmplitude;
                _noise.FrequencyGain = data.NoiseFrequency;
            }
        }

        // ---------------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------------

        private void ValidateComponents()
        {
            if (_thirdPersonFollow == null)
                Debug.LogError($"[CinemachineConfigurator] No CinemachineThirdPersonFollow found on {name}. " +
                               "Add it via Add Cinemachine Component → Third Person Follow.", this);

            if (_noise == null)
                Debug.LogWarning($"[CinemachineConfigurator] No CinemachineBasicMultiChannelPerlin on {name}. " +
                                 "Ambient noise is disabled. Add via Add Cinemachine Component → Noise → Basic Multi Channel Perlin.", this);

            if (_cmCamera.Follow == null)
                Debug.LogError($"[CinemachineConfigurator] Cinemachine Camera has no Follow target. " +
                               "Assign the player's CameraRoot transform.", this);
        }
    }
}
