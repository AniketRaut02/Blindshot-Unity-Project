using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Laser
{
    [RequireComponent(typeof(Collider))]
    public class LaserBeam : MonoBehaviour
    {
        [SerializeField]
        private string _beamName = "Laser";

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameEvents.RaiseLaserHit(
                new LaserHitPayload(_beamName));
        }
    }
}