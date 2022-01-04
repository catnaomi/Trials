using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntelligentActor : NavigatingHumanoidActor
{

    [Header("AI Settings")]
    public List<AIModule> modules;
    public float ModuleUpdateFrequency = 2f;
    float updateClock;
    public override void ActorStart()
    {
        base.ActorStart();

        List<AIModule> moduleClones = new List<AIModule>();
        foreach(AIModule module in modules)
        {
            AIModule newModule = module.CloneModule();
            moduleClones.Add(newModule);
            newModule.StartModule(this);
        }

        modules = moduleClones;

        updateClock = Random.Range(0, ModuleUpdateFrequency);
    }

    public override void ActorPreUpdate()
    {
        base.ActorPreUpdate();

        updateClock += Time.deltaTime;
        foreach (AIModule module in modules)
        {
            if (updateClock > ModuleUpdateFrequency)
            {
                module.SlowUpdate(this);
                updateClock = 0f;
            }
            module.FastUpdate(this);
        }
        
    }
}
