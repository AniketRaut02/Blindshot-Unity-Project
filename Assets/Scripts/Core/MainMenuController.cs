// MainMenuController.cs
// Handles core main menu logic: Scene loading, quitting, and UI audio integration.
//
// DESIGN RATIONALE:
// This component exposes public methods that Unity's UI Button "OnClick()" and 
// EventTrigger "OnPointerEnter()" events can call directly from the Inspector.

using UnityEngine;
using UnityEngine.SceneManagement;
using SciFiGame.Audio; 

namespace SciFiGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Header("Scene Management")]
        [Tooltip("The exact name of the scene to load when Play is clicked.")]
        [SerializeField] private int _gameplaySceneIndex = 1;

        [Tooltip("Sound played when any button is clicked.")]
        [SerializeField] private AudioClip _clickSound;

        [Tooltip("Sound played when the mouse hovers over a button.")]
        [SerializeField] private AudioClip _hoverSound;

        // ---------------------------------------------------------------------------
        // Public UI Methods (Assigned via Unity Inspector)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called by the Play Button's OnClick event.
        /// </summary>
        public void OnPlayButtonClicked()
        {
            PlayClickAudio();

            // Load the target scene. 
            // Note: Ensure the scene is added to File > Build Settings!
            SceneManager.LoadScene(_gameplaySceneIndex);
        }

        /// <summary>
        /// Called by the Quit Button's OnClick event.
        /// </summary>
        public void OnQuitButtonClicked()
        {
            PlayClickAudio();

            Debug.Log("[MainMenuController] Quitting Application...");

            Application.Quit();

            // This ensures the quit button visually works while testing in the Unity Editor
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Called by an EventTrigger (OnPointerEnter) attached to the buttons.
        /// </summary>
        public void OnButtonHovered()
        {
            if (AudioManagerComponent.Instance != null && _hoverSound != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(_hoverSound, 1f);
            }
        }

        // ---------------------------------------------------------------------------
        // Internal Helpers
        // ---------------------------------------------------------------------------

        private void PlayClickAudio()
        {
            if (AudioManagerComponent.Instance != null && _clickSound != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(_clickSound,1f);
            }
            else
            {
                Debug.LogWarning("[MainMenuController] Missing AudioManager or ClickSound reference!");
            }
        }
    }
}