// PlayerInputDisableTrack.cs
// Custom Timeline track that disables player input for the duration of a clip.
//
// DESIGN RATIONALE:
// During QTE cinematic sequences, Timeline must own all control. The cleanest way
// to communicate this to the InputReader from a Timeline is via a custom Playable
// Track, not a Timeline Signal (which fires a point event, not a duration).
//
// HOW IT WORKS:
// 1. Add PlayerInputDisableTrack to a Timeline.
// 2. Bind it to the InputReader ScriptableObject in the Track Bindings.
// 3. Place a PlayerInputDisableClip wherever input should be suspended.
// 4. The mixer calls DisableAllMaps() on enter and EnableGameplay() on exit.
//
// WHY BIND TO INPUTREADER DIRECTLY?
// InputReader is a ScriptableObject, which Timeline can bind to via
// ScriptableObjectBindingAttribute. This avoids a scene object reference in the
// Timeline asset, making the Timeline re-usable across scenes.
//
// Note: Timeline track bindings for ScriptableObjects require the binding type
// to be specified via [TrackBindingType]. This is supported in Unity 2021.3+.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using SciFiGame.Input;

namespace SciFiGame.Timeline
{
    // ---------------------------------------------------------------------------
    // Clip — the draggable block inside the Timeline Editor
    // ---------------------------------------------------------------------------

    public class PlayerInputDisableClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<PlayerInputDisableBehaviour>.Create(graph);
        }
    }

    // ---------------------------------------------------------------------------
    // Behaviour — runs each frame while the clip is active
    // ---------------------------------------------------------------------------

    public class PlayerInputDisableBehaviour : PlayableBehaviour
    {
        private InputReader _inputReader;

        public override void OnGraphStart(Playable playable) { }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _inputReader = playerData as InputReader;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            _inputReader?.DisableAllMaps();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Restore gameplay input when the clip ends or the Timeline stops.
            _inputReader?.EnableGameplay();
        }
    }

    // ---------------------------------------------------------------------------
    // Track — the lane in the Timeline Editor that holds clips
    // ---------------------------------------------------------------------------

    [TrackColor(0.8f, 0.2f, 0.2f)]
    [TrackClipType(typeof(PlayerInputDisableClip))]
    [TrackBindingType(typeof(InputReader))]
    public class PlayerInputDisableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayerInputDisableBehaviour>.Create(graph, inputCount);
        }
    }
}
