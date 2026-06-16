using UnityEngine;
using UnityEngine.SceneManagement;
using SciFiGame.Input; // Required for the InputReader
using SciFiGame.Audio;

namespace SciFiGame.UI
{
    public class EscapeMenuUIComponent : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The main parent panel of the Escape Menu")]
        [SerializeField] private GameObject _escapePanel;

        [Header("Dependencies")]
        [SerializeField] private InputReader _inputReader;

        [Header("Audio")]
        [Tooltip("Sound played when any button is clicked.")]
        [SerializeField] private AudioClip _clickSound;

        [Tooltip("Sound played when the mouse hovers over a button.")]
        [SerializeField] private AudioClip _hoverSound;
        private bool _isPaused = false;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------
        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnPauseInput += PauseGame;
                _inputReader.OnResumeInput += ResumeGame;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnPauseInput -= PauseGame;
                _inputReader.OnResumeInput -= ResumeGame;
            }
        }

        private void Start()
        {
            // Ensure the menu is off when the game starts
            if (_escapePanel != null)
            {
                _escapePanel.SetActive(false);
            }
        }

        // ---------------------------------------------------------------------------
        // Core Logic
        // ---------------------------------------------------------------------------
        private void PauseGame()
        {
            if (_isPaused) return;

            _isPaused = true;
            _escapePanel.SetActive(true);

            // 1. Stop time (freezes physics, movement, and standard animations)
            Time.timeScale = 0f;

            // 2. Switch input maps so gameplay inputs stop and UI inputs start
            _inputReader.EnableUI();

            // 3. Unlock the cursor for mouse navigation
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _escapePanel.SetActive(false);

            // 1. Restore time
            Time.timeScale = 1f;

            // 2. Restore gameplay input
            _inputReader.EnableGameplay();

            // 3. Re-lock the cursor for the camera controller
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ---------------------------------------------------------------------------
        // Button Callbacks
        // ---------------------------------------------------------------------------

        // Link this to the "Continue" button's OnClick event
        public void OnContinueClicked()
        {
            PlayClickAudio();
            ResumeGame();
        }

        // Link this to the "Home" button's OnClick event
        public void OnHomeClicked()
        {
            // IMPORTANT: Always reset time scale before loading a new scene!
            Time.timeScale = 1f;
            if (_inputReader != null)
            {
                _inputReader.EnableGameplay();
            }
            // Assuming 0 is your Main Menu build index. Change if necessary.
            PlayClickAudio();
            SceneManager.LoadScene(0);
        }

        // Link this to the "Exit" button's OnClick event
        public void OnExitClicked()
        {
            Debug.Log("Exiting Game from Pause Menu...");
            PlayClickAudio();
            Application.Quit();
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

        public void OnButtonHovered()
        {
            if (AudioManagerComponent.Instance != null && _hoverSound != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(_hoverSound, 1f);
            }
        }

    }
}