using UnityEngine;

namespace SciFiGame.Audio
{
    public class AudioManagerComponent : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Singleton Instance
        // ---------------------------------------------------------------------------
        public static AudioManagerComponent Instance { get; private set; }

        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------
        [Header("Audio Sources")]
        [Tooltip("The audio source used for 2D global sound effects (UI, item pickups, etc.)")]
        [SerializeField] private AudioSource _sfxSource;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------
        private void Awake()
        {
            // Enforce Singleton pattern: Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[AudioManager] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional: Keep the audio manager alive across scene loads
            // DontDestroyOnLoad(gameObject);

        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Plays a global, 2D audio clip once.
        /// </summary>
        /// <param name="clip">The sound clip to play.</param>
        /// <param name="volumeScale">Optional volume multiplier (default is 1.0).</param>
        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play a null audio clip.");
                return;
            }

            _sfxSource.PlayOneShot(clip, volumeScale);
        }
    }
}