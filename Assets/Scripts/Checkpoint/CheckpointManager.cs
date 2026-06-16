// CheckpointManager.cs
// Stores the most recently activated checkpoint and handles respawning.
//
// DESIGN RATIONALE:
// CheckpointManager owns checkpoint state.
// Individual Checkpoint triggers are stateless world objects.

using UnityEngine;
using SciFiGame.Core;
using SciFiGame.Laser;
using SciFiGame.Timeline;

namespace SciFiGame.Checkpoint
{
    public class CheckpointManager : MonoBehaviour
    {

        [SerializeField]
        private GlobalTimelinePlayer _timelinePlayer;
        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private CheckpointPayload _lastCheckpoint;

        private bool _hasCheckpoint;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            GameEvents.OnCheckpointActivated += OnCheckpointActivated;
            GameEvents.OnPlayerDied += OnPlayerDied;

            Debug.Log("CheckpointManager subscribed");
        }

        private void OnDisable()
        {
            GameEvents.OnCheckpointActivated -= OnCheckpointActivated;
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnCheckpointActivated(CheckpointPayload payload)
        {
            _lastCheckpoint = payload;
            _hasCheckpoint = true;

            Debug.Log($"Checkpoint activated: {payload.CheckpointId}");
        }

        private void OnPlayerDied(PlayerDeathPayload payload)
        {
            Debug.Log("Player died, waiting for timeline.");

            if (payload.DeathTimeline != null)
            {
                GameEvents.OnTimelineFinished += OnDeathTimelineFinished;

                _timelinePlayer.Play(payload.DeathTimeline);
            }
            else
            {
                Respawn();
            }
        }

        private void OnDeathTimelineFinished(TimelinePayload _)
        {
            Debug.Log("Death timeline finished, respawning.");

            GameEvents.OnTimelineFinished -= OnDeathTimelineFinished;

            Respawn();
        }

        // ---------------------------------------------------------------------------
        // Respawn
        // ---------------------------------------------------------------------------

        private void Respawn()
        {
            if (!_hasCheckpoint)
            {
                Debug.LogWarning("No checkpoint available.");
                return;
            }

            GameEvents.RaisePlayerRespawned(_lastCheckpoint);
        }
    }
}