using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossParryParticleController : MonoBehaviour
{
    public DojoBossMecanimActor actor;
    public ParticleSystem circleParticle;
    public ParticleSystem circleSuccess;
    public ParticleSystem crossParticle;
    public ParticleSystem crossSuccess;
    bool parryIsCircle;
    // Start is called before the first frame update
    void Start()
    {
        actor.OnParrySuccess.AddListener(ParrySuccessParticle);
        actor.OnParryFail.AddListener(ParrySuccessParticle);
    }

    // Update is called once per frame
    void Update()
    {
        if (actor.IsParrying())
        {
            if (actor.IsCircleParrying())
            {
                if (!circleParticle.isPlaying)
                {
                    circleParticle.Play();
                }
                if (crossParticle.isPlaying)
                {
                    crossParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                parryIsCircle = true;
            }
            else
            {
                if (!crossParticle.isPlaying)
                {
                    crossParticle.Play();
                }
                if (circleParticle.isPlaying)
                {
                    circleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                parryIsCircle = false;
            }
        }
        else if (!actor.IsParrying())
        {
            if (crossParticle.isPlaying)
            {
                crossParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (circleParticle.isPlaying)
            {
                circleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

        }
    }

    public void ParrySuccessParticle()
    {
        if (parryIsCircle)
        {
            circleSuccess.Play();
        }
        else
        {
            crossSuccess.Play();
        }
    }
}
