using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Moveset
{
    [Header("Light Attacks")]
    public InputAttack slashMain;
    public InputAttack slashOff;
    [Space(5)]
    public InputAttack thrustMain;
    public InputAttack thrustOff;
    [Header("Heavy Attacks")]
    public InputAttack slashMainHeavy;
    public InputAttack slashOffHeavy;
    [Space(5)]
    public InputAttack thrustMainHeavy;
    public InputAttack thrustOffHeavy;
    [Header("Movement-Based Attacks")]
    public InputAttack slashDash;
    public InputAttack slashPlunge;
    public InputAttack slashSneak;
    [Space(5)]
    public InputAttack thrustDash;
    public InputAttack thrustPlunge;
    public InputAttack thrustSneak;
    [Header("Skill Attacks")]
    public InputAttack skill1;
    public InputAttack skill2;

    public enum AttackStyle
    {
        Slash,
        Thrust,
        Skill1,
        Skill2
    }
}
