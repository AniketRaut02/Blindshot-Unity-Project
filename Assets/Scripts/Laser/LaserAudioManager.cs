using UnityEngine;

// This ensures the script won't crash if you forget to add an AudioSource
[RequireComponent(typeof(AudioSource))]
public class LaserAudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Keep this subtle. 0.8 to 1.2 is usually the sweet spot.")]
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    private AudioSource laserAudio;

    void Start()
    {
        laserAudio = GetComponent<AudioSource>();

        // 1. Randomize the pitch
        laserAudio.pitch = Random.Range(minPitch, maxPitch);

        // 2. Desync the start time so adjacent lasers don't pulse perfectly in sync
        if (laserAudio.clip != null)
        {
            laserAudio.time = Random.Range(0f, laserAudio.clip.length);
        }
    }
}