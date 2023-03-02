using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepParticleController : MonoBehaviour
{
    AnimationFXHandler[] fxHandlers;
    public GameObject particlePrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fxHandlers == null || fxHandlers.Length == 0) //TODO : registration system for actors
        {
            fxHandlers = FindObjectsOfType<AnimationFXHandler>();
            foreach (AnimationFXHandler fxHandler in fxHandlers)
            {
                fxHandler.OnStepL.AddListener(() => { CreateParticle(fxHandler.footL); });
                fxHandler.OnStepR.AddListener(() => { CreateParticle(fxHandler.footR); });
            }
        }
    }

    public void CreateParticle(Transform source)
    {
        if (source != null)
        {
            Vector3 position = source.transform.position;
            position.y = this.transform.position.y;
            GameObject particle = Instantiate(particlePrefab, position, particlePrefab.transform.rotation);
            particle.SetActive(true);
        }
    }
}
