using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Player
{
    public class PlayerRespawnRecovery : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign ALL Animators in the hierarchy (PlayerRoot AND Visual)")]
        [SerializeField] private Animator[] _animators;

        [SerializeField] private Transform _visualRoot;

        private Vector3 _defaultVisualPosition;
        private Quaternion _defaultVisualRotation;

        private void Awake()
        {
            _defaultVisualPosition = _visualRoot.localPosition;
            _defaultVisualRotation = _visualRoot.localRotation;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
        }

        private void OnPlayerRespawned(CheckpointPayload _)
        {
            // Reset local transform
            _visualRoot.localPosition = _defaultVisualPosition;
            _visualRoot.localRotation = _defaultVisualRotation;

            // Reset EVERY Animator in the rig
            foreach (Animator anim in _animators)
            {
                if (anim == null) continue;

                anim.enabled = false;
                anim.applyRootMotion = false; // Critical: Release the rotation lock

                anim.Rebind();
                anim.Update(0f);

                anim.enabled = true;
            }
        }
    }
}