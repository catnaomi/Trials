using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class TimeTravelController : MonoBehaviour
{
    public static TimeTravelController time;
    [Header("Rewind Settings")]
    public UnityEvent RecordStateEvent;
    public UnityEvent OnRewindStart;
    public UnityEvent OnRewindFrame;
    public UnityEvent OnRewindStop;
    int stepsToRewind = 10;
    public float rewindStepDuration = 0.5f;
    public float recordInterval = 4f;
    public int maxSteps = 30;
    public int stepsRecorded;
    public List<IAffectedByTimeTravel> affectees;
    
    bool isRewinding;
    bool recording;
    bool cancelRewind;
    PlayerInput playerInput;
    [Header("Time Stop Settings")]
    public UnityEvent OnTimeStopStart;
    public UnityEvent OnTimeStopEnd;
    public EventHandler<DamageKnockback> OnTimeStopDamageEvent;
    public bool globalFreeze;
    public GameObject timeStopObject;
    public Vector3 timeStopOrigin;
    public float timeStopRadius;
    public float timeToOpenBubble = 1f;
    public AnimationCurve bubbleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool freeze;
    public List<IAffectedByTimeTravel> frozens;
    public UnityEvent OnTimeStopHit;
    bool updateFreeze;
    [Header("Slow Time Settings")]
    public float timeSlowAmount = 0.5f;
    bool isSlowing;
    public UnityEvent OnSlowTimeStart;
    public UnityEvent OnSlowTimeStop;
    [Header("Meter Settings")]
    public AttributeValue charges = new AttributeValue(7, 7, 7);
    public AttributeValue meter = new AttributeValue(60f, 60f, 60f);
    public float timeChargeRecoveryRate = 1f;
    public float timeChargeQuickRecoveryRate = 3f;
    [SerializeField, ReadOnly] public bool isQuickRecharging;
    public float timeStopDrainRate;
    public float rewindDrainRate;
    public float timePowerCooldown = 5f;
    [SerializeField, ReadOnly] public float timePowerClock = 0f;
    public float timeStopDamageCostRatio = 1f;
    public float timeAimSlowDrainRate = 5f;
    public float timeStopMovementCostRatio = 1f;
    public float timeStopHitboxActivationCost = 10f;

    Vector3 lastPosition;
    public UnityEvent OnCooldownFail;
    public UnityEvent OnCooldownComplete;
    public UnityEvent OnMeterFail;
    public UnityEvent OnChargeSpent;
    public UnityEvent OnChargeRecovered;
    public UnityEvent OnQuickChargeStart;
    public UnityEvent OnQuickChargeEnd;
    [Header("Shader Settings")]
    public Material magicVignette;
    public float magicVignetteStrength;
    public Renderer bubbleInner;
    MaterialPropertyBlock block;
    bool ignoreLimits;
    bool infiniteResources;
    private void Awake()
    {
        time = this;
        affectees = new List<IAffectedByTimeTravel>();
        frozens = new List<IAffectedByTimeTravel>();
    }
    // Use this for initialization
    void Start()
    {
        playerInput = GameObject.FindObjectOfType<PlayerInput>();
        StartCoroutine("RecordRoutine");
        SetupInput();
        stepsRecorded = 0;
        StartRecord();
        if (bubbleInner != null)
        {
            block = new MaterialPropertyBlock();
            bubbleInner.SetPropertyBlock(block);
        }
        if (SceneLoader.IsSceneLoaderActive())
        {
            SceneLoader.GetOnActiveSceneChange().AddListener(ClearTimeDatas);
        }
        if (PlayerActor.player != null)
        {
            lastPosition = PlayerActor.player.transform.position;
            PlayerActor.player.OnHitboxActive.AddListener(TimeStopHitboxActivation);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Vector3 screenSpacePoint = Camera.main.WorldToScreenPoint(timeStopOrigin);
        float dist = screenSpacePoint.z;
        float screenSpaceRadius = (timeStopRadius / (Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2f) * dist)) * (Screen.height / 2f);
        //screenSpaceRadius *= Mathf.Max(Screen.width, Screen.height) / (Camera.main.fieldOfView * Mathf.Deg2Rad);//((Vector2)screenSpacePoint - (Vector2)Camera.main.WorldToScreenPoint(timeStopOrigin + Vector3.forward * timeStopRadius)).magnitude;
        circleScreenPosition = new Vector4(screenSpacePoint.x, screenSpacePoint.y, 0, 0);
        circleScreenRadius = screenSpaceRadius;
        fullscreenMaterial.SetVector("_CircleScreenPosition", circleScreenPosition);
        fullscreenMaterial.SetFloat("_CircleScreenRadius", screenSpaceRadius);
        */
        Vector3 movementDelta = Vector3.zero;
        if (PlayerActor.player != null)
        {
            movementDelta = lastPosition - PlayerActor.player.transform.position;
            movementDelta.y = 0f;
        }
            

        if (isRewinding)
        {
            if (!ignoreLimits)
            {
                meter.current -= rewindDrainRate * Time.deltaTime;
            }
            
            timePowerClock = timePowerCooldown;
            if (meter.current <= 0f)
            {
                meter.current = 0f;
                if (charges.current > 0)
                {
                    ConsumeChargeAndResetMeter();
                }
                else
                {
                    meter.current = 0f;
                    CancelRewind();
                    OnMeterFail.Invoke();
                }

            }
        }
        else if (freeze)
        {
            if (!ignoreLimits)
            {
                meter.current -= timeStopDrainRate * Time.deltaTime;

                meter.current -= movementDelta.magnitude * timeStopMovementCostRatio * Time.deltaTime;
            }
            

            timePowerClock = timePowerCooldown;

            
            if (meter.current <= 0f)
            {
                if (!ignoreLimits)
                {
                    if (charges.current > 0)
                    {
                        ConsumeChargeAndResetMeter();
                    }
                    else
                    {
                        meter.current = 0f;
                        StopFreeze();
                        OnMeterFail.Invoke();
                    }  
                }
            }
        }
        else if (isSlowing)
        {
            if (!ignoreLimits)
            {
                meter.current -= timeAimSlowDrainRate * Time.deltaTime;
            }
            
            if (meter.current < 0f)
            {
                if (charges.current > 0)
                {
                    ConsumeChargeAndResetMeter();
                }
                else
                {
                    meter.current = 0f;
                    StopSlowTime();
                    OnMeterFail.Invoke();
                }
            }
        }
        else // not using powers
        {
            
            /*
            if (meter.current < 0f)
            {
                meter.current = 0f;
            }

            if (meter.current < meter.max)
            {
                meter.current += time.timePowerRecoveryRate * Time.deltaTime;
            }
            if (meter.current > meter.max)
            {
                meter.current = meter.max;
            }
            */
            //meter.current = meter.max;
            if (charges.current < charges.max)
            {
                if (timePowerClock > 0f)
                {
                    timePowerClock -= Time.deltaTime * (isQuickRecharging ? timeChargeQuickRecoveryRate : timeChargeRecoveryRate);
                    if (timePowerClock <= 0f)
                    {
                        RecoverCharge();
                        OnCooldownComplete.Invoke();
                        if (charges.current < charges.max)
                        {
                            timePowerClock = timePowerCooldown;
                        }
                    }
                }
            }
            else
            {
                timePowerClock = timePowerCooldown;
            }
            
        }
        bool slow = PlayerActor.player.ShouldSlowTime();
        if (slow && !isSlowing)
        {
            if (meter.current >= 10f)
            {
                StartSlowTime();
            }
        }
        else if (!slow && isSlowing)
        {
            StopSlowTime();
        }

        if (infiniteResources)
        {
            meter.current = meter.max;
            charges.current = charges.max;
        }

        bool isAnyPowerOn = freeze || isSlowing || isRewinding;
        magicVignetteStrength = Mathf.MoveTowards(magicVignetteStrength, isAnyPowerOn ? 1f : 0f, 5f * Time.deltaTime);
        magicVignette.SetFloat("_Weight", magicVignetteStrength);

        lastPosition = PlayerActor.player.transform.position;
    }

    void SetupInput()
    {
        playerInput.actions["UsePower"].performed += UsePowerInput;

        playerInput.actions["UsePower"].canceled += CancelPowerInput;
    }

    void UsePowerInput(UnityEngine.InputSystem.InputAction.CallbackContext c)
    {
        if (!ShouldAllowInput()) return;
        if (ignoreLimits) return;
        if (isSlowing)
        {
            // do nothing
        }
        else if (!isRewinding && !freeze && c.interaction is TapInteraction)
        {
            if (!CanStartPower())
            {
                OnCooldownFail.Invoke();
                return;
            }

            /*
            GameObject target = PlayerActor.player.GetCombatTarget();
            if (target == null)
            {
                timeStopOrigin = PlayerActor.player.transform.position;
            }
            else
            {
                timeStopOrigin = PlayerActor.player.GetCombatTarget().transform.position;
            }
            if (!globalFreeze)
            {
                StartCoroutine(OpenBubbleRoutine());
                StartFreeze();
            }*/
            AddAllToFreeze();
            StartFreeze();
        }
        else if (!freeze && recording && !isRewinding && c.interaction is HoldInteraction)
        {
            if (!CanStartPower())
            {
                OnCooldownFail.Invoke();
                return;
            }
            StartRewind();
        }
        else if (freeze && c.interaction is TapInteraction)
        {
            StopFreeze();
        }
    }

    public void ConsumeChargeAndResetMeter()
    {
        charges.current--;
        meter.current = meter.max;
        OnChargeSpent.Invoke();
    }

    public void RecoverCharge()
    {
        if (charges.current < charges.max)
        {
            charges.current++;
        }
        OnChargeRecovered.Invoke();
    }

    public void ToggleQuickRecharge(bool isOn)
    {
        isQuickRecharging = isOn;
    }
    public bool IsAnyPowerActive()
    {
        return IsFreezing() || IsRewinding() || IsSlowingTime();
    }
    void CancelPowerInput(UnityEngine.InputSystem.InputAction.CallbackContext c)
    {
        if (!ShouldAllowInput()) return;
        if (ignoreLimits) return;
        if (isSlowing)
        {
            // do nothing
        }
        else if (isRewinding)
        {
            CancelRewind();
        }
        else if (freeze)
        {
            //StopFreeze();
        }
    }

    private void OnDestroy()
    {
        if (playerInput == null) return;
        playerInput.actions["UsePower"].performed -= UsePowerInput;

        playerInput.actions["UsePower"].canceled -= CancelPowerInput;
    }
    public void RegisterAffectee(IAffectedByTimeTravel affectee)
    {
        affectees.Add(affectee);
        if (freeze && globalFreeze)
        {
            AddFrozen(affectee);
        }
    }

    public void DeregisterAffectee(IAffectedByTimeTravel affectee)
    {
        affectees.Remove(affectee);
    }

    public void ClearTimeDatas()
    {
        if (isRewinding)
        {
            CancelRewind();
        }
        foreach (IAffectedByTimeTravel affectee in affectees)
        {
            affectee.ClearTimeData();
        }
    }

    public void StartRecord()
    {
        recording = true;
        stepsRecorded = 0;
        StartCoroutine(RecordRoutine());
    }

    public void StartRewind()
    {
        recording = false;
        cancelRewind = false;
        stepsToRewind = Mathf.Min(stepsRecorded, maxSteps);
        StartCoroutine(RewindRoutine(affectees));
        StartPostProcessing();
    }
    public void StartRewindSelective(params IAffectedByTimeTravel[] affectees)
    {
        recording = false;
        cancelRewind = false;
        stepsToRewind = Mathf.Min(stepsRecorded, maxSteps);
        List<IAffectedByTimeTravel> selectiveAffectees = new List<IAffectedByTimeTravel>();
        selectiveAffectees.AddRange(affectees);
        StartCoroutine(RewindRoutine(selectiveAffectees));
        StartPostProcessing();
    }

    public void StopRewind()
    {
        ignoreLimits = false;
        StopPostProcessing();
    }

    void StartPostProcessing()
    {
        PostProcessingController.SetVolumeWeight(PostProcessingController.instance.MagicVolume, 1f, timeToOpenBubble);
    }

    void StopPostProcessing()
    {
        PostProcessingController.SetVolumeWeight(PostProcessingController.instance.MagicVolume, 0f, timeToOpenBubble);
    }
    public void CancelRewind()
    {
        cancelRewind = true;
    }

    public void CancelRecord()
    {
        recording = false;
        foreach (IAffectedByTimeTravel affected in affectees)
        {
            affected.GetTimeStates()?.Clear();
        }
    }

    public void Freeze(IAffectedByTimeTravel affected)
    {

        TimeTravelData data = affected.SaveTimeState();
        affected.LoadTimeState(data);
        affected.StartFreeze();
    }

    public void SetFreezeToVisible()
    {
        frozens.Clear();
        foreach (IAffectedByTimeTravel affected in affectees)
        {
            Renderer renderer = affected.GetObject().GetComponent<Renderer>();
            if (renderer == null)
            {
                affected.GetObject().GetComponentInChildren<Renderer>();
            }
            if (renderer != null)
            {
                if (renderer.isVisible)
                {
                    frozens.Add(affected);
                }
            }
        }
        Debug.Log("added " + frozens.Count + " affected with visible renderers");
        updateFreeze = true;
    }

    public void SetFreezeToTarget()
    {
        frozens.Clear();
        if (PlayerActor.player != null)
        {
            GameObject target = PlayerActor.player.GetCombatTarget();
            if (target != null && target.transform.root.TryGetComponent<IAffectedByTimeTravel>(out IAffectedByTimeTravel affected))
            {
                frozens.Add(affected);
            }
        }
        updateFreeze = true;
    }
    public void Unfreeze(IAffectedByTimeTravel affected)
    {
        if (affected == null) return;
        affected.StopFreeze();
    }

    public void AddFrozen(IAffectedByTimeTravel affected)
    {
        frozens.Add(affected);
        updateFreeze = true;
    }

    public void RemoveFrozen(IAffectedByTimeTravel affected)
    {
        frozens.Remove(affected);
        updateFreeze = false;
    }

    public void AddAllToFreeze()
    {
        foreach (IAffectedByTimeTravel affected in affectees)
        {
            //if (affected)
            if (affected is not PlayerTimeTravelHandler)
            {
                frozens.Add(affected);
            }
        }
        updateFreeze = true;
    }

    public void StartFreeze()
    {
        freeze = true;
        meter.current = meter.max;
        charges.current--;
        //StartCoroutine(OpenBubbleRoutine());
        StartCoroutine(FreezeRoutine());
        StartPostProcessing();
        OnTimeStopStart.Invoke();
    }

    public void StopFreeze()
    {
        //timeStopObject.SetActive(false);
        freeze = false;
        ignoreLimits = false;
        StopPostProcessing();
        OnTimeStopEnd.Invoke();
    }

    IEnumerator FreezeRoutine()
    {
        while (freeze)
        {
            foreach (IAffectedByTimeTravel affected in affectees)
            {
                if (affected == null || affected.IsNull())
                {
                    DeregisterAffectee(affected);
                    continue;
                }
                if (!affected.IsFrozen() && frozens.Contains(affected))
                {
                    Freeze(affected);
                }
                else if (affected.IsFrozen() && !frozens.Contains(affected))
                {
                    Unfreeze(affected);
                }
            }
            updateFreeze = false;
            yield return new WaitUntil(() => { return !freeze || updateFreeze; });
        }
        foreach (IAffectedByTimeTravel affected in frozens)
        {
            if (affected.IsFrozen())
            {
                Unfreeze(affected);
            }
        }
        //frozens.Clear();
    }

    IEnumerator OpenBubbleRoutine()
    {
        timeStopObject.transform.position = timeStopOrigin;
        timeStopObject.transform.localScale = Vector3.zero;
        block.SetVector("_Center", timeStopOrigin);
        block.SetFloat("_Radius", timeStopRadius);
        bubbleInner.SetPropertyBlock(block);
        timeStopObject.SetActive(true);
        float t = 0f;
        float currentTime = 0f;
        while (t < 1f)
        {
            yield return null;
            currentTime += Time.deltaTime;
            t = Mathf.Clamp01(currentTime / timeToOpenBubble);
            float currentRadius = bubbleCurve.Evaluate(t) * timeStopRadius;
            timeStopObject.transform.localScale = Vector3.one * currentRadius;
        }
        yield return new WaitWhile(() => { return freeze; });
        t = 0;
        currentTime = 0f;
        while (t < 1f)
        {
            yield return null;
            currentTime += Time.deltaTime;
            t = Mathf.Clamp01(currentTime / timeToOpenBubble);
            float currentRadius = bubbleCurve.Evaluate(1f - t) * timeStopRadius;
            timeStopObject.transform.localScale = Vector3.one * currentRadius;
        }
        timeStopObject.SetActive(false);
    }
    IEnumerator RecordRoutine()
    {
        while (recording)
        {
            yield return null;
            foreach (IAffectedByTimeTravel affected in affectees)
            {
                if (affected != null && !affected.IsFrozen())
                {
                    affected.SaveTimeState();
                }
            }
            stepsRecorded++;

            yield return new WaitForSeconds(recordInterval);

        }
        
    }

    IEnumerator RewindRoutine(List<IAffectedByTimeTravel> affectees)
    {
        isRewinding = true;
        int stepsRemaining = stepsToRewind;
        int index = 0;
        while (stepsRemaining >= 0)
        {
            yield return null;
            if (cancelRewind)
            {
                stepsRemaining = 0;
            }
            foreach (IAffectedByTimeTravel affected in affectees)
            {
                if (affected.IsFrozen())
                {
                    continue;
                }
                if (stepsRemaining == stepsToRewind)
                {
                    affected.StartRewind();
                }
                if (stepsRemaining > 0 && affected.GetTimeStates()?.Count > index)
                {
                    TimeTravelData data = affected.GetTimeStates()[affected.GetTimeStates().Count - 1 - index];
                    affected.LoadTimeState(data);
                }
                else if (stepsRemaining == 0)
                {
                    affected.StopRewind();
                }
            }
            stepsRemaining--;
            index++;
            yield return new WaitForSeconds(rewindStepDuration);
        }
        stepsRecorded = 0;
        isRewinding = false;
        StopRewind();
        yield return null;
        StartRecord();
    }

    public void TimeStopDamage(DamageKnockback damage, IDamageable target, float totalDamage)
    {
        meter.current -= totalDamage * timeStopDamageCostRatio;
        OnTimeStopDamageEvent.Invoke(target, damage);
        OnTimeStopHit.Invoke();
    }

    public void TimeStopHitboxActivation()
    {
        if (!freeze) return;
        meter.current -= timeStopHitboxActivationCost;
        OnTimeStopHit.Invoke();
    }

    public bool IsSlowingTime()
    {
        return isSlowing;
    }
    public void StartSlowTime()
    {
        isSlowing = true;
        //meter.current -= timeAimSlowDrainRate + timePowerRecoveryRate;
        StartCoroutine("SlowTimeRoutine");
        StartPostProcessing();
        OnSlowTimeStart.Invoke();
    }

    public void StopSlowTime()
    {
        isSlowing = false;
        ignoreLimits = false;
        StopPostProcessing();
        OnSlowTimeStop.Invoke();
    }

    public void IgnoreLimits()
    {
        ignoreLimits = true;
    }

    public void ToggleInfiniteResources()
    {
        infiniteResources = !infiniteResources;
    }
    IEnumerator SlowTimeRoutine()
    {
        float timeScale = Time.timeScale;
        float timeFixed = Time.fixedDeltaTime;
        Time.timeScale = timeScale * timeSlowAmount;
        Time.fixedDeltaTime = timeFixed * timeSlowAmount;
        yield return new WaitWhile(() => { return isSlowing; });
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = timeFixed;
    }
    public bool CanStartPower()
    {
        return charges.current > 0;
        //return meter.current >= 0f && timePowerClock <= 0f;
    }

    public bool IsRewinding()
    {
        return isRewinding;
    }
    
    public bool IsFreezing()
    {
        return freeze;
    }

    public bool ShouldAllowInput()
    {
        return Time.timeScale > 0f;
    }
    public static float GetTimeAffectedDeltaTime()
    {
        if (time != null && time.IsFreezing())
        {
            return 0f;
        }
        else
        {
            return Time.deltaTime;
        }
    }

    public static float GetTimeAffectedFixedDeltaTime()
    {
        if (time != null && time.IsFreezing())
        {
            return 0f;
        }
        else
        {
            return Time.fixedDeltaTime;
        }
    }

    private void OnApplicationQuit()
    {
        magicVignette.SetFloat("_Weight", 0f);
    }


}