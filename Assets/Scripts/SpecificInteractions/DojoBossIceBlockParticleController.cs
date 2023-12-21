using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossIceBlockParticleController : MonoBehaviour
{
    public ParticleSystem iceBlockParticle;
    public ParticleSystem destroyParticle;
    Vector3 initialScale;
    public float expandTime = 0.5f;
    float clock = 0f;
    bool expanding;
    bool started;
    public bool Playing { get { return started; } }
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Vector2 scaleMinMax = new Vector2(0.01f, 1);
    bool addedListener;
    // Start is called before the first frame update
    void Start()
    {
        initialScale = iceBlockParticle.transform.localScale;
        if (PlayerActor.player != null)
        {
            PlayerActor.player.OnHurt.AddListener(StopParticle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerActor.player != null)
        {
            this.transform.position = PlayerActor.player.transform.position;
        }
        if (expanding)
        {

            //iceBlockParticle.transform.localScale
            float t = Mathf.Clamp01(clock / expandTime);
            iceBlockParticle.transform.localScale = initialScale * Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, expandCurve.Evaluate(t));
            if (clock >= expandTime)
            {
                expanding = false;
            }
            clock += Time.deltaTime;
        }
    }

    public void StartParticle()
    {
        iceBlockParticle.Play();
        expanding = true;
        clock = 0f;
        started = true;

    }

    public void StopParticle()
    {
        
        iceBlockParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (started)
        {
            destroyParticle.Play();
        }
        started = false;
    }
}
