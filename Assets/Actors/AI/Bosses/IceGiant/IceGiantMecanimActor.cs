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
    [Space(15)]
    public bool regenerateWeapons;
    HitboxGroup rightHitboxes;
    HitboxGroup leftHitboxes;
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        if (regenerateWeapons)
        {
            GenerateWeapons();
            regenerateWeapons = false;
        }
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
        rightHitboxes = Hitbox.CreateHitboxLine(RightHand.position, RightHand.up, RightWeaponLength, RightWeaponRadius, RightHand, new DamageKnockback(), this.gameObject);
        leftHitboxes = Hitbox.CreateHitboxLine(LeftHand.position, LeftHand.up, LeftWeaponLength, LeftWeaponRadius, LeftHand, new DamageKnockback(), this.gameObject);
    }
    public DamageKnockback GetLastDamage()
    {
        throw new System.NotImplementedException();
    }
}
