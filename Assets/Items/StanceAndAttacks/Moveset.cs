using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Moveset
{
    public InputAttack mainStrong;
    public InputAttack mainQuick;
    [Space(5)]
    public InputAttack offStrong;
    public InputAttack offQuick;
    [Space(5)]
    public InputAttack twoStrong;
    public InputAttack twoQuick;
    [Space(5)]
    public InputAttack dualStrong;
    public InputAttack dualQuick;

    public enum AttackStyle
    {
        None,
        MainStrong,
        MainQuick,
        OffStrong,
        OffQuick,
        TwoStrong,
        TwoQuick,
        BothStrong,
        BothQuick
    }

    public InputAttack GetAttackFromInput(AttackStyle style)
    {
        switch (style)
        {
            case AttackStyle.MainStrong:
                return mainStrong;
            case AttackStyle.MainQuick:
                return mainQuick;
            case AttackStyle.OffStrong:
                return offStrong;
            case AttackStyle.OffQuick:
                return offQuick;
            case AttackStyle.TwoStrong:
                return twoStrong;
            case AttackStyle.TwoQuick:
                return twoQuick;
            case AttackStyle.BothStrong:
                return dualStrong;
            case AttackStyle.BothQuick:
                return dualQuick;
            default:
                return null;
        }
    }

    public InputAttack SetAttackFromInput(AttackStyle style, InputAttack atk)
    {
        switch (style)
        {
            case AttackStyle.MainStrong:
                mainStrong = atk;
                return mainStrong;
            case AttackStyle.MainQuick:
                mainQuick = atk;
                return mainQuick;
            case AttackStyle.OffStrong:
                offStrong = atk;
                return offStrong;
            case AttackStyle.OffQuick:
                offQuick = atk;
                return offQuick;
            case AttackStyle.TwoStrong:
                twoStrong = atk;
                return twoStrong;
            case AttackStyle.TwoQuick:
                twoQuick = atk;
                return twoQuick;
            case AttackStyle.BothStrong:
                dualStrong = atk;
                return dualStrong;
            case AttackStyle.BothQuick:
                dualQuick = atk;
                return dualQuick;
            default:
                return null;
        }
    }
}
