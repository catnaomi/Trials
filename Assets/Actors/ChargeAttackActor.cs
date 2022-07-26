using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttackActor : CombatantActor
{

    public InputAttack GapCloserAttack;
    public ClipTransition GapCloserAnim;
    public float GapCloserMaxTime = -1;
    public float GapCloserAttackDistance = 1f;
    float GapCloserClock;
    AnimancerState state_GapCloserApproach;
    AnimancerState state_GapCloserAttack;
    public override void ActorStart()
    {
        base.ActorStart();
        OnHitboxActive.AddListener(RealignToTarget);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (CombatTarget == null)
        {
            if (DetermineCombatTarget(out GameObject target))
            {
                CombatTarget = target;
            }
        }
        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (animancer.States.Current == state_GapCloserApproach)
        {
            bool attack = false;
            if (GapCloserMaxTime > 0 && GapCloserClock > 0)
            {
                GapCloserClock -= Time.deltaTime;
                if (GapCloserClock <= 0f)
                {
                    attack = true;
                }
            }
            
            if (Vector3.Distance(CombatTarget.transform.position,this.transform.position) < GapCloserAttackDistance)
            {
                attack = true;
            }
            if (Vector3.Dot((CombatTarget.transform.position-this.transform.position),this.transform.forward) < 0f)
            {
                attack = true;
            }
            if (attack)
            {
                state_GapCloserAttack = GapCloserAttack.ProcessHumanoidAction(this, MoveOnEnd);
            }
        }
    }

    public void StartCharge()
    {
        state_GapCloserApproach = animancer.Play(GapCloserAnim);
        if (GapCloserMaxTime > 0)
        {
            GapCloserClock = GapCloserMaxTime;
        }
    }

    public void Shockwave(int active)
    {
        if (currentDamage == null) return;
        currentDamage.source = this.gameObject;
        float SHOCKWAVE_RADIUS = 2f;

        bool main = (inventory.IsMainDrawn());
        bool off = (inventory.IsOffDrawn());

        Vector3 origin = this.transform.position;
        if (active == 1 && main)
        {
            origin = inventory.GetMainWeapon().GetModel().transform.position;
            if (inventory.GetMainWeapon() is BladeWeapon blade)
            {
                origin += inventory.GetMainWeapon().GetModel().transform.up * blade.length;
            }
        }
        else if (active == 2 && off)
        {
            origin = inventory.GetOffWeapon().GetModel().transform.position;
            if (inventory.GetOffWeapon() is BladeWeapon blade)
            {
                origin += inventory.GetOffWeapon().GetModel().transform.up * blade.length;
            }
        }

        Collider[] colliders = Physics.OverlapSphere(origin, SHOCKWAVE_RADIUS, LayerMask.GetMask("Actors"));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable) && (collider.transform.root != this.transform.root || currentDamage.canDamageSelf))
            {
                damageable.TakeDamage(currentDamage);
            }
        }
        Debug.DrawRay(origin, Vector3.forward * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.back * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.right * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.left * SHOCKWAVE_RADIUS, Color.red, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * GapCloserMaxTime * 4f);
    }
}
