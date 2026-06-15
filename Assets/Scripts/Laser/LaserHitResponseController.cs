// LaserHitResponseController.cs
//
// DESIGN RATIONALE:
// LaserBeam only detects collisions.
// This controller determines what a laser hit means.
//
// Currently, a laser hit kills the player.

using UnityEngine;
using UnityEngine.Timeline;
using SciFiGame.Core;

namespace SciFiGame.Laser
{
    public class LaserHitResponseController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField]
        private TimelineAsset _deathTimeline;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            GameEvents.OnLaserHit += OnLaserHit;
        }

        private void OnDisable()
        {
            GameEvents.OnLaserHit -= OnLaserHit;
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnLaserHit(LaserHitPayload payload)
        {
            Debug.Log( $"Player killed by laser: {payload.BeamName}");

            GameEvents.RaisePlayerDied(
                new PlayerDeathPayload( _deathTimeline));
        }
    }
}