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

    public int lives;
    public int maxLives;

    public UnityEvent OnHealthLoss;
    public UnityEvent OnHealthGain;
    public UnityEvent OnHealthChange;
    [Header("Resistances & Weaknesses")]
    public DamageResistance resistances;

    public List<EffectDuration> effects;

    private void Start()
    {
        effects = new List<EffectDuration>();
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
        if (effectClock >= EFFECT_UPDATE_FREQUENCY)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                EffectDuration effectDuration = effects[i];
                effectDuration.elapsed += effectClock;
                Effect effect = effectDuration.effect;
                effect.UpdateEffect(this, effectClock);
                if (effect.HasExpired(effectDuration.elapsed))
                {
                    RemoveEffect(effect);
                }
            }
            effectClock = 0f;
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
            ReduceHealth(-diff);
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
        if (effect.ApplyEffect(this))
        {
            effects.Add(new EffectDuration(effect, 0f));
        }
            
    }

    public void RemoveEffect(Effect effect)
    {
        if (effect.RemoveEffect(this))
        {
            int removeIndex = -1;
            for (int i = 0; i < effects.Count; i++)
            {
                EffectDuration effectDuration = effects[i];
                if (effectDuration.effect == effect)
                {
                    removeIndex = i;
                    break;
                }
            }
            if (removeIndex >= 0)
            {
                effects.RemoveAt(removeIndex);
            }
        }
    }

    [Serializable]
    public struct EffectDuration
    {
        public Effect effect;
        public float elapsed;

        public EffectDuration(Effect effect, float elapsed)
        {
            this.effect = effect;
            this.elapsed = elapsed;
        }
    }
}


[Serializable]
public class AttributeValue
{
    public float baseValue;
    public float current;
    public float max;

    public AttributeValue()
    {

    }
    public AttributeValue(float baseValue, float current, float max)
    {
        this.baseValue = baseValue;
        this.current = current;
        this.max = max;
    }
}
