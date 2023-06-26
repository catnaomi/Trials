using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantMecanimActor : Actor, IAttacker
{
    [Header("Position Reference")]
    public Transform RightHand;
    public Transform LeftHand;
    [Header("Weapons")]
    public float RightWeaponLength = 1f;
    public float RightWeaponRadius = 1f;
    [Space(15)]
    public float LeftWeaponLength = 1f;
    public float LeftWeaponRadius = 1f;
    [Header("Attacks")]
    public DamageKnockback tempDamage;
    HitboxGroup rightHitboxes;
    HitboxGroup leftHitboxes;
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        GenerateWeapons();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

    }
    void GenerateWeapons()
    {
        if (rightHitboxes != null)
        {
            rightHitboxes.DestroyAll();
        }
        if (leftHitboxes != null)
        {
            leftHitboxes.DestroyAll();
        }
        rightHitboxes = Hitbox.CreateHitboxLine(RightHand.position, RightHand.up, RightWeaponLength, RightWeaponRadius, RightHand, new DamageKnockback(tempDamage), this.gameObject);
        leftHitboxes = Hitbox.CreateHitboxLine(LeftHand.position, LeftHand.up, LeftWeaponLength, LeftWeaponRadius, LeftHand, new DamageKnockback(tempDamage), this.gameObject);
    }
    public DamageKnockback GetLastDamage()
    {
        return tempDamage;
    }

    public void HitboxActive(int active)
    {
        if (active == 1)
        {
            rightHitboxes.SetActive(true);
            leftHitboxes.SetActive(false);
        }
        else if (active == 2)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(true);
        }
        else if (active == 0)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(false);
        }

    }
}
