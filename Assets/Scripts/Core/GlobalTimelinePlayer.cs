using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SciFiGame.Timeline
{
    [RequireComponent(typeof(PlayableDirector))]
    public class GlobalTimelinePlayer : MonoBehaviour
    {
        private PlayableDirector _director;

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }

        public void Play(TimelineAsset asset)
        {
            if (asset == null)
                return;

            _director.Stop();

            _director.time = 0;

            _director.playableAsset = asset;

            _director.Evaluate();

            _director.Play();
        }
    }
}