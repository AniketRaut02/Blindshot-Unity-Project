// QTESequenceData.cs
// ScriptableObject that fully describes one QTE sequence.
//
// DESIGN RATIONALE:
// The entire data contract for a QTE lives in this asset. QTEManager reads it,
// QTEPromptWidget displays from it, QTETriggerZone references it. No QTE logic
// is hardcoded anywhere — adding a new QTE means creating a new asset, not
// touching existing scripts.
//
// TIMELINE REFERENCES:
// Success and Failure timelines are references to PlayableDirector components
// in the scene. This is the one place we intentionally use scene references
// because a Timeline is a scene-specific cinematic sequence, not a data asset.
// An alternative (Timeline asset reference + dynamic director) was considered
// but adds indirection with no benefit at this scope.
//
// MASH THRESHOLD:
// For Mash QTEs, MashThreshold is the required number of presses. This is data
// so designers can tune difficulty per-QTE without code.
//
// HOLD TOLERANCE:
// For Hold QTEs, the player must hold for at least (TimeLimit * HoldTolerance)
// seconds. 1.0 = must hold the full duration. 0.8 = 80% of the time limit.
//
// CREATE: Right-click in Project → SciFiGame → QTE → QTE Sequence Data

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SciFiGame.QTE
{
    [CreateAssetMenu(fileName = "QTESequence_New", menuName  = "SciFiGame/QTE/QTE Sequence Data")]
    public class QTESequenceData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-readable name used in logs and UI headers.")]
        public string SequenceName = "Unnamed QTE";

        [Header("Input")]
        public QTEInputType InputType = QTEInputType.SinglePress;

        [Tooltip("The InputConstants action name for SinglePress, Hold, and Mash types.")]
        public string ActionName = "Confirm";

        [Tooltip("Sequence steps — only used when InputType = Sequence.")]
        public QTESequenceStep[] Steps;

        [Header("Timing")]
        [Tooltip("Total time in seconds the player has to complete the QTE.")]
        public float TimeLimit = 3f;

        [Header("Hold Settings")]
        [Tooltip("Fraction of TimeLimit the player must hold (for Hold QTEs). Range 0–1.")]
        [Range(0f, 1f)]
        public float HoldTolerance = 0.9f;

        [Header("Mash Settings")]
        [Tooltip("Required number of presses to succeed (for Mash QTEs).")]
        public int MashThreshold = 8;

        [Header("UI")]
        [Tooltip("Icon displayed in the QTE prompt widget for the main action.")]
        public Sprite PromptIcon;

        [Header("Timelines (Scene References)")]
        [Tooltip("PlayableDirector to play when the QTE succeeds.")]
        public TimelineAsset SuccessDirector;

        [Tooltip("PlayableDirector to play when the QTE fails.")]
        public TimelineAsset FailureDirector;

        [Header("Failure")]
        public QTEFailureOutcome FailureOutcome = QTEFailureOutcome.Respawn;
    }
}
