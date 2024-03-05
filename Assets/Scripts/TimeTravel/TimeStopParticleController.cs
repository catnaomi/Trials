using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class TimeStopParticleController : MonoBehaviour
{
    public TimeTravelController timeTravelController;
    public float timeToFreeze = 0.5f;
    public float timeToUnfreeze = 2f;
    public float timeToFadeOut = 0.5f;
    [SerializeField, ReadOnly] int particleCount;
    public float maxPlayerAxisDistance = 20f;
    float clock;
    ParticleSystem system;
    Particle[] particles;
    [SerializeField, ReadOnly] ParticleState state;
    float speedMultiplierMax;
    float emissionTimeMax;
    bool isFrozen;
    enum ParticleState
    {
        Stopped,
        Started,
        Slowing,
        Frozen,
        Speeding,
        Ending
    }

    void Start()
    {
        system = this.GetComponent<ParticleSystem>();
        particles = new Particle[system.main.maxParticles];
        //timeToFreeze = timeTravelController.timeToOpenBubble;
        state = ParticleState.Stopped;
    }

    void LateUpdate()
    {
        //bool isFrozen = timeTravelController.IsFreezing();

        if (state == ParticleState.Stopped)
        {
            if (isFrozen)
            {
                system.Emit(150);
                system.Play();
                state = ParticleState.Started;
                clock = 0f;
            }
            else
            {
                if (system.isPlaying)
                {
                    system.Stop();
                }
            }
        }
        else if (state == ParticleState.Started)
        {
            if (isFrozen)
            {
                state = ParticleState.Slowing;
                speedMultiplierMax = system.main.simulationSpeed;
                //system.Stop();
            }
            else
            {
                state = ParticleState.Ending;
            }
        }
        else if (state == ParticleState.Slowing)
        {

            clock += Time.deltaTime;
            float t = Mathf.Clamp01(clock / timeToFreeze);


            var systemMain = system.main;
            systemMain.simulationSpeed = Mathf.Lerp(speedMultiplierMax, 0f, t);

            //var emission = system.emission;
            //emission.rateOverTimeMultiplier = Mathf.Lerp(emissionTimeMax, 0f, t);


            //SetParticles();
            if (clock >= timeToFreeze)
            {
                state = ParticleState.Frozen;
            }
        }
        else if (state == ParticleState.Frozen)
        {
            if (!isFrozen)
            {
                state = ParticleState.Speeding;
                clock = 0f;
            }


            /*
            if (system.isEmitting)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }*/

        }
        else if (state == ParticleState.Speeding)
        {
            clock += Time.deltaTime;
            float t = Mathf.Clamp01(clock / timeToUnfreeze);

            var systemMain = system.main;
            systemMain.simulationSpeed = Mathf.Lerp(0f, speedMultiplierMax, t);

            if (clock >= timeToUnfreeze)
            {
                state = ParticleState.Ending;
                //system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                clock = 0f;
            }
        }
        else if (state == ParticleState.Ending)
        {
            clock += Time.deltaTime;
            float t = Mathf.Clamp01(clock / timeToFadeOut);

            if (clock >= timeToFadeOut)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                state = ParticleState.Stopped;
            }

        }
        Vector3 midPoint = (PlayerActor.player.transform.position + Camera.main.transform.position) * 0.5f;
        this.transform.position = midPoint;
        if (system.IsAlive())
        {

            GetParticles();
            for (int i = 0; i < particleCount; i++)
            {
                Particle p = particles[i];
                float xOffset = p.position.x - Camera.main.transform.position.x;
                xOffset %= maxPlayerAxisDistance;
                float yOffset = p.position.y - Camera.main.transform.position.y;
                yOffset %= maxPlayerAxisDistance;
                float zOffset = p.position.z - Camera.main.transform.position.z;
                zOffset %= maxPlayerAxisDistance;

                Vector3 offset = p.position - midPoint;

                if (offset.magnitude > maxPlayerAxisDistance)
                {
                    p.position = Vector3.ClampMagnitude(-offset, maxPlayerAxisDistance) + midPoint;
                }
                //p.position = Camera.main.transform.position + new Vector3(xOffset, yOffset, zOffset);
                particles[i] = p;
            }
            SetParticles();
        }

    }

    public void StartFreeze()
    {
        isFrozen = true;
    }

    public void StopFreeze()
    {
        isFrozen = false;
    }

    void GetParticles()
    {
        if (system.isPlaying)
        {
            particleCount = system.GetParticles(particles);
        }
    }

    void SetParticles()
    {
        system.SetParticles(particles);
    }

    void BoundParticle(Particle p)
    {
        float xOffset = Camera.main.transform.position.x - p.position.x;
        xOffset %= maxPlayerAxisDistance;
        float yOffset = Camera.main.transform.position.y - p.position.y;
        yOffset %= maxPlayerAxisDistance;
        float zOffset = Camera.main.transform.position.z - p.position.z;
        zOffset %= maxPlayerAxisDistance;

        p.position = Camera.main.transform.position + new Vector3(xOffset, yOffset, zOffset);
    }
}
