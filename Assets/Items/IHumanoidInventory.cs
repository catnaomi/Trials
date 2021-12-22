using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IHumanoidInventory
{
    public int GetItemHand(EquippableWeapon equippableWeapon);

    public GameObject GetOffhandModel();

    public GameObject GetWeaponModel();

}