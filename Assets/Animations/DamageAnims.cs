using Animancer;
using System.Collections;
using UnityEngine;

public class DamageAnims : ScriptableObject
{
    public ClipTransition flinch;
    public ClipTransition stagger;
    public ClipTransition stagger2;
    public ClipTransition stumble;
    public ClipTransition knockdown;
    public ClipTransition stun;
    public ClipTransition crumple;
    public ClipTransition blockStagger;
    public ClipTransition guardBreak;
    public ClipTransition recoil;


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