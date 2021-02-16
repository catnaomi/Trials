using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using CustomUtilities;

// TODO: FIX THIS

public class Inventory : MonoBehaviour
{
    public static int invID;

    HumanoidActor actor;
    [ReadOnly] public EquippableWeapon MainWeapon;
    private bool MainIsDrawn;
    //private bool TwoHanding;
    
    [ReadOnly] public EquippableWeapon OffWeapon;
    private bool OffIsDrawn;

    public int startingMainSlot = -1;
    public int startingOffSlot = -1;

    [Header("Inspector-set Weapons")]
    public EquippableWeapon Slot0Weapon; // starts equipped
    public EquippableWeapon Slot1Weapon; // up
    public EquippableWeapon Slot2Weapon; // left
    public EquippableWeapon Slot3Weapon; // right


    public List<Item> StartingContents; // for use in inspector or otherwise
    public List<Item> contents;

    public UnityEvent OnChange;

    public bool weaponChanged;

    public enum EquipSlot
    {
        none,  // 0
        rHip,  // 1
        lHip,  // 2
        rBack, // 3
        lBack, // 4
        cBack  // 5
    }

    public void Init()
    {
        OnChange = new UnityEvent();
        this.actor = GetComponent<HumanoidActor>();

        MainWeapon = null;
        OffWeapon = null;

        contents = new List<Item>();
        foreach (Item item in StartingContents)
        {
            if (item != null)
            {
                Item newItem = ScriptableObject.Instantiate(item);
                AddItem(newItem);
            }
        }
        StartingContents.Clear();

        EquippableWeapon mweapon = null;
        EquippableWeapon oweapon = null;

        if (Slot0Weapon != null)
        {
            mweapon = Instantiate(Slot0Weapon);
            Slot0Weapon = mweapon;
            AddItem(mweapon);
            
        }

        if (Slot1Weapon != null)
        {
            oweapon = Instantiate(Slot1Weapon);
            Slot1Weapon = oweapon;
            AddItem(oweapon);
            EquipOffHandWeapon(oweapon);
        }

        if (Slot2Weapon != null)
        {
            var sweapon = Instantiate(Slot2Weapon);
            Slot2Weapon = sweapon;
            AddItem(sweapon);
        }

        if (Slot3Weapon != null)
        {
            var sweapon = Instantiate(Slot3Weapon);
            Slot3Weapon = sweapon;
            AddItem(sweapon);
        }

        if (mweapon != null && oweapon != null)
        {
            EquipMainWeapon(mweapon, false);
            EquipOffHandWeapon(oweapon, false);
        }
        else if (mweapon != null)
        {
            EquipMainWeapon(mweapon, true);
        }
        else if (oweapon != null)
        {
            EquipOffHandWeapon(oweapon, true);
        }
        


        //GenerateModels();
        PositionWeapon();
        /*
        if (EquippedIsMainEquipped())
        {
            EquippableWeapon weapon = ScriptableObject.Instantiate(EquippedMainWeapon);
            AddItem(weapon);
            EquipMainWeapon((BladeWeapon)weapon);
        }
        if (EquippedOffHand != null)
        {
            EquippableWeapon weapon = ScriptableObject.Instantiate(EquippedOffHand);
            AddItem(weapon);
            EquipOffHandWeapon(weapon);
        }*/
    }

    public void AddItem(Item item)
    {
        if (item != null)
        {
            contents.Add(item);
            item.holder = actor;
            OnChange.Invoke();
        }
    }

    public void RemoveItem(Item item)
    {
        if (item != null)
        {
            bool success = contents.Remove(item);
            item.holder = null;
            OnChange.Invoke();
        }
    }

    public void EquipMainWeapon(EquippableWeapon weapon, bool draw)
    {
        if (!weapon.EquippableMain)
        {
            return;
        }
        if (IsMainEquipped())
        {
            if (weapon == MainWeapon)
            {
                return;
            }
            UnequipMainWeapon();
        }
        if (weapon == OffWeapon)
        {
            UnequipOffHandWeapon();
        }
        MainWeapon = weapon;
        //slot.slot = slot.weapon.MainHandEquipSlot;

        MainIsDrawn = false;
        GenerateMainModel();
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(actor);

        //UpdateTwoHand(weapon.TwoHanded && !weapon.OneHanded);

        if (draw)
        {
            actor.TriggerSheath(true, MainWeapon.MainHandEquipSlot, true);
        }

        OnChange.Invoke();
        weaponChanged = true;
    }
    public void EquipMainWeapon(EquippableWeapon weapon)
    {
        EquipMainWeapon(weapon, true);
    }

    public void EquipOffHandWeapon(EquippableWeapon weapon, bool draw)
    {
        if (!weapon.EquippableOff)
        {
            return;
        }
        if (IsOffEquipped())
        {
            if (weapon == OffWeapon)
            {
                return;
            }
            UnequipOffHandWeapon();
        }
        if (weapon == MainWeapon)
        {
            UnequipMainWeapon();
        }
        OffWeapon = weapon;
        //offHandModel = GameObject.Instantiate(weapon.prefab);
        //slot.isMain = false;

        OffIsDrawn = false;

        GenerateModels();
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(actor);

        //UpdateTwoHand(false);

        if (draw)
        {
            actor.TriggerSheath(true, OffWeapon.OffHandEquipSlot, false);
        }

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void EquipOffHandWeapon(EquippableWeapon weapon)
    {
        EquipOffHandWeapon(weapon, true);
    }

    public void UnequipMainWeapon()
    {
        if (!IsMainEquipped())
        {
            return;
        }
        MainWeapon.UnequipWeapon(actor);
        MainWeapon.isEquipped = false;
        MainWeapon.DestroyModel();
        //MainWeapon.isMain = false;
        //MainWeapon.isOff = false;
        //MainWeapon.isDrawn = false;
        //MainWeapon.is2h = false;
        MainWeapon = null;
        PositionWeapon();

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void UnequipOffHandWeapon()
    {
        if (!IsOffEquipped())
        {
            return;
        }
        OffWeapon.UnequipWeapon(actor);
        OffWeapon.isEquipped = false;
        OffWeapon.DestroyModel();
        //OffWeapon.isMain = false;
        //OffWeapon.isOff = false;
        //OffWeapon.isDrawn = false;
        //OffWeapon.is2h = false;
        OffWeapon = null;
        PositionWeapon();

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void GenerateMainModel()
    {
        if (IsMainEquipped())
        {
            MainWeapon.GenerateModel();
        }
    }

    public void GenerateOffModel()
    {
        if (IsOffEquipped())
        {
            OffWeapon.GenerateModel();
        }
    }
    public void GenerateModels()
    {
        GenerateMainModel();
        GenerateOffModel();
    }

    /*
    public void UpdateTwoHand(bool set2h)
    {
        if (MainWeapon == null)
        {
            return;
        }
        if (set2h)
        {
            // if weapon can be two handed, do so, and unequip off hand weapon
            TwoHanding = MainWeapon.TwoHanded;           
        }
        else
        {
            // if weapon can be one handed, do so
            TwoHanding = !MainWeapon.OneHanded;
        }
        if (TwoHanding)
        {
            UnequipOffHandWeapon();
        }
        OnChange.Invoke();
        weaponChanged = true;
    }
    */
    public void PositionWeapon()
    {
        // main
        if (IsMainEquipped())
        {
            GameObject parent;
            if (IsMainDrawn() && !MainWeapon.ParentLeftAsMain)
            {
                parent = actor.positionReference.MainHand;
            }
            else if (IsMainDrawn() && MainWeapon.ParentLeftAsMain)
            {
                parent = actor.positionReference.OffHand;
            }
            else
            {
                parent = actor.GetPositionRefSlot(MainWeapon.MainHandEquipSlot);
            }
            MainWeapon.model.transform.position = parent.transform.position;
            MainWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            MainWeapon.model.transform.SetParent(parent.transform, true);
        }
        // off
        if (IsOffEquipped())
        {
            GameObject parent;
            if (IsOffDrawn() && OffWeapon.ParentRightAsOff)
            {
                parent = actor.positionReference.MainHand;
            }
            else if (IsOffDrawn() && !OffWeapon.ParentRightAsOff)
            {
                parent = actor.positionReference.OffHand;
            }
            else
            {
                parent = actor.GetPositionRefSlot(OffWeapon.OffHandEquipSlot);
            }
            OffWeapon.model.transform.position = parent.transform.position;
            OffWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            OffWeapon.model.transform.SetParent(parent.transform, true);
        }
    }

    public bool IsMainDrawn()
    {
        return IsMainEquipped() && MainIsDrawn;
    }

    public bool IsOffDrawn()
    {
        return IsOffEquipped() && OffIsDrawn;
    }
    
    public void UpdateWeapon()
    {
        if (IsMainEquipped())
        {
            MainWeapon.UpdateWeapon(actor);
        }
        if (IsOffEquipped())
        {
            OffWeapon.UpdateWeapon(actor);
        }
    }
// during fixed update
    public void FixedUpdateWeapon()
    {
        if (IsMainEquipped())
        {
            MainWeapon.FixedUpdateWeapon(actor);
        }
        if (IsOffEquipped())
        {
            OffWeapon.FixedUpdateWeapon(actor);
        }
    }
    public EquippableWeapon GetMainWeapon()
    {
        if (IsMainEquipped())
        {
            return MainWeapon;
        }
        return null;
    }

    public EquippableWeapon GetOffHand()
    {
        if (IsOffEquipped())
        {
            return OffWeapon;
        }
        return null;
    }

    public float GetAttackSpeed()
    {
        if (IsMainEquipped())
        {
            return MainWeapon.GetAttackSpeed(IsTwoHanding());
        }
        return 1f;
    }

    public float GetOffAttackSpeed()
    {
        if (IsOffEquipped())
        {
            return OffWeapon.GetAttackSpeed(false);
        }
        return 1f;
    }


    public bool IsTwoHanding()
    {
        return IsMainDrawn() && !IsOffDrawn();
        //return TwoHanding;
    }

    public void UpdateStance(StanceHandler stance)
    {
        StanceHandler mainStance = null;
        StanceHandler offStance = null;

        if (IsMainEquipped() && IsMainDrawn())
        {
            //stance = StanceHandler.MergeStances(stance, GetMainWeapon().PrfMainHandStance);
            mainStance = GetMainWeapon().PrfStance;
        }

        if (IsOffEquipped() && IsOffDrawn())
        {
            //stance = StanceHandler.MergeStances(stance, GetOffHand().PrfOffHandStance);
            offStance = GetOffHand().PrfStance;
        }

        stance.MergeStance(mainStance, offStance);
    }

    public float GetEquipWeight()
    {
        float weight = 0f;
        if (IsMainEquipped())
        {
            weight += GetMainWeapon().GetWeight();
        }

        if (IsOffEquipped())
        {
            weight += GetOffHand().GetWeight();
        }

        return weight;
    }

    public float GetPoiseFromAttack(BladeWeapon.AttackType attackType)
    {
        if (IsMainEquipped() && MainWeapon is BladeWeapon mb)
        {
            return mb.GetPoiseFromAttack(attackType);
        }
        return 0f;
    }

    public void SetDrawn(bool main, bool drawn)
    {
        if (main)
        {
            if (IsMainEquipped())
            {
                MainIsDrawn = drawn;
            }
        }
        else
        {
            if (IsOffEquipped())
            {
                OffIsDrawn = drawn;
            }
        }
        PositionWeapon();
        weaponChanged = true;
    }

    public void EquipToSlot(EquippableWeapon weapon, int slot)
    {
        // remove from old slot
        int currentSlot = FindWeaponSlot(weapon);
        if (currentSlot == 0)
        {
            Slot0Weapon = null;
        }
        else if (currentSlot == 1)
        {
            Slot1Weapon = null;
        }
        else if (currentSlot == 2)
        {
            Slot2Weapon = null;
        }
        else if (currentSlot == 3)
        {
            Slot3Weapon = null;
        }

        // add to new slot
        if (slot == 0)
        {
            Slot0Weapon = weapon;
        }
        else if (slot == 1)
        {
            Slot1Weapon = weapon;
        }
        else if (slot == 2)
        {
            Slot2Weapon = weapon;
        }
        else if (slot == 3)
        {
            Slot3Weapon = weapon;
        }
        UnequipMainWeapon();
        UnequipOffHandWeapon();
    }

    public int FindWeaponSlot(EquippableWeapon weapon)
    {
        if (Slot0Weapon == weapon)
        {
            return 0;
        }
        else if (Slot1Weapon == weapon)
        {
            return 1;
        }
        else if (Slot2Weapon == weapon)
        {
            return 2;
        }
        else if (Slot3Weapon == weapon)
        {
            return 3;
        }
        return -1;
    }
    public bool IsWeaponDrawn()
    {
        return IsMainDrawn();
    }

    public bool IsMainEquipped()
    {
        if (MainWeapon == null)
        {
            return false;
        }
        if (MainWeapon.itemName == "")
        {
            return false;
        }
        return true;
    }

    public bool IsOffEquipped()
    {
        if (OffWeapon == null)
        {
            return false;
        }
        if (OffWeapon.itemName == "")
        {
            return false;
        }
        return true;
    }

    public GameObject GetOffhandModel()
    {
        return OffWeapon.model;
    }

    public GameObject GetWeaponModel()
    {
        return MainWeapon.model;
    }
    
    public Damage GetBlockResistance(bool main)
    {
        if (main && IsMainEquipped())
        {
            return GetMainWeapon().GetBlockResistance();
        }
        else if (!main && IsOffEquipped())
        {
            return GetOffHand().GetBlockResistance();
        }
        else
        {
            return new Damage();
        }
    }

    public float GetBlockPoiseDamage(bool main)
    {
        if (main && IsMainEquipped())
        {
            return GetMainWeapon().GetBlockPoiseDamage();
        }
        else if (!main && IsOffEquipped())
        {
            return GetOffHand().GetBlockPoiseDamage();
        }
        else
        {
            return 0f;
        }
    }
    // gets which hand the current Equipped Weapon is Equipped to
    //  0 - not equipped
    //  1 - main hand
    // -1 - off hand
    public int GetItemHand(EquippableWeapon weapon)
    {
        if (weapon == this.MainWeapon)
        {
            return 1;
        }
        else if (weapon == this.OffWeapon)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public bool CheckWeaponChanged()
    {
        if (weaponChanged)
        {
            weaponChanged = false;
            return true;
        }
        return false;
    }
}