using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionStepEvent : MonoBehaviour
{
    ParticleSystem particle;
    List<ParticleCollisionEvent> collisionEvents;
    // Start is called before the first frame update
    void Start()
    {
        particle = this.GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }


    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);

        
        for (int i = 0; i < numCollisionEvents; i++)
        {
            StepParticleController.CreateStepGlobal(collisionEvents[i].intersection);
        }
    }
}
