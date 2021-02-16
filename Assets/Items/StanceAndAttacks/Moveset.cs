using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Moveset
{
    [Header("Light Attacks")]
    public InputAttack slash1H;
    public InputAttack slash2H;
    [Space(5)]
    public InputAttack thrust1H;
    public InputAttack thrust2H;
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
