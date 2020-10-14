using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[Serializable]
public class Effect
{
    [ReadOnly]
    public string displayName;
    [ReadOnly, TextArea]
    public string desc;
    [ReadOnly]
    public bool isIndefinite;
    [ReadOnly]
    public float duration;
    [ReadOnlyAttribute]
    public bool applied;

    public Effect()
    {
        displayName = "Undefined Effect";
        desc = "";
        applied = false;
        isIndefinite = false;
        duration = -1f;
    }

    public bool HasExpired(float deltaTime)
    {
        if (!isIndefinite)
        {
            return false;
        }
        duration -= deltaTime;
        if (duration < 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Apply(ActorAttributes attributes)
    {
        // do nothing

        if (!attributes.effects.Contains(this))
        {
            attributes.effects.Add(this);
            if (ApplyEffect(attributes))
            {
                applied = true;
            }
        }

    }

    public void Remove(ActorAttributes attributes)
    {
        // do nothing

        bool success = attributes.effects.Remove(this);
        if (success)
        {
            success = RemoveEffect(attributes);
            if (success)
            {
                applied = false;
            }          
        }
    }

    protected virtual bool ApplyEffect(ActorAttributes attributes)
    {
        return true; // do nothing
    }

    protected virtual bool RemoveEffect(ActorAttributes attributes)
    {
        return true;// do nothing
    }
}
