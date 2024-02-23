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
    bool didAddListeners;
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
            
            if (node != "_pause")
            {
                if (runner.CheckDialogueRunning()) runner.Stop();
                AddListeners();
                runner.StartDialogueWhenAble(node);
            }
            else if (runner.CheckDialogueRunning())
            {
                RemoveListeners();
                OnNodeStart(node);
            }
            
            started = true;
        }
    }

    void AddListeners()
    {
        if (runner != null)
        {
            runner.onDialogueComplete.AddListener(OnDialogueComplete);
            runner.onNodeStart.AddListener(OnNodeStart);
        }
    }

    void RemoveListeners()
    {
        if (runner != null)
        {
            runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
            runner.onNodeStart.RemoveListener(OnNodeStart);
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
        RemoveListeners();
    }


    void FastForwardTimelineToPosition(float targetPosition)
    {
        float RATE = 1 / 20f;
        while (director.time < targetPosition)
        {
            director.Evaluate();
            director.time += RATE;
        }
        director.Resume();
        if (director.playableGraph.GetRootPlayable(0).GetSpeed() <= 0)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }
}
