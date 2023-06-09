using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Yarn.Unity;

public class YarnTimelineAsset : PlayableAsset
{
    public YarnProject yarnProject;
    public string node;
    [Tooltip("Pauses the entire Timeline, freezing all animations")]
    public bool pauseOnStart;
    [Tooltip("Sets Timeline speed to zero, but extrapolate animations continue")]
    public bool zeroSpeedOnStart;
    [Space(20)]
    public bool setPositionOnFinish = false;
    public float timelinePosition = -1;


    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<YarnTimelineBehaviour>.Create(graph);

        var yarnTimelineBehaviour = playable.GetBehaviour();
        yarnTimelineBehaviour.yarnProject = yarnProject;
        if ((node == "" || node == "_fillfromdirector") && owner.TryGetComponent<YarnTimelineDialogueReference>(out YarnTimelineDialogueReference reference))
        {
            yarnTimelineBehaviour.node = reference.node;
        }
        else
        {
            yarnTimelineBehaviour.node = node;
        }
        
        yarnTimelineBehaviour.pauseOnStart = pauseOnStart;
        yarnTimelineBehaviour.zeroSpeedOnStart = zeroSpeedOnStart;
        yarnTimelineBehaviour.setPositionOnFinish = setPositionOnFinish;
        yarnTimelineBehaviour.timelinePosition = timelinePosition;
        return playable;
    }
}
