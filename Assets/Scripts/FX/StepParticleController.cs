using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepParticleController : MonoBehaviour
{
    public static StepParticleController instance;
    AnimationFXHandler[] fxHandlers;
    public GameObject particlePrefab;
    public EventHandler<Vector3> StepEvent;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        fxHandlers = FindObjectsOfType<AnimationFXHandler>();
        if (fxHandlers == null || fxHandlers.Length == 0) //TODO : registration system for actors
        {
            foreach (AnimationFXHandler fxHandler in fxHandlers)
            {
                fxHandler.OnStepL.AddListener(() => { CreateParticle(fxHandler.footL); });
                fxHandler.OnStepR.AddListener(() => { CreateParticle(fxHandler.footR); });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateStep(Vector3 position)
    {
        Vector3 newPosition = position;
        newPosition.y = this.transform.position.y;
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
        if (source != null)
        {
            CreateStep(source.transform.position);
        }
    }
}
