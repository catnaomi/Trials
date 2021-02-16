using UnityEngine;
using System.Collections;

public class CombatantActor : NavigatingHumanoidActor
{
    public float EngagementRange = 0.1f;

    public float clock;

    public float UpdateTime = 1f;

    public float Aggression = 0f;
    float AggressionIncrease;
    float AggressionDecrease;

    public bool inCombat;
    public bool ranged;

    public override void ActorStart()
    {
        base.ActorStart();

    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        clock += Time.deltaTime;

        if (clock >= UpdateTime)
        {
            UpdateCombatant();
            clock = 0f;
        }

        this.OnAttack.AddListener(() =>
        {
            Aggression -= AggressionDecrease;
        });

        this.OnBlock.AddListener(() =>
        {
            Aggression += AggressionIncrease;
        });

        this.OnHurt.AddListener(() =>
        {
            Aggression += AggressionDecrease;
        });

        this.OnInjure.AddListener(() => 
        {
            StartHelpless();
        });

        if (CombatTarget != null && CombatTarget.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            //Physics.IgnoreCollision(this.GetComponent<Collider>(), CombatTarget.GetComponent<Collider>(), this.IsJumping() && !this.cc.isGrounded);
        }
        
    }

    void UpdateCombatant()
    {

        AggressionIncrease = 10f + attributes.audacity.current;
        AggressionDecrease = 50f;
        if (IsInCombat())
        {
            Aggression += AggressionIncrease * UpdateTime;
        }
        else
        {
            if (ShouldEnterCombat())
            {
                inCombat = true;
                // starting combat
                if (DetermineCombatTarget(out GameObject target))
                {
                    CombatTarget = target;

                    NavigateToTarget(target, EngagementRange);
                }
            }
        }

        Aggression = Mathf.Clamp(Aggression, 0f, 100f);
        animator.SetFloat("Aggression", Aggression);
        animator.SetFloat("DistanceToTarget", GetDistanceToTarget());
        animator.SetBool("InRange", InRangeOfTarget());
        animator.SetBool("LineOfSight", IsClearLineToTarget());
        animator.SetBool("InCombat", IsInCombat());
    }

    public bool ShouldEnterCombat()
    {
        return true;
    }
    public bool DetermineCombatTarget(out GameObject target)
    {
        target = PlayerActor.player.gameObject;
        return true;
    }

    // anim details

    public bool IsInCombat()
    {
        return inCombat;
        string TAG = "COMBAT";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(StanceHandler.AILayer).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(StanceHandler.AILayer));
    }

    public bool ShouldAttack()
    {
        string TAG = "ATTACKING";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(StanceHandler.AILayer).IsName(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(StanceHandler.AILayer)) && CanMove();
    }

    public bool ShouldDefend()
    {
        string TAG = "DEFENDING";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(StanceHandler.AILayer).IsName(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(StanceHandler.AILayer));
    }

    public virtual void Attack()
    {
        if (!ranged)
        {
            int attack;
            float rand = Random.Range(0f, 1f);

            if (currentDistance > 4f)
            {
                attack = 2;
            }
            else if (rand < .50)
            {
                attack = 1;
            }
            else
            {
                attack = 0;
            }
            animator.SetInteger("AI-Attack-ID", attack);            
        }
        else
        {
            animator.SetInteger("AI-Attack-ID", -1);
        }
        animator.SetTrigger("AI-Attack");
    }
}
