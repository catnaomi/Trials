using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAnimations", menuName = "ScriptableObjects/Animancer/Create Damage Animations Asset", order = 1)]
public class DamageAnims : ScriptableObject
{
    public ClipTransition flinch;
    public MixerTransition2D staggerSmall;
    public MixerTransition2D staggerLarge;
    public ClipTransition stumble;
    public ClipTransition crumple;
    public ClipTransition fallOver;
    public ClipTransition spinDeath;
    public ClipTransition stun;
    public ClipTransition blockStagger;
    public ClipTransition guardBreak;
    public ClipTransition recoil;

    public AvatarMask flinchMask;
    
    [Space(20)]
    public ClipTransition knockdownFaceUp;
    public ClipTransition fallFaceUp;
    public ClipTransition proneFaceUp;
    public ClipTransition getupFaceUp;
    public ClipTransition deadFaceUp;
    [Space(5)]
    public ClipTransition knockdownFaceDown;
    public ClipTransition fallFaceDown;
    public ClipTransition proneFaceDown;
    public ClipTransition getupFaceDown;
    public ClipTransition deadFaceDown;



    public ClipTransition GetClipFromStaggerType(DamageKnockback.StaggerType type)
    {
        switch (type)
        {
            case DamageKnockback.StaggerType.Flinch:
                return flinch;
            case DamageKnockback.StaggerType.Stumble:
                return stumble;
            case DamageKnockback.StaggerType.Knockdown:
                return knockdownFaceUp;
            case DamageKnockback.StaggerType.SpinDeath:
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