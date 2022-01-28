using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ActorAttributes : MonoBehaviour
{
    private readonly float SMOOTHING_DELAY = 1.5f;
    private readonly float SMOOTHING_MAX_DELTA = 10f;

    private readonly float EFFECT_UPDATE_FREQUENCY = 1f; // how often to update effects, in seconds.

    private float effectClock;
    // basic/vitality stats


    [Header("Vitality")]
    [ReadOnly] public float smoothedHealth;
    public float healthRecoveryClock;
    private float healthSmoothClock;
    private float healthLast;
    public AttributeValue health;
    public AttributeValue healthRecoveryRate;
    public bool usesHearts = false;
    public bool spareable = false;

    public UnityEvent OnHealthLoss;
    public UnityEvent OnHealthGain;
    public UnityEvent OnHealthChange;
    [Header("Resistances & Weaknesses")]
    public List<DamageResistance> resistances;

    public List<Effect> effects;

    private void Start()
    {
        effects = new List<Effect>();
    }
    // Update is called once per frame
    void Update()
    {
        smoothedHealth = NumberUtilities.TimeDelayedSmoothDelta(smoothedHealth, health.current, healthSmoothClock + SMOOTHING_DELAY, health.max / SMOOTHING_MAX_DELTA, Time.time);

        if (health.current < healthLast)
        {
            healthRecoveryClock = 0f;
        }
        else
        {
            healthRecoveryClock += Time.deltaTime;
        }
        if (smoothedHealth == health.current)
        {
            healthSmoothClock = Time.time;
        }
        else if (smoothedHealth < health.current)
        {
            smoothedHealth = health.current;
        }

        healthLast = health.current;
        effectClock += Time.deltaTime;
        if (effectClock > EFFECT_UPDATE_FREQUENCY)
        {
            foreach (Effect effect in effects)
            {
                if (effect.HasExpired(EFFECT_UPDATE_FREQUENCY))
                {
                    effect.Remove(this);
                }
            }
        }
    }

    public void ResetAttributes()
    {
        effectClock = 0f;

        smoothedHealth = health.current = health.max = health.baseValue;
        healthRecoveryRate.current = healthRecoveryRate.max = healthRecoveryRate.baseValue;

        effects.Clear();

        // TODO: resistances
    }

    public bool HasAttributeRemaining(AttributeValue value)
    {
        return value.current > 0f;
    }
    public void RecoverAttribute(AttributeValue value, float amount)
    {
        RecoverAttributeToMax(value, amount, value.max);
    }

    public void RecoverAttributeToMax(AttributeValue value, float amount, float max)
    {
        if (value.current < max)
        {
            value.current += amount;
            if (value.current > max)
            {
                value.current = max;
            }
        }
    }

    public void ReduceAttribute(AttributeValue value, float amount)
    {
        ReduceAttributeToMin(value, amount, 0f);
    }
    public void ReduceAttributeToMin(AttributeValue value, float amount, float min)
    {
        if (value.current > min)
        {
            value.current -= amount;
            if (value.current < min)
            {
                value.current = min;
            }
        }
    }

    public void SetHealth(float amount)
    {
        float diff = amount - health.current;
        if (diff >= 0)
        {
            RecoverHealth(diff);
        }
        else
        {
            ReduceHealth(diff);
        }
    }
    public void ReduceHealth(float damage)
    {
        if (damage > 0)
        {
            ReduceAttribute(health, damage);
            if (usesHearts)
            {
                health.current = Mathf.Ceil(health.current / 10f) * 10f;
            }
            OnHealthChange.Invoke();
            OnHealthLoss.Invoke();
        }
        
    }

    public void RecoverHealth(float recovery)
    {
        if (recovery > 0)
        {
            RecoverAttributeToMax(health, recovery, health.max);
            if (usesHearts)
            {
                health.current = Mathf.Ceil(health.current / 10f) * 10f;
            }
            OnHealthGain.Invoke();
            OnHealthChange.Invoke();
        }
    }
    public bool HasHealthRemaining()
    {
        return health.current > 0;
    }

    public void AddEffect(Effect effect)
    {
        effect.Apply(this);
    }

    public void RemoveEffect(Effect effect)
    {
        effect.Remove(this);
    }
}


[Serializable]
public class AttributeValue
{
    public float baseValue;
    public float current;
    public float max;
}
