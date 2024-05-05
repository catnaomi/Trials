using System.Collections.Generic;
using UnityEngine;

public class StepParticleController : MonoBehaviour
{
    public static StepParticleController instance;
    HashSet<AnimationFXHandler> fxHandlers;
    public GameObject particlePrefab;
    public GameObject slideParticlePrefab;
    public Transform heightReference;
    public List<ParticleSystem> ripples;
    public Dictionary<AnimationFXHandler, ParticleSystem> slideRipples;
    public int maxRipples = -1;
    int poolIndex;
    GenericTimeTravelHandler timeTravelController;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Collider c = this.GetComponent<Collider>();
        Collider[] colliders = Physics.OverlapBox(c.bounds.center, c.bounds.extents);

        fxHandlers = new HashSet<AnimationFXHandler>();
        foreach (Collider other in colliders)
        {
            if (other.TryGetComponent(out AnimationFXHandler fxHandler))
            {
                Register(fxHandler);
            }
        }
        ripples = new List<ParticleSystem>(maxRipples > 0 ? maxRipples : 64);
        slideRipples = new Dictionary<AnimationFXHandler, ParticleSystem>();
        timeTravelController = this.GetComponent<GenericTimeTravelHandler>();

        timeTravelController.OnStartFreeze.AddListener(FreezeAll);
        timeTravelController.OnStopFreeze.AddListener(UnFreezeAll);
    }

    void Update()
    {
        if (slideRipples == null)
        {
            return;
        }

        foreach (AnimationFXHandler fxHandler in slideRipples.Keys)
        {
            if (slideRipples.TryGetValue(fxHandler, out ParticleSystem system))
            {
                if (system != null && system.isPlaying)
                {
                    Vector3 pos = fxHandler.transform.position;
                    pos.y = heightReference.position.y;

                    system.transform.position = pos;
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out AnimationFXHandler fxHandler))
        {
            Register(fxHandler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out AnimationFXHandler fxHandler))
        {
            Deregister(fxHandler);
        }
    }

    public void Register(AnimationFXHandler fxHandler)
    {
        if (fxHandlers.Contains(fxHandler)) return;
        fxHandlers.Add(fxHandler);
        for (int footIndex = 0; footIndex < 2; footIndex++)
        {
            var foot = fxHandler.feet[footIndex];
            fxHandler.OnStep[footIndex].AddListener(() => CreateParticle(foot));
            fxHandler.OnStep[footIndex].AddListener(() => CreateParticle(foot));
        }

        fxHandler.OnSlideStart.AddListener(() => StartSlide(fxHandler));
        fxHandler.OnSlideEnd.AddListener(() => StopSlide(fxHandler));
    }

    public void Deregister(AnimationFXHandler fxHandler)
    {
        fxHandlers.Remove(fxHandler);
    }

    public void CreateStep(Vector3 position)
    {
        if (maxRipples <= 0 || ripples.Count < maxRipples)
        {
            Vector3 newPosition = position;
            newPosition.y = heightReference.position.y;
            GameObject particle = Instantiate(particlePrefab, newPosition, particlePrefab.transform.rotation);

            ParticleSystem system = particle.GetComponent<ParticleSystem>();
            particle.SetActive(true);
            if (CheckFreeze(system))
            {
                system.Pause();
            }
            else
            {
                system.Play();
            }
            ripples.Add(system);
        }
        else
        {
            RecycleStep(position);
        }
    }

    public void RecycleStep(Vector3 position)
    {
        ParticleSystem system = ripples[poolIndex];
        Vector3 newPosition = position;
        newPosition.y = heightReference.position.y;
        GameObject particle = system.gameObject;
        particle.transform.position = newPosition;

        particle.SetActive(true);
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (CheckFreeze(system))
        {
            system.Play();
            system.Pause();
        }
        else
        {
            system.Play();
        }
        

        poolIndex ++;
        poolIndex %= maxRipples;
    }

    public ParticleSystem CreateSlide(AnimationFXHandler fxHandler)
    {
        GameObject particle = Instantiate(slideParticlePrefab, fxHandler.transform.position, slideParticlePrefab.transform.rotation);

        ParticleSystem system = particle.GetComponent<ParticleSystem>();
        particle.SetActive(true);

        slideRipples[fxHandler] = system;

        return system;
    }

    public void StartSlide(AnimationFXHandler fxHandler)
    {
        ParticleSystem system = null;
        if (!slideRipples.TryGetValue(fxHandler, out system))
        {
            system = CreateSlide(fxHandler);
        }

        Vector3 pos = fxHandler.transform.position;
        pos.y = heightReference.position.y;

        system.transform.position = pos;

        system.Play();
    }

    public void StopSlide(AnimationFXHandler fxHandler)
    {
        ParticleSystem system = null;
        if (!slideRipples.TryGetValue(fxHandler, out system))
        {
            return;
        }

        system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public bool CheckFreeze(ParticleSystem system)
    {
        if (timeTravelController != null && timeTravelController.IsFrozen())
        {
            return true;
        }
        return false;
    }

    public void FreezeAll()
    {
        foreach (ParticleSystem system in ripples)
        {
            if (system == null) continue;
            system.Pause();
        }

        foreach (ParticleSystem ssystem in slideRipples.Values)
        {
            if (ssystem == null) continue;
            ssystem.Pause();
            
        }
    }

    public void UnFreezeAll()
    {
        foreach (ParticleSystem system in ripples)
        {
            if (system == null) continue;
            if (!system.isStopped)
            {
                system.Play();
            }
        }

        foreach (ParticleSystem ssystem in slideRipples.Values)
        {
            if (ssystem == null) continue;
            if (!ssystem.isStopped)
            {
                ssystem.Play();
            }
        }
    }

    public static void CreateStepGlobal(Vector3 position)
    {
        if (instance != null)
        {
            instance.CreateStep(position);
        }
    }

    public void CreateParticle(Transform source)
    {
        if (source != null && this.enabled && this.gameObject.activeInHierarchy)
        {
            CreateStep(source.transform.position);
        }
    }
}
