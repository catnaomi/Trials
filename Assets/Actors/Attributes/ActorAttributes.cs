using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ActorAttributes : MonoBehaviour, IHasHealthAttribute
{
    private readonly float SMOOTHING_DELAY = 1f;
    private readonly float SMOOTHING_MAX_DELTA = 0.5f;

    private readonly float EFFECT_UPDATE_FREQUENCY = 1f; // how often to update effects, in seconds.

    private float effectClock;
    // basic/vitality stats


    [Header("Vitality")]
    [ReadOnly] public float smoothedHealth;
    public float healthRecoveryClock;
    private double healthSmoothClock;
    private float healthLast;
    public AttributeValue health;
    public AttributeValue healthRecoveryRate;
    public bool usesHearts = false;
    public bool spareable = false;
    public bool isInvulnerable = false;
    public bool diesOnCrit = false;
    public int lives;
    public int maxLives;

    public UnityEvent OnHealthLoss;
    public UnityEvent OnHealthGain;
    public UnityEvent OnHealthChange;
    [Header("Resistances & Weaknesses")]
    public DamageResistance resistances;
    [Header("Faction & Friendly Fire")]
    public FriendlyGroup friendlyGroup;
    public List<EffectDuration> effects;
    [Header("Journal Entry")]
    public JournalEntry journalEntry;
    [Header("Flow & World")]
    public float cleanUpTime = 5f;

    private void Start()
    {
        effects = new List<EffectDuration>();
    }
    // Update is called once per frame
    void Update()
    {

        //smoothedHealth = NumberUtilities.TimeDelayedSmoothDelta(smoothedHealth, health.current, healthSmoothClock + SMOOTHING_DELAY, health.max * SMOOTHING_MAX_DELTA, Time.timeAsDouble);
        smoothedHealth = GetSmoothedHealth(ref health, smoothedHealth, ref healthSmoothClock, SMOOTHING_DELAY, SMOOTHING_MAX_DELTA);


        if (health.current < healthLast)
        {
            healthRecoveryClock = 0f;
        }
        else
        {
            healthRecoveryClock += Time.deltaTime;
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

    public static float GetSmoothedHealth(ref AttributeValue health, float currentSmoothed, ref double lastSmoothTime, float delay = 1f, float maxDelta = 0.5f)
    {
        float smoothed = NumberUtilities.TimeDelayedSmoothDelta(currentSmoothed, health.current, lastSmoothTime + delay, health.max * maxDelta, Time.timeAsDouble);
        if (smoothed == health.current)
        {
            lastSmoothTime = Time.timeAsDouble;
        }
        else if (smoothed < health.current)
        {
            smoothed = health.current;
        }

        return smoothed;
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
        float floorDamage = Mathf.Floor(damage);
        if (floorDamage > 0)
        {
            ReduceAttribute(health, floorDamage);
            OnHealthChange.Invoke();
            OnHealthLoss.Invoke();
        }
        
    }

    public void RecoverHealth(float recovery)
    {
        float floorRecovery = Mathf.Floor(recovery);
        if (floorRecovery > 0)
        {
            RecoverAttributeToMax(health, floorRecovery, health.max);
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

    public AttributeValue GetHealth()
    {
        return health;
    }

    public float GetSmoothedHealth()
    {
        return smoothedHealth;
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
