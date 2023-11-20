using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepParticleController : MonoBehaviour
{
    public static StepParticleController instance;
    HashSet<AnimationFXHandler> fxHandlers;
    public GameObject particlePrefab;
    public EventHandler<Vector3> StepEvent;
    public Transform heightReference;
    public List<ParticleSystem> ripples;
    public int maxRipples = -1;
    int poolIndex;
    GenericTimeTravelHandler timeTravelController;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
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
        timeTravelController = this.GetComponent<GenericTimeTravelHandler>();

        timeTravelController.OnStartFreeze.AddListener(FreezeAll);
        timeTravelController.OnStopFreeze.AddListener(UnFreezeAll);

    }

    /*
    void ListenLeftFoot(AnimationFXHandler fxHandler)
    {
        if (!fxHandlers.Contains(fxHandler)) return;
        CreateParticle(fxHandler.footL);
    }

    void ListenRightFoot(AnimationFXHandler fxHandler)
    {
        if (!fxHandlers.Contains(fxHandler)) return;
        CreateParticle(fxHandler.footR);
    }
    */
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
        fxHandler.OnStepR.AddListener(() => CreateParticle(fxHandler.footR));
        fxHandler.OnStepL.AddListener(() => CreateParticle(fxHandler.footL));
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
