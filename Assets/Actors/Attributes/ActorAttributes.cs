using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using System.Collections.Generic;

public class ActorAttributes : MonoBehaviour
{
    private readonly float SMOOTHING_DELAY = 1.5f;
    private readonly float SMOOTHING_MAX_DELTA = 25f;

    private readonly float EFFECT_UPDATE_FREQUENCY = 1f; // how often to update effects, in seconds.

    private float effectClock;
    // basic/vitality stats

    
    [Header("Vitality")]

    [ReadOnly] public float smoothedHealth;
    private float healthRecoveryClock;
    private float healthSmoothClock;
    private float healthLast;
    public AttributeValue health;
    public AttributeValue healthRecoveryRate;

    [ReadOnly] public float smoothedStamina;
    private float staminaRecoveryClock;
    private float staminaSmoothClock;
    private float staminaLast;
    public AttributeValue stamina;
    public AttributeValue staminaRecoveryRate;

    [Header("AI Personality")]
    public AttributeValue audacity;
    public AttributeValue guile;
    /*
    [ReadOnly] public float smoothedPoise;
    
    private float poiseSmoothClock;
    private float poiseLast;
    public AttributeValue poiseRecoveryRate;
    */
    private float poiseRecoveryClock;
    private float poiseLast;
    private bool poiseIncreased;
    public float poise;
    public float weightPoise = 10f; // poise of character weight during attacks and movement
    public float poiseRate = 100f;
    public bool isOffBalance;

    [Space(5)]
    public float attributeRecoveryDelay = 3f;

    public Damage resistances;

    [Header("Statistics")]
    public float BlockReduction = 1f;

    public List<Effect> effects;

    private void Start()
    {
        effects = new List<Effect>();
    }
    // Update is called once per frame
    void Update()
    {
        smoothedHealth = NumberUtilities.TimeDelayedSmoothDelta(smoothedHealth, health.current, healthSmoothClock + SMOOTHING_DELAY, health.max / SMOOTHING_MAX_DELTA, Time.time);
        smoothedStamina = NumberUtilities.TimeDelayedSmoothDelta(smoothedStamina, stamina.current, staminaSmoothClock + SMOOTHING_DELAY, stamina.max / SMOOTHING_MAX_DELTA, Time.time);
        //smoothedPoise = NumberUtilities.TimeDelayedSmoothDelta(smoothedPoise, poise.current, poiseSmoothClock + SMOOTHING_DELAY, poise.max / SMOOTHING_MAX_DELTA, Time.time);

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
        if (healthRecoveryClock > attributeRecoveryDelay)
        {
            this.RecoverAttribute(health, healthRecoveryRate.current * Time.deltaTime);
        }

        if (stamina.current < staminaLast)
        {
            staminaRecoveryClock = 0f;
        }
        else
        {
            staminaRecoveryClock += Time.deltaTime;
        }
        if (smoothedStamina == stamina.current)
        {
            staminaSmoothClock = Time.time;
        }
        else if (smoothedStamina < stamina.current)
        {
            smoothedStamina = stamina.current;
        }
        if (staminaRecoveryClock > attributeRecoveryDelay)
        {
            this.RecoverAttribute(stamina, staminaRecoveryRate.current * Time.deltaTime);
        }

        CheckOffBalance();


        if (poiseIncreased)
        {
            poiseRecoveryClock = 0f;
            poiseIncreased = false;
        }
        else if (poise > poiseLast || health.current < healthLast)
        {
            poiseRecoveryClock = 0f;
        }
        else
        {
            poiseRecoveryClock += Time.deltaTime;
        }
        if (poiseRecoveryClock > attributeRecoveryDelay)
        {
            //this.ReduceAttribute(poise, poiseLossRate * Time.deltaTime);

            //float targetPoise = (this.TryGetComponent<HumanoidActor>(out HumanoidActor human) && (!human.CanMove() || human.moveDirection == Vector3.zero)) ? 0f : GetBasePoise();
            float targetPoise = 100f;

            this.poise = Mathf.MoveTowards(this.poise, targetPoise, poiseRate * Time.deltaTime);
        }

        healthLast = health.current;
        staminaLast = stamina.current;
        poiseLast = poise;

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

    public void Reset()
    {
        effectClock = 0f;

        smoothedHealth = health.current = health.max = health.baseValue;
        healthRecoveryRate.current = healthRecoveryRate.max = healthRecoveryRate.baseValue;
        smoothedStamina = stamina.current = stamina.max = stamina.baseValue;
        staminaRecoveryRate.current = staminaRecoveryRate.max = staminaRecoveryRate.baseValue;

        poise = 100f;

        effects.Clear();

        // TODO: resistances
    }

    public bool HasAttributeRemaining(AttributeValue value)
    {
        return value.current > 0f;
    }

    public bool HasPoiseRemaining()
    {
        return poise > 0f;
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

    public void IncreasePoiseByWeight(float poise)
    {
        float totalWeightPoise = poise + weightPoise + this.GetComponent<Inventory>().GetEquipWeight();

        if (this.poise <= totalWeightPoise + 100f)
        {
            this.poise += totalWeightPoise;
            if (this.poise >= totalWeightPoise + 100f)
            {
                this.poise = totalWeightPoise + 100f;
            }
        }
        poiseIncreased = true;

        CheckOffBalance();
    }

    public void SetPoise(float poise)
    {
        this.poise = poise;
        poiseIncreased = true;

        CheckOffBalance();
    }

    public void ReducePoise(float poise)
    {
        this.poise -= poise;
        poiseIncreased = true;

        CheckOffBalance();
    }

    public bool HasHealthRemaining()
    {
        return health.current > 0;
    }

    public void CheckOffBalance()
    {
        if (isOffBalance && poise >= 100f)
        {
            isOffBalance = false;
        }

        if (!isOffBalance && poise <= 0)
        {
            isOffBalance = true;
        }
    }

    public bool GetOffBalance()
    {
        return isOffBalance;
    }

    public void AddEffect(Effect effect)
    {
        effect.Apply(this);
    }

    public void RemoveEffect(Effect effect)
    {
        effect.Remove(this);
    }

    public float GetModifiedPoiseDamage(float amount, DamageType type)
    {
        float mult = 1f;// Mathf.Clamp((0.2f) * -resistances.resist[(int)type], -0.8f, 0.8f) + 1f;

        return amount * mult;
    }

    public int GetModifiedHealthDamage(int amount, DamageType type)
    {
        int mod = 0;

        return Mathf.Max(0, amount + mod);
    }
}


[Serializable]
public class AttributeValue
{
    public float baseValue;
    [ReadOnly] public float current;
    [ReadOnly] public float max;
}
