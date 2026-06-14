// TimelineController.cs
// Listens to PlayableDirector stopped/played callbacks and relays them to GameEvents.
//
// DESIGN RATIONALE:
// PlayableDirector exposes UnityEvents (played, paused, stopped) on each director
// instance. Rather than every system subscribing to every director's UnityEvent,
// this component acts as an adapter: attach it to any PlayableDirector, and it
// will raise the game-wide TimelineStarted / TimelineFinished events.
//
// This keeps the event bus clean. QTEManager, CheckpointManager, and UI only
// subscribe to GameEvents — they never hold director references themselves.
//
// WHY PER-DIRECTOR COMPONENT AND NOT A SINGLE GLOBAL WATCHER?
// A global watcher would need to track all directors in the scene. A per-director
// component is added exactly where needed, is self-contained, and survives scene
// changes without modification.
//
// USAGE:
// Add TimelineController to any GameObject that has a PlayableDirector.
// Wire the PlayableDirector field in the Inspector (or let Awake find it).

using UnityEngine;
using UnityEngine.Playables;
using SciFiGame.Core;

namespace SciFiGame.Timeline
{
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------------------

        private PlayableDirector _director;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }

        private void OnEnable()
        {
            _director.played += OnPlayed;
            _director.stopped += OnStopped;
        }

        private void OnDisable()
        {
            _director.played -= OnPlayed;
            _director.stopped -= OnStopped;
        }

        // ---------------------------------------------------------------------------
        // Director callbacks
        // ---------------------------------------------------------------------------

        private void OnPlayed(PlayableDirector director)
        {
            GameEvents.RaiseTimelineStarted(new TimelinePayload(director.name));
        }

        private void OnStopped(PlayableDirector director)
        {
            Debug.Log("Timeline finished");
            GameEvents.RaiseTimelineFinished(new TimelinePayload(director.name));
        }

        // ---------------------------------------------------------------------------
        // Public helpers — lets other systems play this director via code without
        // holding a direct reference to the PlayableDirector.
        // ---------------------------------------------------------------------------

        public void Play()  => _director.Play();
        public void Stop()  => _director.Stop();
        public void Pause() => _director.Pause();
    }
}
