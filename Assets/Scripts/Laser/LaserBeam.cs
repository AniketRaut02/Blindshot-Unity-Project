using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Laser
{
    [RequireComponent(typeof(Collider))]
    public class LaserBeam : MonoBehaviour
    {
        [SerializeField]
        private string _beamName = "Laser";
        private bool _hasTriggered;

        private void OnEnable()
        {
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered)
                return;

            if (!other.CompareTag("Player"))
                return;

            _hasTriggered = true;

            GameEvents.RaiseLaserHit(
                new LaserHitPayload(_beamName));
        }

        private void OnPlayerRespawned(CheckpointPayload _)
        {
            _hasTriggered = false;
        }
    }
}