// LaserPayloads.cs
// Shared payload structs for the laser system.

using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace SciFiGame.Laser
{
   
    public readonly struct LaserHitPayload
    {
        public readonly string BeamName;

        public LaserHitPayload(string beamName)
        {
            BeamName = beamName;
        }
    }
    public readonly struct PlayerDeathPayload
    {
        public readonly TimelineAsset DeathTimeline;

        public PlayerDeathPayload( TimelineAsset deathTimeline)
        {
            DeathTimeline = deathTimeline;
        }
    }
}