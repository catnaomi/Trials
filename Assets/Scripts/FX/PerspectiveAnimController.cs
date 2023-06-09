using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class PerspectiveAnimController : MonoBehaviour
{
    public AnimancerComponent hand;
    public AnimancerComponent body;
    public AvatarMask bodyMask;
    public TimeScaleController timeScale;
    AnimancerState handState;
    AnimancerState bodyState;
    [Header("Time Stop")]
    public ClipTransition timeStopAnim;
    public float timeScaleCurveDuration = 1f;
    public AnimationCurve timeScaleCurve;
    public UnityEvent TimeStopEvent;
    [Header("Time Resume")]
    public ClipTransition timeResumeAnim;
    // Start is called before the first frame update
    void Start()
    {
        body.Layers[1].SetWeight(1f);
        body.Layers[1].SetMask(bodyMask);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayTimeStop()
    {
        handState = hand.Play(timeStopAnim);
        handState.Events.OnEnd = HandStop;
        body.Layers[1].SetWeight(1f);
        bodyState = body.Layers[1].Play(timeStopAnim);
        bodyState.Events.OnEnd = BodyStop;
        StartCoroutine(TimeStopRoutine());
    }

    public void TimeStopSnap()
    {
        TimeStopEvent.Invoke();
    }

    IEnumerator TimeStopRoutine()
    {
        float clock = 0f;
        while (clock < timeScaleCurveDuration)
        {
            float t = Mathf.Clamp01(clock / timeScaleCurveDuration);
            timeScale.scale = timeScaleCurve.Evaluate(t);
            clock += 1 / 30f;
            yield return new WaitForSecondsRealtime(1 / 30f);
        }
        timeScale.scale = 1f;
    }

    public void PlayTimeResume()
    {
        handState = hand.Play(timeResumeAnim);
        handState.Events.OnEnd = HandStop;
        body.Layers[1].SetWeight(1f);
        bodyState = body.Layers[1].Play(timeResumeAnim);
        bodyState.Events.OnEnd = BodyStop;
    }

    void HandStop()
    {
        hand.Stop();
    }

    void BodyStop()
    {
        body.Layers[1].StartFade(0f, .5f);
    }
}
