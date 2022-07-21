using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class PoseDialogueNPCActor : NavigatingHumanoidActor
{
    [Header("Pose Animation")]
    public ClipTransition pose;
    public bool forcePoseInEditor;
    AnimancerState poseState;

    public override void ActorStart()
    {
        base.ActorStart();
        poseState = animancer.States.GetOrCreate(pose);
        animancer.Play(poseState);
    }

    private void OnValidate()
    {
        if (forcePoseInEditor && pose != null)
        {
            if (TryGetComponent<AnimancerComponent>(out animancer))
            {
                poseState = animancer.States.GetOrCreate(pose);
                animancer.Play(poseState);
            }
        }
    }
}
