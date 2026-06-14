// GameEvents.cs
// Central event bus for all inter-system communication.
//
// DESIGN RATIONALE:
// Systems should never hold direct references to one another. Instead, they raise
// and subscribe to named C# events defined here. This makes every system independently
// testable, hot-swappable, and easy to extend without touching existing code.
//
// All events are static so any system can raise or subscribe without needing a
// reference to a manager. The payload types (structs) keep allocations low and
// make the data contract explicit at the call site.
//
// Convention: Raise via GameEvents.OnXxx?.Invoke(payload);
//             Subscribe via GameEvents.OnXxx += handler;
//             Unsubscribe in OnDisable/OnDestroy — always.

using System;
using UnityEngine;
using SciFiGame.QTE;
using SciFiGame.Laser;

namespace SciFiGame.Core
{
    // ---------------------------------------------------------------------------
    // Payload structs — lightweight, zero-allocation value types.
    // Each event carries exactly what subscribers need, no more.
    // ---------------------------------------------------------------------------

    public readonly struct PlayerStateChangedPayload
    {
        public readonly PlayerState Previous;
        public readonly PlayerState Next;
        public PlayerStateChangedPayload(PlayerState previous, PlayerState next)
        {
            Previous = previous;
            Next = next;
        }
    }

    public readonly struct QTEZonePayload
    {
        public readonly QTESequenceData SequenceData;
        public QTEZonePayload(QTESequenceData sequenceData) => SequenceData = sequenceData;
    }

    public readonly struct QTEInputPayload
    {
        public readonly string ActionName;
        public readonly float HoldDuration;
        public readonly int MashCount;
        public QTEInputPayload(string actionName, float holdDuration = 0f, int mashCount = 0)
        {
            ActionName = actionName;
            HoldDuration = holdDuration;
            MashCount = mashCount;
        }
    }

    public readonly struct TimelinePayload
    {
        public readonly string TimelineName;
        public TimelinePayload(string timelineName) => TimelineName = timelineName;
    }

    public readonly struct CheckpointPayload
    {
        public readonly Vector3 RespawnPosition;
        public readonly Quaternion RespawnRotation;
        public readonly string CheckpointId;
        public CheckpointPayload(Vector3 position, Quaternion rotation, string id)
        {
            RespawnPosition = position;
            RespawnRotation = rotation;
            CheckpointId = id;
        }
    }

    // ---------------------------------------------------------------------------
    // GameEvents — static event declarations.
    // ---------------------------------------------------------------------------

    public static class GameEvents
    {
        // --- Player ---
        // Raised whenever the player state machine transitions between states.
        public static event Action<PlayerStateChangedPayload> OnPlayerStateChanged;
        //---Death----
        public static event Action<PlayerDeathPayload> OnPlayerDied;

        // Raised after the player has been moved to a checkpoint position.
        public static event Action<CheckpointPayload> OnPlayerRespawned;

        // --- QTE Zone ---
        // Raised by QTETriggerZone when the player enters its collider.
        // QTEManager subscribes and decides whether to start the sequence.
        public static event Action<QTEZonePayload> OnPlayerEnteredQTEZone;

        // --- QTE Lifecycle ---
        // Raised by QTEManager when it officially begins a sequence.
        public static event Action<QTEZonePayload> OnQTEStarted;

        // Raised each time valid QTE input is received during an active sequence.
        public static event Action<QTEInputPayload> OnQTEInputReceived;

        // Raised by QTEManager when the player completes the sequence successfully.
        public static event Action<QTEZonePayload> OnQTESucceeded;

        // Raised by QTEManager when the sequence fails (timeout or wrong input).
        public static event Action<QTEZonePayload> OnQTEFailed;

        // --- Timeline ---
        // Raised when a PlayableDirector begins playback.
        public static event Action<TimelinePayload> OnTimelineStarted;

        // Raised when a PlayableDirector stops (either finished or stopped early).
        public static event Action<TimelinePayload> OnTimelineFinished;

        // --- Checkpoint ---
        // Raised when the player activates a checkpoint trigger in the world.
        public static event Action<CheckpointPayload> OnCheckpointActivated;

        // ---------------------------------------------------------------------------
        // Raise helpers — null-safe invocation kept in one place.
        // Using the helper ensures we never forget the null check at call sites.
        // ---------------------------------------------------------------------------

        public static void RaisePlayerStateChanged(PlayerStateChangedPayload p)
            => OnPlayerStateChanged?.Invoke(p);

        public static void RaisePlayerRespawned(CheckpointPayload p)
            => OnPlayerRespawned?.Invoke(p);

        public static void RaisePlayerEnteredQTEZone(QTEZonePayload p)
            => OnPlayerEnteredQTEZone?.Invoke(p);

        public static void RaiseQTEStarted(QTEZonePayload p)
            => OnQTEStarted?.Invoke(p);

        public static void RaiseQTEInputReceived(QTEInputPayload p)
            => OnQTEInputReceived?.Invoke(p);

        public static void RaiseQTESucceeded(QTEZonePayload p)
            => OnQTESucceeded?.Invoke(p);

        public static void RaiseQTEFailed(QTEZonePayload p)
            => OnQTEFailed?.Invoke(p);

        public static void RaiseTimelineStarted(TimelinePayload p)
            => OnTimelineStarted?.Invoke(p);

        public static void RaiseTimelineFinished(TimelinePayload p)
            => OnTimelineFinished?.Invoke(p);

        public static void RaiseCheckpointActivated(CheckpointPayload p)
            => OnCheckpointActivated?.Invoke(p);
        public static void RaisePlayerDied(PlayerDeathPayload payload)
        {
            OnPlayerDied?.Invoke(payload);
        }



        // --------------------------------- Laser Based Events--------------------------------------------
        public static event Action OnLaserEntered;

        public static event Action OnLaserExited;

        public static event Action<LaserHitPayload> OnLaserHit;
        //-------------------------- Helpers --------------------------------------
        public static void RaiseLaserEntered()
            => OnLaserEntered?.Invoke();

        public static void RaiseLaserExited()
            => OnLaserExited?.Invoke();

        public static void RaiseLaserHit(LaserHitPayload p)
            => OnLaserHit?.Invoke(p);
    }
}