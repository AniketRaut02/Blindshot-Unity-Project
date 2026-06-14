// Checkpoint.cs
// Trigger volume representing a respawn checkpoint.
//
// DESIGN RATIONALE:
// Checkpoint is a passive trigger. It does not store global state;
// CheckpointManager owns the currently active checkpoint.

using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.Checkpoint
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField]
        private string _checkpointId = "first";

        [SerializeField]
        private Transform _respawnPoint;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        // ---------------------------------------------------------------------------
        // Trigger
        // ---------------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameEvents.RaiseCheckpointActivated(new CheckpointPayload(_respawnPoint.position, _respawnPoint.rotation,_checkpointId));
        }
    }
}