using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Laser
{
    [RequireComponent(typeof(Collider))]
    public class LaserZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameEvents.RaiseLaserEntered();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameEvents.RaiseLaserExited();
        }
    }
}