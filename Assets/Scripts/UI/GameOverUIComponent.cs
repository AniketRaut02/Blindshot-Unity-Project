using UnityEngine;
using UnityEngine.SceneManagement;
using SciFiGame.Core; // Required to listen to GameEvents
using SciFiGame.Input;
using SciFiGame.Audio;

namespace SciFiGame.UI
{
    public class GameOverUIComponent : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Assign the Game Over Canvas or Panel here")]
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Input Control")]
        [Tooltip("Assign your InputReader ScriptableObject here")]
        [SerializeField] private InputReader _inputReader;

        [Header("Audio")]
        [Tooltip("Sound played when any button is clicked.")]
        [SerializeField] private AudioClip _clickSound;

        [Tooltip("Sound played when the mouse hovers over a button.")]
        [SerializeField] private AudioClip _hoverSound;

        // ---------------------------------------------------------------------------
        // Lifecycle & Subscription
        // ---------------------------------------------------------------------------
        private void OnEnable()
        {
            // Subscribe to the event
            GameEvents.OnGameOver += ShowGameOverScreen;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            GameEvents.OnGameOver -= ShowGameOverScreen;
        }

        private void Start()
        {
            // Ensure the screen is hidden when the level starts
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }
        }

        // ---------------------------------------------------------------------------
        // Event Handlers
        // ---------------------------------------------------------------------------
        private void ShowGameOverScreen()
        {
            if (_gameOverPanel == null) return;

            // 1. Show the UI panel
            _gameOverPanel.SetActive(true);

            if (_inputReader != null)
            {
                _inputReader.EnableUI();
            }
            else
            {
                Debug.LogWarning("[GameOverUI] InputReader is not assigned!");
            }

            // 3. Unlock the cursor so the player can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 3. Optional: Pause the game world so enemies stop moving
            Time.timeScale = 0f; 
        }

        // ---------------------------------------------------------------------------
        // Button Callbacks (Link these to Unity UI Buttons)
        // ---------------------------------------------------------------------------

        public void OnRestartPressed()
        {
            // If you paused the time scale, remember to unpause it!
            Time.timeScale = 1f;
            if (_inputReader != null)
            {
                _inputReader.EnableGameplay();
            }
            PlayClickAudio();

            // Reloads the current active scene
            SceneManager.LoadScene(0);
        }

        public void OnQuitPressed()
        {
            Debug.Log("Quitting Game...");
            PlayClickAudio();
            Application.Quit();
        }

        public void OnButtonHovered()
        {
            if (AudioManagerComponent.Instance != null && _hoverSound != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(_hoverSound, 1f);
            }
        }

        private void PlayClickAudio()
        {
            if (AudioManagerComponent.Instance != null && _clickSound != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(_clickSound, 1f);
            }
            else
            {
                Debug.LogWarning("[MainMenuController] Missing AudioManager or ClickSound reference!");
            }
        }
    }
}