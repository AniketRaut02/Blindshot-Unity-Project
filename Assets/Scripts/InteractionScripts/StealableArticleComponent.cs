using UnityEngine;
using SciFiGame.Audio;

namespace SciFiGame.Interaction
{
    public class StealableArticleComponent : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private string _promptText = "Steal Classified Article";
        [SerializeField] private GameObject barrierObject;
        [SerializeField] private AudioClip feedbackClip;

        public string GetInteractionPrompt()
        {
            return _promptText;
        }

        public void Interact()
        {
            // TODO: Add any logic here to update player inventory or play a sound effect.
            barrierObject.SetActive(false);
            if (AudioManagerComponent.Instance != null)
            {
                AudioManagerComponent.Instance.PlayOneShot(feedbackClip);
            }
            // Remove the object from the scene
            Destroy(gameObject);
        }
    }
}