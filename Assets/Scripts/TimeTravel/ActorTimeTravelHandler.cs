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
    public float[] timeRemaining;
    public List<TimeTravelData> timeTravelStates;
    public TimeTravelController timeTravelController;
    int imageIndex;
    bool isRewinding;
    Actor actor;
    float lastHealth;
    TimeTravelData lastData;
    bool isFrozen;
    void Start()
    {
        actor = this.GetComponent<Actor>();
        animancer = this.GetComponent<AnimancerComponent>();
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        afterimages = new AnimancerComponent[timeTravelController.maxSteps];
        timeRemaining = new float[timeTravelController.maxSteps];
        for (int i = 0; i < timeTravelController.maxSteps; i++)
        {
            GameObject image = GameObject.Instantiate(afterimagePrefab, timeTravelController.transform);
            image.name = "Afterimage (" + i + ") for " + this.gameObject.name;
            afterimages[i] = image.GetComponent<AnimancerComponent>();
            timeRemaining[i] = 1f;
        }
        timeTravelStates = new List<TimeTravelData>();
        TimeTravelController.time.RegisterAffectee(this);
    }

    void Update()
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

    public virtual TimeTravelData SaveTimeState()
    {
        if (isRewinding || isFrozen) return null;
        ActorTimeTravelData data = new ActorTimeTravelData()
        {
            time = Time.time,
            position = this.transform.position,
            rotation = this.transform.rotation,
            heading = this.transform.forward,
            velocity = actor.xzVel + Vector3.up * actor.yVel,
            health = actor.attributes.health.current
        };
        if (animancer != null && animancer.States.Current != null)
        {
            data.animancerState = animancer.States.Current;
            data.animancerSpeed = data.animancerState.EffectiveSpeed;
            data.animancerNormalizedTime = animancer.States.Current.NormalizedTime;
            if (data.animancerState is MixerState mixerState)
            {
                data.isMixer = true;
            }
            data.animationClip = CustomUtilities.AnimancerUtilities.GetCurrentClip(animancer);
            data.animancerEndEvent = data.animancerState.Events.OnEnd;
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

    public void LoadTimeState(TimeTravelData data, float speed)
    {


        actor.MoveOverTime(data.position, data.rotation, isRewinding ? TimeTravelController.time.rewindStepDuration : 0f);
        if (data is ActorTimeTravelData actorData)
        {
            if (actorData.animationClip != null)
            {
                AnimancerState mainState = animancer.Layers[(int)HumanoidPositionReference.AnimLayer.TimeEffects].Play(actorData.animationClip, TimeTravelController.time.rewindStepDuration);
                mainState.NormalizedTime = actorData.animancerNormalizedTime;
                mainState.Speed = actorData.animancerSpeed * speed;
                if (speed > 0f)
                {
                    mainState.Events.OnEnd = actorData.animancerEndEvent;
                    mainState.Events.OnEnd += EndTimeState;
                }
            }
            actor.xzVel = new Vector3(actorData.velocity.x, 0f, actorData.velocity.z);
            actor.yVel = actorData.velocity.y;
        }

        if (lastData != null && lastData is ActorTimeTravelData actorLastData && isRewinding)
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
        }

        lastData = data;
    }
    public virtual void StartRewind()
    {
        imageIndex = 0;
        isRewinding = true;
        actor.isInTimeState = true;
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.TimeEffects].SetWeight(1f);
        animancer.States.Current.Speed = 0f;
        lastHealth = actor.attributes.health.current;
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
        lastData = null;
        if (actor is PlayerActor player)
        {
            player.walkAccelReal = player.walkAccel;
        }
        EndTimeState();
    }

    public virtual void StartFreeze()
    {
        isFrozen = true;
        actor.isInTimeState = true;
        animancer.States.Current.Speed = 0f;
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.TimeEffects].SetWeight(1f);
;
    }

    public virtual void StopFreeze()
    {
        isFrozen = false;
        actor.isInTimeState = false;
        if (actor is PlayerActor player)
        {
            player.walkAccelReal = player.walkAccel;
        }
        LoadTimeState(lastData, 1f);
    }

    void EndTimeState()
    {
        actor.SetToIdle();
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.TimeEffects].SetWeight(0f);
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.TimeEffects].DestroyStates();
        animancer.States.Current.Speed = 1f;
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

    public GameObject GetObject()
    {
        return this.gameObject;
    }
}