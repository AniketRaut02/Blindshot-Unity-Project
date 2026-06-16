// QTETypes.cs
// Shared enums and value types for the QTE system.
//
// DESIGN RATIONALE:
// Placing all QTE type definitions here prevents circular dependencies between
// QTESequenceData (ScriptableObject), QTEManager, and QTEPromptWidget. Any file
// that needs QTE types imports this; none of them import each other.

namespace SciFiGame.QTE
{
    // The kind of player interaction required for a QTE step.
    public enum QTEInputType
    {
        SinglePress,    // Tap the key once. E.g., press E.
        Hold,           // Hold the key until the timer expires. E.g., hold Space.
        Mash,           // Press the key rapidly N times within the time limit.
        Sequence,       // Press a specific sequence of keys in order.
    }

    // What happens to the player if the QTE sequence is failed.
    public enum QTEFailureOutcome
    {
        Respawn,        // Play fail Timeline, then respawn at last checkpoint.
        Damage,         // Reserved for Phase 2 (laser damage system).
    }

    // A single step within a Sequence-type QTE.
    [System.Serializable]
    public struct QTESequenceStep
    {
        // The InputConstants action name this step expects. e.g., InputConstants.QTELeft.
        public string ActionName;

        [UnityEngine.Tooltip("Icon to display for this specific step.")]
        public UnityEngine.Sprite StepIcon;
    }
}
