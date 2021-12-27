﻿using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAnimations", menuName = "ScriptableObjects/Animancer/Create Damage Animations Asset", order = 1)]
public class DamageAnims : ScriptableObject
{
    public ClipTransition flinch;
    public MixerTransition2D staggerSmall;
    public MixerTransition2D staggerLarge;
    public ClipTransition stumble;
    public ClipTransition knockdown;
    public ClipTransition stun;
    public ClipTransition crumple;
    public ClipTransition blockStagger;
    public ClipTransition guardBreak;
    public ClipTransition recoil;


    public ClipTransition GetClipFromStaggerType(DamageKnockback.StaggerType type)
    {
        switch (type)
        {
            case DamageKnockback.StaggerType.Flinch:
                return flinch;
            case DamageKnockback.StaggerType.Stumble:
                return stumble;
            case DamageKnockback.StaggerType.Knockdown:
                return knockdown;
            case DamageKnockback.StaggerType.Stun:
                return stun;
            case DamageKnockback.StaggerType.Crumple:
                return crumple;
            case DamageKnockback.StaggerType.BlockStagger:
                return blockStagger;
            case DamageKnockback.StaggerType.GuardBreak:
                return guardBreak;
            case DamageKnockback.StaggerType.Recoil:
                return recoil;
        }
        return null;
    }
    /*
     *  None,           // 0
        // hitsuns
        Flinch,         // 1
        Stagger,   // 2
        Stumble,   // 3
        // knockouts
        Knockdown,      // 4
        Stun,           // 5
        Crumple,        // 6

        // on block
        BlockStagger,   // 7
        GuardBreak,     // 8
        Recoil,          // 9 shorter stun

        FallDamage,       // 10
    */


}