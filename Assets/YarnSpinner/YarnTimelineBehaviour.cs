using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Yarn.Unity;

public class YarnTimelineBehaviour : PlayableBehaviour
{
    public YarnProject yarnProject;
    public string node;
    public bool pauseOnStart;
    public bool zeroSpeedOnStart;
    public bool setPositionOnFinish = false;
    public float timelinePosition = -1;
    bool started;
    double speed;
    DialogueRunner runner;
    PlayableDirector director;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        runner = playerData as DialogueRunner;
        if (runner == null) return;

        director = playable.GetGraph().GetResolver() as PlayableDirector;
       
        if (!started)
        {


            runner.onDialogueComplete.AddListener(OnDialogueComplete);
            //runner.onNodeComplete.AddListener(OnNodeEnd);
            runner.onNodeStart.AddListener(OnNodeStart);
            if (node != "_pause")
            {
                if (runner.CheckDialogueRunning()) runner.Stop();
                runner.GetComponent<LineActorPositioningHandler>()?.SetSpeaker(null, null);

                runner.StartDialogueWhenAble(node);
            }
            else if (runner.CheckDialogueRunning())
            {
                OnNodeStart(node);
            }
            
            started = true;
        }
    }

    void OnNodeStart(string node)
    {
        if (pauseOnStart)
        {
            director.Pause();
        }
        else if (zeroSpeedOnStart)
        {
            if (director == null || !director.playableGraph.IsValid()) return;
            speed = director.playableGraph.GetRootPlayable(0).GetSpeed();
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        }
    }

    void OnDialogueComplete()
    {
        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        runner.onNodeStart.RemoveListener(OnNodeStart);
        if (pauseOnStart)
        {
            director.Resume();
        }
        else if (zeroSpeedOnStart)
        {
            if (director != null && director.playableGraph.IsValid())
            {
                director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
            }
            
        }
        if (setPositionOnFinish)
        {
            if (director.time < timelinePosition)
            {
                FastForwardTimelineToPosition(timelinePosition);
            }
        }
    }


    void FastForwardTimelineToPosition(float targetPosition)
    {
        float RATE = 1 / 20f;
        while (director.time < targetPosition)
        {
            director.Evaluate();
            director.time += RATE;
        }
    }
}
