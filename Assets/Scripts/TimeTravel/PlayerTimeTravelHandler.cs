using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeTravelHandler : ActorTimeTravelHandler
{
    [Header("After Images")]
    public bool useAfterimages = false;
    public GameObject afterimagePrefab;
    public float fadeTime = 3f;
    [ReadOnly] public AnimancerComponent[] afterimages;
    List<TimeStateDamagePair> afterImageData;
    [Header("Damage")]
    public float unfreezeDamageDelay = 1f;
    
    bool unfreezeRoutineStarted;
    public override void Initialize()
    {
        base.Initialize();
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
        afterImageData = new List<TimeStateDamagePair>();
        TimeTravelController.time.OnTimeStopDamageEvent += ProcessTimeStopHit;
        TimeTravelController.time.OnTimeStopEnd.AddListener(TimeResumeAfterImages);
    }
    public override bool ShouldApplyTimeVisualEffect()
    {
        return (actor is PlayerActor player && player.IsResurrecting());
    }

    public override void ActorTimeUpdate()
    {
        base.ActorTimeUpdate();
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
    }
    public override TimeTravelData SaveTimeState()
    {
        PlayerTimeTravelData data = (PlayerTimeTravelData)base.SaveTimeState();

        data.inWorld2 = PortalManager.instance != null ? PortalManager.instance.inWorld2 : false;
        return data;
    }

    public override void LoadTimeState(TimeTravelData data, float speed)
    {
        PlayerActor player = actor as PlayerActor;
        CharacterController cc = player.GetComponent<CharacterController>();
        bool ccWasEnabled = cc.enabled;

        player.GetComponent<CharacterController>();

        base.LoadTimeState(data, speed);
        if (useAfterimages && lastData != null && lastData is ActorTimeTravelData actorLastData && isRewinding)
        {
            AnimancerComponent afterimage = GetNextAfterImage(fadeTime);
            AnimancerState imageState = CreateAfterimageFromTimeState(afterimage, actorLastData);
        }


        cc.enabled = ccWasEnabled;

        if (PortalManager.instance != null && ((PlayerTimeTravelData)data).inWorld2 != PortalManager.instance.inWorld2)
        {
            PortalManager.instance.Swap();
        }
    }

    public GameObject GetAfterImagePrefab()
    {
        return afterimagePrefab;
    }

    public AnimancerComponent GetNextAfterImage(float fadeTime)
    {
        AnimancerComponent animancerComponent = afterimages[imageIndex];
        timeRemaining[imageIndex] = fadeTime;
        imageIndex++;
        imageIndex %= afterimages.Length;
        return animancerComponent;
    }

    public AnimancerComponent GetNextAfterImage()
    {
        return GetNextAfterImage(fadeTime);
    }

    public override void StartRewind()
    {
        base.StartRewind();
        if (actor is PlayerActor player)
        {
            player.DisableCloth();
        }
        foreach (AnimancerComponent afterimage in afterimages)
        {
            afterimage.gameObject.SetActive(false);
        }
    }

    public void ProcessTimeStopHit(object sender, DamageKnockback damage)
    {
        if (unfreezeRoutineStarted) return;
        PlayerActor player = actor as PlayerActor;
        ActorTimeTravelData data = (ActorTimeTravelData)SaveTimeState();
        TimeStateDamagePair pair = new TimeStateDamagePair
        {
            damage = damage,
            data = data,
            target = sender as IDamageable,
            targetPosition = (sender as IDamageable).GetGameObject().transform.position
        };
        Vector3 contactPoint = damage.originPoint;
        GameObject targetObject = pair.target.GetGameObject();
        if (targetObject.TryGetComponent<Collider>(out Collider c))
        {
            contactPoint = c.ClosestPoint(damage.originPoint);
        }
        FXController.CreateMiragiaParticleSingleSound(contactPoint);
        afterImageData.Add(pair);
    }

    struct TimeStateDamagePair
    {
        public DamageKnockback damage;
        public ActorTimeTravelData data;
        public IDamageable target;
        public Vector3 targetPosition;
        public bool isLastHitOnTarget;

    }
    public void TimeResumeAfterImages()
    {
        Queue<TimeStateDamagePair> afterImageDamageDataQueue = new Queue<TimeStateDamagePair>(afterImageData.Count);
        for (int i = 0; i < afterImageData.Count; i++)
        {
            TimeStateDamagePair refPair = afterImageData[i];
            IDamageable target = refPair.target;
            refPair.isLastHitOnTarget = true;
            for (int j = i + 1; j < afterImageData.Count; j++)
            {
                if (afterImageData[j].target == target)
                {
                    refPair.isLastHitOnTarget = false;
                    break;
                }
            }
            afterImageDamageDataQueue.Enqueue(refPair);
        }

        StartCoroutine(TimeResumeDamageRoutine(afterImageDamageDataQueue));
        afterImageData.Clear();
    }

    IEnumerator TimeResumeDamageRoutine(Queue<TimeStateDamagePair> afterImageDamageDataQueue)
    {
        float clock = 0f;
        unfreezeRoutineStarted = true;
        yield return null;
        while (afterImageDamageDataQueue.Count > 0)
        {
            TimeStateDamagePair timeStateDamagePair = afterImageDamageDataQueue.Dequeue();

            DamageKnockback damage = timeStateDamagePair.damage;
            damage.breaksArmor = true;
            damage.cannotAutoFlinch = true;
            damage.bouncesOffBlock = false;
            damage.bouncesOffTypedBlock = false;
            damage.cannotRecoil = true;
            damage.cannotKill = !timeStateDamagePair.isLastHitOnTarget;
            damage.timeDelayed = true;
            GameObject targetObject = timeStateDamagePair.target.GetGameObject();
            ActorTimeTravelData data = timeStateDamagePair.data;
            if (afterImageDamageDataQueue.Count > 0)
            {
                damage.critData.doesNotConsumeCritState = true;
                damage.critData.criticalExtensionTime = afterImageDamageDataQueue.Count * unfreezeDamageDelay;
            }
            else
            {
                damage.critData.doesNotConsumeCritState = false;
            }

            float damagedRadius = 2f;

            if (targetObject != null && targetObject.GetComponent<Collider>() != null)
            {
                damagedRadius = targetObject.GetComponent<Collider>().bounds.extents.magnitude;
            }

            Vector3 offset = Vector3.zero;
            if (targetObject != null)
            {
                offset = timeStateDamagePair.data.position - timeStateDamagePair.targetPosition;
            }
            if (offset.magnitude < damagedRadius)
            {
                offset = offset.normalized * damagedRadius;
            }
            Debug.Log("afterimage offset magnitude: " + offset.magnitude);


            if (targetObject != null)
            {
                timeStateDamagePair.target.TakeDamage(damage);
            }
            


            AnimancerComponent afterImage = GetNextAfterImage(fadeTime);
            AnimancerState afterimageState = CreateAfterimageFromTimeState(afterImage, timeStateDamagePair.data);
            afterImage.transform.position = timeStateDamagePair.target.GetGameObject().transform.position + offset;
            afterImage.transform.LookAt(targetObject.transform.position, Vector3.up);
            afterimageState.Speed = (afterimageState.Length / unfreezeDamageDelay);
            afterimageState.NormalizedTime = 0f;
            afterimageState.Events.Clear();
            /*
            while (clock < unfreezeDamageDelay)
            {
                
                afterimageState.NormalizedTime = Mathf.Clamp01(clock / unfreezeDamageDelay);
                clock += Time.deltaTime;
                if (damaged && clock > (unfreezeDamageDelay/2f))
                {
                    timeStateDamagePair.target.TakeDamage(damage);
                }
                yield return null;
            }
            */
            yield return new WaitForSeconds(unfreezeDamageDelay);
            //afterimageState.NormalizedTime = 1f;
        }
        afterImageDamageDataQueue.Clear();
        unfreezeRoutineStarted = false;
    }
}
