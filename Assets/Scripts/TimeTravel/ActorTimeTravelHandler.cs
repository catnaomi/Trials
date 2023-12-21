using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActorTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public AnimancerComponent animancer;
    public float[] timeRemaining;
    public List<TimeTravelData> timeTravelStates;
    public TimeTravelController timeTravelController;
    protected int imageIndex;
    protected bool isRewinding;
    protected Actor actor;
    protected float lastHealth;
    protected TimeTravelData lastData;
    protected bool isFrozen;

    List<Renderer> renderers;
    int initLayer;
    bool applyVisual;
    bool registered;

    public float deltaTime;
    public UnityEvent OnFreeze;
    public UnityEvent OnUnfreeze;
    void Start()
    {
        Initialize();   
    }

    public virtual void Initialize()
    {
        actor = this.GetComponent<Actor>();
        animancer = this.GetComponent<AnimancerComponent>();
        initLayer = this.gameObject.layer;
        renderers = new List<Renderer>();
        if (TryGetComponent<Renderer>(out Renderer r))
        {
            renderers.Add(r);
        }
        renderers.AddRange(this.GetComponentsInChildren<Renderer>());
        timeTravelController = TimeTravelController.time;

        timeTravelStates = new List<TimeTravelData>();
        TimeTravelController.AttemptToRegisterAffectee(this);
    }

    void Update()
    {
        ActorTimeUpdate();
    }

    public virtual void ActorTimeUpdate()
    {
        if (!IsRegistered()) return;
        if (ShouldApplyTimeVisualEffect() && !applyVisual)
        {
            applyVisual = true;
            foreach (Renderer r in renderers)
            {
                // TODO: different way of doing this
                //r.gameObject.layer = LayerMask.NameToLayer("TimeAffected");
            }
        }
        else if (!ShouldApplyTimeVisualEffect() && applyVisual)
        {
            applyVisual = false;
            foreach (Renderer r in renderers)
            {
                //r.gameObject.layer = initLayer;
            }
        }
    }

    public virtual TimeTravelData SaveTimeState()
    {
        if (isRewinding || isFrozen || actor == null || !IsRegistered()) return null;
        ActorTimeTravelData data;
        if (actor is PlayerActor)
        {
            data = new PlayerTimeTravelData();
        }
        else
        {
            data = new ActorTimeTravelData();
        }
        data.time = Time.time;
        data.position = this.transform.position;
        data.rotation = this.transform.rotation;
        data.heading = this.transform.forward;
        data.velocity = actor.xzVel + Vector3.up * actor.yVel;
        data.health = actor.attributes.health.current;
        
        if (animancer != null && animancer.States.Current != null)
        {
            data.animancerState = CustomUtilities.AnimancerUtilities.GetHighestWeightStateRecursive(animancer.States.Current);//animancer.States.Current;
            data.animancerSpeed = data.animancerState.EffectiveSpeed;
            try
            {
                data.animancerNormalizedTime = data.animancerState.NormalizedTime;
                data.animancerEndEvent = data.animancerState.Events.OnEnd;
            }
            catch (System.NullReferenceException ex)
            {
                data.animancerNormalizedTime = 0.5f;
                data.animancerEndEvent = () => { };
                Debug.LogWarning(ex);
            }
            if (data.animancerState is MixerState mixerState)
            {
                data.isMixer = true;
            }
            data.animationClip = data.animancerState.Clip;//CustomUtilities.AnimancerUtilities.GetCurrentClip(animancer);
            
        }

        if (actor is PlayerActor player)
        {
            ((PlayerTimeTravelData)data).carryable = player.carryable;
            ((PlayerTimeTravelData)data).isCarrying = player.isCarrying;
        }
        timeTravelStates.Add(data);
        if (timeTravelStates.Count > TimeTravelController.time.maxSteps)
        {
            timeTravelStates.RemoveRange(0, timeTravelStates.Count - TimeTravelController.time.maxSteps);
        }
        return data;
    }

    public void LoadTimeState(TimeTravelData data)
    {
        LoadTimeState(data, 0f);
    }

    public virtual void LoadTimeState(TimeTravelData data, float speed)
    {
        if (!this.IsRegistered()) return;
        if (!this.gameObject.activeInHierarchy) return;
        actor.MoveOverTime(data.position, data.rotation, isRewinding ? TimeTravelController.time.rewindStepDuration : 0f);
        if (data is ActorTimeTravelData actorData)
        {
            if (actorData.animationClip != null)
            {
                AnimancerState mainState = animancer.Layers[HumanoidAnimLayers.TimeEffects].GetOrCreateState("loaded_time_state-"+data.time.ToString("F3"),actorData.animationClip);//animancer.Layers[HumanoidAnimLayers.TimeEffects].Play(actorData.animationClip, TimeTravelController.time.rewindStepDuration);
                mainState.NormalizedTime = actorData.animancerNormalizedTime;
                mainState.Speed = actorData.animancerSpeed * speed;
                if (speed > 0f)
                {
                    mainState.Events.OnEnd = actorData.animancerEndEvent;
                    mainState.Events.OnEnd += EndTimeState;
                }
                animancer.Layers[HumanoidAnimLayers.TimeEffects].Play(mainState);
            }
            actor.xzVel = new Vector3(actorData.velocity.x, 0f, actorData.velocity.z);
            actor.yVel = actorData.velocity.y;
        }

        if (actor is PlayerActor player)
        {
            player.UnsnapLedge();
            player.SkipCloth();
        }

        lastData = data;
    }

    public AnimancerState CreateAfterimageFromTimeState(AnimancerComponent afterimage, ActorTimeTravelData timeTravelData)
    {
        afterimage.gameObject.SetActive(true);
        afterimage.transform.position = timeTravelData.position;
        afterimage.transform.rotation = timeTravelData.rotation;
        AnimancerState imageState = afterimage.Play(timeTravelData.animationClip);
        imageState.Speed = 0f;
        imageState.NormalizedTime = timeTravelData.animancerNormalizedTime;
        imageState.Events.Clear();
        return imageState;
    }
    public virtual void StartRewind()
    {
        imageIndex = 0;
        isRewinding = true;
        actor.isInTimeState = true;
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(1f);
        animancer.States.Current.IsPlaying = false;
        lastHealth = actor.attributes.health.current;
    }

    public virtual void StopRewind()
    {
        isRewinding = false;
        actor.isInTimeState = false;
        if (lastData != null && lastData is ActorTimeTravelData actorData)
        {
            actor.attributes.SetHealth(actorData.health);
        }
        if (actor is PlayerActor player)
        {
            player.walkAccelReal = player.walkAccel;
            if (lastData != null && (((PlayerTimeTravelData)lastData).isCarrying || player.isCarrying))
            {
                if (player.carryable != null)
                {
                    player.Carry(player.carryable);
                }
                else if (((PlayerTimeTravelData)lastData).carryable != null)
                {
                    player.Carry(((PlayerTimeTravelData)lastData).carryable);
                }
            }
            player.EnableCloth();
        }
        lastData = null;
        EndTimeState();
    }

    public virtual void StartFreeze()
    {
        if (!gameObject.activeInHierarchy) return;
        isFrozen = true;
        actor.isInTimeState = true;
        if (animancer.States.Current != null)
        {
            animancer.States.Current.IsPlaying = false;
        }
        
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(1f);
        OnFreeze.Invoke();
    }

    public virtual void StopFreeze()
    {
        if (this == null || this.gameObject == null || !gameObject.activeInHierarchy) return;
        isFrozen = false;
        actor.isInTimeState = false;
        if (actor is PlayerActor player)
        {
            player.walkAccelReal = player.walkAccel;
        }
        
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(0f);
        animancer.Layers[HumanoidAnimLayers.TimeEffects].DestroyStates();
        if (animancer.States.Current != null)
        {
            animancer.States.Current.IsPlaying = true;
        }
        
        OnUnfreeze.Invoke();
    }

    public void EndTimeState()
    {
        actor.SetToIdle();
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(0f);
        animancer.Layers[HumanoidAnimLayers.TimeEffects].DestroyStates();
        animancer.States.Current.IsPlaying = true;
    }
    public List<TimeTravelData> GetTimeStates()
    {
        return timeTravelStates;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public bool IsRewinding()
    {
        return isRewinding;
    }

    public bool IsNull()
    {
        return this == null;
    }
    public GameObject GetObject()
    {
        return this.gameObject;
    }

    void OnDestroy()
    {
        TimeTravelController.time.DeregisterAffectee(this);
    }

    public virtual bool ShouldApplyTimeVisualEffect()
    {
        return false;// IsFrozen() || (actor is PlayerActor player && player.IsResurrecting());
    }

    public void ClearTimeData()
    {
        
        if (IsFrozen())
        {
            StopFreeze();
        }
        else if (IsRewinding())
        {
            StopRewind();
        }
        timeTravelStates.Clear();
    }

    public void SetRegistered()
    {
        registered = true;
        timeTravelController = TimeTravelController.time;
    }

    public bool IsRegistered()
    {
        return registered;
    }
}