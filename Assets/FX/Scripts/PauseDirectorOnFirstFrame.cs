using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class PauseDirectorOnFirstFrame : MonoBehaviour
{
    public int framesToWait = 2;
    int frames;

    private void Start()
    {
        frames = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (frames >= framesToWait)
        {
            PlayableDirector director = this.GetComponent<PlayableDirector>();
            director.Pause();
            this.enabled = false;
        }
        frames++;
    }
}
