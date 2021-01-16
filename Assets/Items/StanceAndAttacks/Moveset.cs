using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Moveset
{
    [Header("Light Attacks")]
    public InputAttack light1H;
    public InputAttack light2H;
    [Header("Movement-Based Attacks")]
    public InputAttack dash;
    public InputAttack plunge;
    public InputAttack sneak;
    [Header("Skill Attacks")]
    public InputAttack skill1;
    public InputAttack skill2;

    public enum AttackStyle
    {
        Light,
        Skill1,
        Skill2
    }
}
