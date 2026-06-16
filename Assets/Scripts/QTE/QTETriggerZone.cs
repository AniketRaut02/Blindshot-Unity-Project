// QTETriggerZone.cs
// Trigger collider that raises OnPlayerEnteredQTEZone when the player enters.
//
// DESIGN RATIONALE:
// The zone's sole responsibility is spatial detection. It does not start QTEs,
// does not track state, does not reference QTEManager. This separates the concern
// of "detecting entry" from the concern of "deciding what to do about it".
//
// QTEManager subscribes to OnPlayerEnteredQTEZone and decides whether to start
// the sequence (e.g., it might ignore the event if another QTE is already running).
// If in future we need conditional zones (requires item, requires level flag) we
// add that here, not in QTEManager.
//
// ONE-SHOT:
// By default the zone deactivates its collider after the first trigger so it
// cannot be re-entered mid-sequence. Set AllowReactivation = true for zones that
// can reset (e.g., training rooms). The zone listens for OnPlayerRespawned to know
// whether to re-arm itself after a death/failure.
//
// TAG:
// The zone compares the collider tag to "Player" to avoid responding to physics
// objects or enemies. Make sure the Player GameObject has the "Player" tag.

using UnityEngine;
using SciFiGame.Core;

namespace SciFiGame.QTE
{
    [RequireComponent(typeof(Collider))]
    public class QTETriggerZone : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [Tooltip("The QTE sequence this zone initiates.")]
        [SerializeField] private QTESequenceData _sequenceData;

        [Tooltip("If true, the zone re-arms itself after a player death/respawn so they can retry.")]
        [SerializeField] private bool _allowReactivation = true;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private Collider _collider;
        private bool _triggered;
        private bool _isCompleted; // Tracks if the player successfully beat it

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            if (_sequenceData == null)
                Debug.LogError($"[QTETriggerZone] No QTESequenceData assigned on {name}.", this);
        }

        private void OnEnable()
        {
            // FIX: Listen for respawns instead of the deprecated OnQTEFailed event
            GameEvents.OnPlayerRespawned += OnPlayerRespawned;
            GameEvents.OnQTESucceeded += OnQTESucceeded;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerRespawned -= OnPlayerRespawned;
            GameEvents.OnQTESucceeded -= OnQTESucceeded;
        }

        // ---------------------------------------------------------------------------
        // Trigger detection
        // ---------------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (_sequenceData == null) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            _collider.enabled = false;   // prevent re-trigger mid-sequence

            GameEvents.RaisePlayerEnteredQTEZone(new QTEZonePayload(_sequenceData));
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnPlayerRespawned(CheckpointPayload _)
        {
            // If the player already beat this specific QTE, leave it permanently disabled.
            if (_isCompleted) return;

            // If they died/failed and reactivation is allowed, re-arm the zone!
            if (_allowReactivation)
            {
                _triggered = false;
                _collider.enabled = true;
            }
        }

        private void OnQTESucceeded(QTEZonePayload payload)
        {
            // Successful QTE — this zone is now permanently beaten.
            if (payload.SequenceData != _sequenceData) return;

            _isCompleted = true;
        }
    }
}