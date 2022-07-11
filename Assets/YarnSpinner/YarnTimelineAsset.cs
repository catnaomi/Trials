using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Yarn.Unity;

public class YarnTimelineAsset : PlayableAsset
{
    public YarnProject yarnProject;
    public string node;
    public bool pauseOnStart;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<YarnTimelineBehaviour>.Create(graph);

        var yarnTimelineBehaviour = playable.GetBehaviour();
        yarnTimelineBehaviour.yarnProject = yarnProject;
        yarnTimelineBehaviour.node = node;
        yarnTimelineBehaviour.pauseOnStart = pauseOnStart;

        return playable;
    }
}
