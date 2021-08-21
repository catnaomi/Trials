using UnityEngine;
using System.Collections;

public class CombatantActor : NavigatingHumanoidActor
{
    public float SightRange = 15f;
    public float MaxEngageRange = 10f;
    public float MinEngageRange = 5f;
    public float AttackRange = 1.5f;

    public float clock;
    public static float CLOCK_DEFAULT = 2f;

    public float LowHealthThreshold = 50f;
    public bool isLowHealth;
    public override void ActorStart()
    {
        base.ActorStart();

    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        if (clock > -1)
        {
            clock -= Time.deltaTime;
        }

        this.OnInjure.AddListener(() => 
        {
            StartHelpless();
        });

        if (CombatTarget == null)
        {
            if (DetermineCombatTarget(out GameObject target))
            {
                CombatTarget = target;

                StartNavigationToTarget(target);

                if (target.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
                {
                    actor.OnAttack.AddListener(BeingAttacked);
                }
            }
        }

        float dist = GetDistanceToTarget();
        animator.SetFloat("DistanceToTarget", dist);
        animator.SetBool("LineOfSight", IsClearLineToTarget());
        animator.SetBool("InRange-Sight", dist <= SightRange);
        animator.SetBool("InRange-MaxEngage", dist <= MaxEngageRange);
        animator.SetBool("InRange-MinEngage", dist <= MinEngageRange);
        animator.SetBool("InRange-Attack", dist <= AttackRange);
        animator.SetFloat("Random", Random.value);
        animator.SetFloat("ActionTimer", clock);
        float timeInState = animator.GetFloat("TimeInState");
        animator.SetFloat("TimeInState", timeInState + Time.deltaTime);

        float ASCurve = animator.GetFloat("AttackSpeedCurve");
        if (ASCurve == 0f)
        {
            this.animator.SetFloat("AttackSpeedMain", GetAttackSpeed());
            this.animator.SetFloat("AttackSpeedOff", GetOffAttackSpeed());
        }
        else
        {
            this.animator.SetFloat("AttackSpeedMain", GetAttackSpeed() * ASCurve);
            this.animator.SetFloat("AttackSpeedOff", GetOffAttackSpeed() * ASCurve);
        }

    }

    public float GetAttackSpeed()
    {
        if (inventory.IsMainEquipped())
        {
            return inventory.GetMainWeapon().GetAttackSpeed(false);
        }
        return 1f;
    }

    public float GetOffAttackSpeed()
    {
        if (inventory.IsOffEquipped())
        {
            return inventory.GetOffWeapon().GetAttackSpeed(false);
        }
        return 1f;
    }

    public bool DetermineCombatTarget(out GameObject target)
    {
        target = PlayerActor.player.gameObject;
        return true;
    }

    public void BeingAttacked()
    {
        if (CombatTarget != null && currentDistance < MinEngageRange)
        {
            animator.SetTrigger("GettingAttacked");
        }
    }
}
