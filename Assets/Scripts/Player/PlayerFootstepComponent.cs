using UnityEngine;

namespace SciFiGame.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerFootstepComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------
        [Header("Dependencies")]
        [Tooltip("Leave blank to auto-fetch from this GameObject")]
        [SerializeField] private PlayerMovementComponent _movement;

        [Header("Audio Assets")]
        [SerializeField] private AudioClip[] _footstepClips;

        [Header("Ticking (Animation Sync)")]
        [Tooltip("Time between steps when walking normally")]
        [SerializeField] private float _walkStepInterval = 0.45f;
        [Tooltip("Time between steps when crouching (usually slower)")]
        [SerializeField] private float _crouchStepInterval = 0.65f;

        [Header("Volume & Pitch Dynamics")]
        [SerializeField, Range(0f, 1f)] private float _walkVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _crouchVolume = 0.15f;
        [SerializeField] private float _minPitch = 0.9f;
        [SerializeField] private float _maxPitch = 1.1f;

        // ---------------------------------------------------------------------------
        // Private State
        // ---------------------------------------------------------------------------
        private AudioSource _audioSource;
        private float _stepTimer;
        private int _lastClipIndex = -1;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            if (_movement == null)
            {
                _movement = GetComponent<PlayerMovementComponent>();
            }

            // Configure AudioSource for 3D/Player sound
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f; // Ensure it's 3D if you want enemies to hear it dynamically
        }

        private void Update()
        {
            HandleFootsteps();
        }

        // ---------------------------------------------------------------------------
        // Logic
        // ---------------------------------------------------------------------------
        private void HandleFootsteps()
        {
            // Only tick the timer if the movement component dictates we are actually moving
            if (_movement.IsMoving)
            {
                _stepTimer += Time.deltaTime;

                // Determine the correct interval based on crouch state
                float currentInterval = _movement.IsCrouching ? _crouchStepInterval : _walkStepInterval;

                if (_stepTimer >= currentInterval)
                {
                    PlayFootstep();
                    _stepTimer = 0f; // Reset the tick
                }
            }
            else
            {
                // Reset timer when stopped. Setting it slightly below 0 ensures the player 
                // doesn't instantly play a footstep on the exact frame they tap the movement key.
                _stepTimer = -0.1f;
            }
        }

        private void PlayFootstep()
        {
            if (_footstepClips == null || _footstepClips.Length == 0) return;

            // 1. Select a non-repeating random clip
            int randomIndex = Random.Range(0, _footstepClips.Length);
            if (randomIndex == _lastClipIndex && _footstepClips.Length > 1)
            {
                // Shift by 1 and wrap around to guarantee a different clip
                randomIndex = (randomIndex + 1) % _footstepClips.Length;
            }
            _lastClipIndex = randomIndex;
            AudioClip clipToPlay = _footstepClips[randomIndex];

            // 2. Adjust Volume dynamically based on crouch
            _audioSource.volume = _movement.IsCrouching ? _crouchVolume : _walkVolume;

            // 3. Randomize Pitch for natural variation
            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);

            // 4. Fire the audio
            _audioSource.PlayOneShot(clipToPlay);
        }
    }
}