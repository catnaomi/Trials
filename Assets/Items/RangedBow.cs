using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Bow", menuName = "ScriptableObjects/Weapons/Create Bow", order = 1), SerializeField]
public class RangedBow : EquippableWeapon, HitboxHandler
{
    public GameObject arrowPrefab;
    public DamageKnockback damageKnockback;
    public IKHandler ikHandler;
    float fireStrength;

    public float drawTime = 1f;
    bool canFire;

    /*
    public bool HandleInput(out InputAction action)
    {
        action = null;
        bool down = Input.GetButtonDown("Attack2");
        bool held = Input.GetButton("Attack2");
        bool up = Input.GetButtonUp("Attack2");

        if (down)
        {
            action = ActionsLibrary.GetInputAction("Bow", true);
            return true;
        }

        GetHumanoidHolder().animator.SetBool("AimFire", !held);

        if (!held && ((HumanoidActor)holder).IsAiming() && canFire)
        {
            fireStrength = GetHumanoidHolder().animator.GetFloat("NormalTime");
            //Fire();
            
            return true;
        }



        return false;
    }
    */

    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);
        canFire = true;
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);
    }

    public void Fire()
    {
        if (GetHeldActor() is PlayerMovementController player)
        {

        }
        /*
        HumanoidActor actor = (HumanoidActor)holder;

        //actor.animator.SetTrigger("AimFire");

        Vector3 launchVector = actor.GetLaunchVector(actor.positionReference.OffHand.transform.position) + Vector3.up * 0.05f;

        if (actor.GetCombatTarget() != null)
        {
            // assist at dist 20 = 0.05
            // assist at dist 1 = 0

            float dist = Vector3.Distance(actor.GetCombatTarget().transform.position, actor.transform.position);


            Vector3 aimAssist = Vector3.zero;// Vector3.Lerp(Vector3.zero, new Vector3(0, 0.05f, 0), dist / 20f);

            Debug.Log("aim assist: " + aimAssist.y*100f);

            launchVector = (actor.GetCombatTarget().transform.position - actor.positionReference.OffHand.transform.position).normalized + aimAssist;
        }


        float launchStrength = 25f + (75f * fireStrength);
        ArrowController arrow = ArrowController.Launch(arrowPrefab, actor.transform.position + launchVector + actor.transform.up * 1f, Quaternion.LookRotation(launchVector), launchVector * launchStrength, actor.transform, this.damageKnockback);

        Collider[] arrowColliders = arrow.GetComponentsInChildren<Collider>();
        foreach (Collider actorCollider in actor.transform.GetComponentsInChildren<Collider>())
        {
            foreach (Collider arrowCollider in arrowColliders)
            {
                Physics.IgnoreCollision(actorCollider, arrowCollider);
            }
        }
        //actor.DeductPoiseFromAttack();
        */
    }

    public bool CanOffhandEquip()
    {
        return true;
    }

    public void HitboxActive(bool active)
    {
        if (active && canFire/* && GetHeldActor().IsAiming()*/)
        {
            Debug.Log("bow fire!!!");
            if (GetHeldActor() is PlayerActor)
            {
                fireStrength = Mathf.Clamp(GetHeldActor().animator.GetFloat("Input-HeavyHeldTime"), 0f, 1f);
            }
            else
            {
                fireStrength = 1f;
            }
            GetHeldActor().attributes.ReduceAttribute(GetHeldActor().attributes.stamina, 10f);
            Fire();
            canFire = false;
        }

        if (!active)
        {
            canFire = true;
        }
    }
}
