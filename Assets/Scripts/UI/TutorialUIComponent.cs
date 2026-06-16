using UnityEngine;
using TMPro;
using System.Collections;
using SciFiGame.Core;

namespace SciFiGame.UI
{
    public class TutorialUIComponent : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TextMeshProUGUI _tutorialText;

        [Header("Audio Settings")]
        [Tooltip("The sound that plays when the tutorial pops up on screen.")]
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] AudioSource _audioSource;
        private Coroutine _displayCoroutine;

        // ---------------------------------------------------------------------------
        // Lifecycle & Subscription
        // ---------------------------------------------------------------------------
        private void Awake()
        {
            // Force the AudioSource to be 2D so it plays at full volume in the UI
            _audioSource.spatialBlend = 0f;
            _audioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            GameEvents.OnTutorialPromptTriggered += ShowTutorial;
        }

        private void OnDisable()
        {
            GameEvents.OnTutorialPromptTriggered -= ShowTutorial;
        }

        private void Start()
        {
            if (_tutorialPanel != null)
            {
                _tutorialPanel.SetActive(false);
            }
        }

        // ---------------------------------------------------------------------------
        // Event Handling
        // ---------------------------------------------------------------------------
        private void ShowTutorial(TutorialPromptPayload payload)
        {
            if (_tutorialPanel == null || _tutorialText == null) return;

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }

            // 1. Play the notification sound
            if (_notificationSound != null)
            {
                _audioSource.PlayOneShot(_notificationSound);
            }

            // 2. Set the text and turn on the UI
            _tutorialText.text = payload.Message;
            _tutorialPanel.SetActive(true);

            // 3. Start the countdown timer
            _displayCoroutine = StartCoroutine(HideTutorialAfterDelay(payload.DisplayDuration));
        }

        private IEnumerator HideTutorialAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            _tutorialPanel.SetActive(false);
            _displayCoroutine = null;
        }
    }
}