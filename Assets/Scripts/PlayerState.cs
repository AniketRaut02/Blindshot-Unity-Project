// PlayerState.cs
// Shared enum that defines every discrete state the player can occupy.
//
// DESIGN RATIONALE:
// Keeping this in a standalone file avoids circular dependencies between
// PlayerStateMachine, AnimationComponent, and GameEvents. Any system that
// needs to know the player's state imports this type, not the full component.
//
// States are intentionally minimal for Phase 1. Phase 2 will add states such
// as InLaserZone or HitByLaser without touching this file's existing entries.

namespace SciFiGame.Core
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Crouching,
        CrouchWalking,
        InQTE,         // Player is mid-QTE; input is surrendered to QTEManager.
        InLaserZone,
        Dead,          // Triggered on QTE failure; brief state before respawn.
    }
}
