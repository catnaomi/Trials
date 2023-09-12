using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MecanimActorTimeTravelHandler : ActorTimeTravelHandler
{
    public bool runBaseFreeze = false;
    Animator animator;
    float oldSpeed;
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<Actor>();
        animator = this.GetComponent<Animator>();
        animancer = this.GetComponent<AnimancerComponent>();
        timeTravelController = TimeTravelController.time;
        TimeTravelController.AttemptToRegisterAffectee(this);
    }

    public override TimeTravelData SaveTimeState()
    {
        return null;
    }

    public override void LoadTimeState(TimeTravelData data, float speed)
    {
        return;
    }

    public override void StartRewind()
    {
        return;
    }

    public override void StopRewind()
    {
        return;
    }

    public override void StartFreeze()
    {
        if (runBaseFreeze) base.StartFreeze();
        actor.isInTimeState = true;
        oldSpeed = animator.speed;
        animator.speed = 0f;
        isFrozen = true;
        OnFreeze.Invoke();
    }

    public override void StopFreeze()
    {
        if (runBaseFreeze) base.StopFreeze();
        if (oldSpeed > 0)
        {
            animator.speed = oldSpeed;
        }
        else
        {
            animator.speed = 1f;
        }
        isFrozen = false;
        actor.isInTimeState = false;
        OnUnfreeze.Invoke();
    }
}
