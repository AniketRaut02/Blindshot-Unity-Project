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
// can reset (e.g., training rooms). The zone listens for OnQTEFailed to know
// whether to re-arm itself.
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

        [Tooltip("If true, the zone re-arms itself after a failed QTE so the player can retry.")]
        [SerializeField] private bool _allowReactivation = true;

        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private Collider _collider;
        private bool     _triggered;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _collider           = GetComponent<Collider>();
            _collider.isTrigger = true;

            if (_sequenceData == null)
                Debug.LogError($"[QTETriggerZone] No QTESequenceData assigned on {name}.", this);
        }

        private void OnEnable()
        {
            GameEvents.OnQTEFailed    += OnQTEFailed;
            GameEvents.OnQTESucceeded += OnQTESucceeded;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEFailed    -= OnQTEFailed;
            GameEvents.OnQTESucceeded -= OnQTESucceeded;
        }

        // ---------------------------------------------------------------------------
        // Trigger detection
        // ---------------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)               return;
            if (_sequenceData == null)    return;
            if (!other.CompareTag("Player")) return;

            _triggered         = true;
            _collider.enabled  = false;   // prevent re-trigger mid-sequence

            GameEvents.RaisePlayerEnteredQTEZone(new QTEZonePayload(_sequenceData));
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnQTEFailed(QTEZonePayload payload)
        {
            // Only re-arm if this zone's sequence failed and reactivation is allowed.
            if (payload.SequenceData != _sequenceData) return;
            if (!_allowReactivation) return;

            _triggered        = false;
            _collider.enabled = true;
        }

        private void OnQTESucceeded(QTEZonePayload payload)
        {
            // Successful QTE — this zone stays permanently disabled.
            if (payload.SequenceData != _sequenceData) return;
            _triggered = true;
        }
    }
}
