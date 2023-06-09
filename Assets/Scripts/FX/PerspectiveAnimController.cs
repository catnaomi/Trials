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
    public TimeScaleController timeScale;
    [Header("Time Stop")]
    public ClipTransition timeStopAnim;
    public float timeScaleCurveDuration = 1f;
    public AnimationCurve timeScaleCurve;
    public UnityEvent TimeStopEvent;
    [Header("Time Resume")]
    public ClipTransition timeResumeAnim;
    [Header("Other Animations")]
    public ClipTransition bodyIdle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayTimeStop()
    {
        hand.Play(timeStopAnim).Events.OnEnd = HandStop;
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
        hand.Play(timeResumeAnim).Events.OnEnd = HandStop;
    }

    void HandStop()
    {
        hand.Stop();
    }
}
