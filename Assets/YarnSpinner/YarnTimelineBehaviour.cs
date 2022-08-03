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
    bool started;
    bool paused;
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
            if (runner.IsDialogueRunning) runner.Stop();
            runner.GetComponent<LineActorPositioningHandler>()?.SetSpeaker(null, null);

            runner.StartDialogue(node);

            
            started = true;
        }
    }

    void OnNodeStart(string node)
    {
        if (pauseOnStart) director.Pause();
    }

    void OnDialogueComplete()
    {
        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        runner.onNodeStart.RemoveListener(OnNodeStart);
        if (pauseOnStart) director.Resume();
    }
}
