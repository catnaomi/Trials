using UnityEngine;
using System.Collections;

public class CombatantActor : NavigatingHumanoidActor
{
    public float EngagementRange = 3f;

    public float clock;

    public float UpdateTime = 1f;

    public float Aggression = 0f;
    public float AggressionIncrease = 10f;
    public float AggressionDecrease = 20f;

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

        if (IsInCombat())
        {
            Aggression += AggressionIncrease * UpdateTime;

            if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
            {
                TriggerSheath(true, inventory.MainWeapon.MainHandEquipSlot, true);
            }
            else if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
            {
                TriggerSheath(true, inventory.OffWeapon.OffHandEquipSlot, false);
            }
            else if (ShouldAttack())
            {
                Attack();
            }
            else if (ShouldDefend())
            {
                if (!ranged)
                {
                    animator.SetBool("Blocking", true);
                }
                SetAdditionalMovement(new Vector3(0.25f, 0, 0.1f), true);
            }
            else
            {
                SetAdditionalMovement(Vector3.zero, false);
            }
        }
        else
        {
            if (ShouldEnterCombat())
            {
                animator.SetBool("AI-InCombat", true);
                // starting combat
                if (DetermineCombatTarget(out GameObject target))
                {
                    CombatTarget = target;

                    NavigateToTarget(target, EngagementRange);
                }
            }
        }

        Aggression = Mathf.Clamp(Aggression, 0f, 100f);
        animator.SetFloat("AI-Aggression", Aggression);
        animator.SetBool("AI-InRange", InRangeOfTarget());
        animator.SetBool("AI-ClearLine", IsClearLineToTarget());
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
