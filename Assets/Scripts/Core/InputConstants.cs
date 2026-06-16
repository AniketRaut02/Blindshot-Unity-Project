// InputConstants.cs
// String constants that mirror the Input Action Asset names.
//
// DESIGN RATIONALE:
// The Unity Input System uses string identifiers for Action Maps and Actions.
// Scattering raw strings across components makes refactoring painful and
// introduces silent runtime bugs when names diverge. Centralising them here
// means a rename requires one edit only, and the compiler catches stale references.
//
// These match exactly what you name the bindings inside your .inputactions asset.
// If you rename an action there, update it here — a TODO comment marks each one.

namespace SciFiGame.Input
{
    public static class InputConstants
    {
        // ---------- Action Map names ----------
        public const string MapGameplay = "Gameplay";
        public const string MapQTE      = "QTE";
        public const string MapUI       = "UI";

        // ---------- Gameplay Actions ----------
        public const string Move        = "Move";       // Vector2 composite (WASD / left stick)
        public const string Look        = "Look";       // Vector2 (mouse delta / right stick)
        public const string Crouch      = "Crouch";     // Button (C / left stick click)
        public const string Interact    = "Interact";   // Button (E / South)
        public const string Escape      = "Escape";

        // ---------- QTE Actions ----------
        // Each action corresponds to a possible QTE prompt.
        // Keep these identical to the Action names in the QTE Action Map.
        public const string QTEConfirm  = "Confirm";   // E / South — single press
        public const string QTEHold     = "Hold";      // Space / West — hold
        public const string QTEMash     = "Mash";      // F / East — mash
        public const string QTELeft     = "Left";      // A / Left shoulder — sequence step
        public const string QTERight    = "Right";     // D / Right shoulder — sequence step
        public const string QTEJump     = "Jump";      // Space / South — sequence step
    }
}
