using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public AnimancerComponent animancer;
    public GameObject afterimagePrefab;
    public float fadeTime = 3f;
    [ReadOnly]public AnimancerComponent[] afterimages;
    public bool useAfterimages = false;
    public float[] timeRemaining;
    public List<TimeTravelData> timeTravelStates;
    public TimeTravelController timeTravelController;
    int imageIndex;
    bool isRewinding;
    protected Actor actor;
    float lastHealth;
    TimeTravelData lastData;
    bool isFrozen;

    List<Renderer> renderers;
    int initLayer;
    bool applyVisual;
    void Start()
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
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        if (useAfterimages)
        {
            int images = (int)Mathf.Min(timeTravelController.maxSteps, Mathf.Ceil(fadeTime / TimeTravelController.time.rewindStepDuration));
            afterimages = new AnimancerComponent[images];
            timeRemaining = new float[images];
            for (int i = 0; i < images; i++)
            {
                GameObject image = GameObject.Instantiate(afterimagePrefab, timeTravelController.transform);
                image.name = "Afterimage (" + i + ") for " + this.gameObject.name;
                afterimages[i] = image.GetComponent<AnimancerComponent>();
                timeRemaining[i] = 1f;
            }
        }
        timeTravelStates = new List<TimeTravelData>();
        TimeTravelController.time.RegisterAffectee(this);
    }

    void Update()
    {
        if (useAfterimages)
        {
            for (int i = 0; i < afterimages.Length; i++)
            {
                if (afterimages[i].gameObject.activeInHierarchy)
                {
                    if (timeRemaining[i] > 0)
                    {
                        timeRemaining[i] -= Time.deltaTime;
                    }
                    else
                    {
                        if (afterimages[i].gameObject.activeInHierarchy)
                        {
                            afterimages[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        if (ShouldApplyTimeVisualEffect() && !applyVisual)
        {
            applyVisual = true;
            foreach (Renderer r in renderers)
            {
                r.gameObject.layer = LayerMask.NameToLayer("TimeAffected");
            }
        }
        else if (!ShouldApplyTimeVisualEffect() && applyVisual)
        {
            applyVisual = false;
            foreach (Renderer r in renderers)
            {
                r.gameObject.layer = initLayer;
            }
        }
    }

    public virtual TimeTravelData SaveTimeState()
    {
        if (isRewinding || isFrozen) return null;
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

        if (useAfterimages && lastData != null && lastData is ActorTimeTravelData actorLastData && isRewinding)
        {
            AnimancerComponent afterimage = afterimages[imageIndex];
            afterimage.gameObject.SetActive(true);
            afterimage.transform.position = actorLastData.position;
            afterimage.transform.rotation = actorLastData.rotation;
            AnimancerState imageState = afterimage.Play(actorLastData.animationClip);
            imageState.Speed = 0f;
            imageState.NormalizedTime = actorLastData.animancerNormalizedTime;
            timeRemaining[imageIndex] = fadeTime;
            imageIndex++;
            imageIndex %= afterimages.Length;
        }
        
        if (actor is PlayerActor player)
        {
            player.UnsnapLedge();
            player.SkipCloth();
        }

        lastData = data;
    }
    public virtual void StartRewind()
    {
        imageIndex = 0;
        isRewinding = true;
        actor.isInTimeState = true;
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(1f);
        animancer.States.Current.IsPlaying = false;
        lastHealth = actor.attributes.health.current;
        if (actor is PlayerActor player)
        {
            player.DisableCloth();
        }
        foreach (AnimancerComponent afterimage in afterimages)
        {
            afterimage.gameObject.SetActive(false);
        }
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
        isFrozen = true;
        actor.isInTimeState = true;
        if (animancer.States.Current != null)
        {
            animancer.States.Current.IsPlaying = false;
        }
        
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(1f);
    }

    public virtual void StopFreeze()
    {
        isFrozen = false;
        actor.isInTimeState = false;
        if (actor is PlayerActor player)
        {
            player.walkAccelReal = player.walkAccel;
        }
        animancer.Layers[HumanoidAnimLayers.TimeEffects].SetWeight(0f);
        animancer.Layers[HumanoidAnimLayers.TimeEffects].DestroyStates();
        animancer.States.Current.IsPlaying = true;
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

    public GameObject GetAfterImagePrefab()
    {
        return afterimagePrefab;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public bool IsRewinding()
    {
        return isRewinding;
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
}