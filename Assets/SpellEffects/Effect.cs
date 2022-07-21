using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Status Effects/Create Effect", order = 1)]
public class Effect : ScriptableObject
{
    [TextArea]
    public string desc;
    public float duration;

    public bool HasExpired(float elapsed)
    {
        if (duration < 0)
        {
            return false;
        }
        else if (elapsed >= duration)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual bool ApplyEffect(ActorAttributes attributes)
    {
        return true;
    }

    public virtual bool UpdateEffect(ActorAttributes attributes, float deltaTime)
    {
        return true;
    }

    public virtual bool RemoveEffect(ActorAttributes attributes)
    {
        return true;// do nothing
    }
}
