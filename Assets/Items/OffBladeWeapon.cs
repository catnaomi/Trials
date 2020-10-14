﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "OffBladeWeapon", menuName = "ScriptableObjects/Create Offhand BladeWeapon", order = 1)]
public class OffBladeWeapon : BladeWeapon, OffHandWeapon
{

    public string OffhandAttack;

    public bool HandleInput(out InputAction action)
    {
        throw new System.NotImplementedException();
    }

    public new GameObject GetHand()
    {
        return GetHumanoidHolder().positionReference.OffHand;
    }

    public new GameObject GetModel()
    {
        return GetHumanoidHolder().inventory.GetOffhandModel();
    }

}
