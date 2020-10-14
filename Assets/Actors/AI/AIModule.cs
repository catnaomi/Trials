using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AIModule", menuName = "ScriptableObjects/AI/CreateModule", order = 1)]
public class AIModule : ScriptableObject
{
    public virtual void StartModule(NavigatingHumanoidActor ai)
    {
        // do nothing
    }

    // updates every few seconds, used for changing states
    public virtual void SlowUpdate(NavigatingHumanoidActor ai)
    {
        // do nothing
    }

    // updates every frame, used for reactions
    public virtual void FastUpdate(NavigatingHumanoidActor ai)
    {
        // do nothing
    }

    public virtual AIModule CloneModule()
    {
        // return a new instance of this module so that each may have different values and variables
        return new AIModule();
    }
}
