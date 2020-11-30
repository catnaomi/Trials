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
    public InputAttack bothStrong;
    public InputAttack bothQuick;

    public enum AttackStyle
    {
        None,
        MainStrong,
        MainQuick,
        OffStrong,
        OffQuick,
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
            case AttackStyle.BothStrong:
                return bothStrong;
            case AttackStyle.BothQuick:
                return bothQuick;
            default:
                return null;
        }
    }
}
