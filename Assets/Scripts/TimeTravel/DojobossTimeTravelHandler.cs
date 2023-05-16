using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojobossTimeTravelHandler : ActorTimeTravelHandler
{
    Animator animator;
    float oldSpeed;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        TimeTravelController.time.RegisterAffectee(this);
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
        oldSpeed = animator.speed;
        animator.speed = 0f;
        isFrozen = true;
        OnFreeze.Invoke();
    }

    public override void StopFreeze()
    {
        if (oldSpeed > 0)
        {
            animator.speed = oldSpeed;
        }
        else
        {
            animator.speed = 1f;
        }
        isFrozen = false;
        OnUnfreeze.Invoke();
    }
}
