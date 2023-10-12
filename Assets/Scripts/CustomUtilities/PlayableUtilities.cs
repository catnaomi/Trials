using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomUtilities
{
    public static class PlayableUtilities
    {
        public static List<PlayableBinding> GetTimelineBindings(this PlayableDirector director)
        {
            TimelineAsset timeline = (TimelineAsset)director.playableAsset;
            List<PlayableBinding> bindings = new List<PlayableBinding>();
            foreach (var binding in timeline.outputs)
            {
                bindings.Add(binding);
            }
            return bindings;
        }
    }
}