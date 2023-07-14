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
        Vector3 newPosition = position;
        newPosition.y = heightReference.position.y;
        GameObject particle = Instantiate(particlePrefab, newPosition, particlePrefab.transform.rotation);
        particle.SetActive(true);
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
