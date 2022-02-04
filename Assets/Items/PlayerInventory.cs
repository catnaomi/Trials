using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using CustomUtilities;

// TODO: FIX THIS

public class PlayerInventory : Inventory, IInventory, IHumanoidInventory
{
    public static int invID;

    PlayerActor player;
    [ReadOnly] public EquippableWeapon MainWeapon;
    private bool MainIsDrawn;
    //private bool TwoHanding;
    
    [ReadOnly] public EquippableWeapon OffWeapon;
    private bool OffIsDrawn;

    [ReadOnly] public EquippableWeapon RangedWeapon;
    private bool RangedIsDrawn;

    EquippableWeapon BlockingWeapon;

    public bool equipOnStart = true;
    public bool initialized = false;
    [Header("Inspector-set Weapons")]
    [Tooltip("Up Slot. Equips to Mainhand.")]
    public EquippableWeapon Slot0Weapon; // starts equipped. up
    [Tooltip("Left Slot. Equips to Offhand.")]
    public EquippableWeapon Slot1Weapon; // left
    [Tooltip("Right Slot. Equips to Ranged.")]
    public EquippableWeapon Slot2Weapon; // right
    [ReadOnly] public EquippableWeapon Slot3Weapon; // down (disabled)

    public bool weaponChanged;

    public bool TwoHanding;


    void Awake()
    {
        this.player = GetComponent<PlayerActor>();
        contents = new List<Item>();
        foreach (Item item in StartingContents)
        {
            if (item != null)
            {
                Item newItem = ScriptableObject.Instantiate(item);
                AddItem(newItem);
            }
        }
        OnChange.AddListener(() => { lastChanged = Time.time; });
        StartingContents.Clear();
    }
    void Start()
    {
        
        MainWeapon = null;
        OffWeapon = null;

        EquippableWeapon mweapon = null;
        EquippableWeapon oweapon = null;
        EquippableWeapon rweapon = null;

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
            //if (equipOnStart) EquipOffHandWeapon(oweapon);
        }

        if (Slot2Weapon != null)
        {
            rweapon = Instantiate(Slot2Weapon);
            Slot2Weapon = rweapon;
            AddItem(rweapon);
        }

        if (Slot3Weapon != null)
        {
            var sweapon = Instantiate(Slot3Weapon);
            Slot3Weapon = sweapon;
            AddItem(sweapon);
        }

        
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

    void Update()
    {
        if (!initialized)
        {
            if (equipOnStart)
            {
                EquippableWeapon mweapon = Slot0Weapon;
                EquippableWeapon oweapon = Slot1Weapon;
                EquippableWeapon rweapon = Slot2Weapon;
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
                if (rweapon != null)
                {
                    EquipRangedWeapon(rweapon);
                }
            }

            PositionWeapon();

            initialized = true;
        }
        UpdateWeapon();
    }

    void FixedUpdate()
    {
        FixedUpdateWeapon();
    }
    public void AddItem(Item item)
    {
        if (item != null)
        {
            contents.Add(item);
            item.holder = player;
            OnChange.Invoke();
        }
    }

    public bool RemoveItem(Item item)
    {
        if (item != null)
        {
            bool success = contents.Remove(item);
            item.holder = null;
            OnChange.Invoke();
            return success;
        }
        return false;
    }

    public void EquipMainWeapon(EquippableWeapon weapon, bool draw)
    {
        if (!weapon.EquippableMain || !weapon.IsEquippable())
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
        weapon.EquipWeapon(this.player);

        UpdateTwoHand(weapon.TwoHandOnly());

        //player.moveset = weapon.moveset;

        if (draw)
        {
            this.player.TriggerSheath(true, MainWeapon.MainHandEquipSlot, true);
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

        if (!weapon.EquippableOff || !weapon.IsEquippable())
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
        weapon.EquipWeapon(player);

        UpdateTwoHand(false);

        if (draw)
        {
            player.TriggerSheath(true, OffWeapon.OffHandEquipSlot, false);
        }

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void EquipOffHandWeapon(EquippableWeapon weapon)
    {
        EquipOffHandWeapon(weapon, true);
    }

    public void EquipRangedWeapon(EquippableWeapon weapon)
    {
        if (!weapon.EquippableRanged || !weapon.IsEquippable())
        {
            return;
        }
        if (IsRangedEquipped())
        {
            if (weapon == RangedWeapon)
            {
                return;
            }
            UnequipRangedWeapon();
        }
        RangedWeapon = weapon;
        //slot.slot = slot.weapon.MainHandEquipSlot;

        GenerateRangedModel();
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(this.player);

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void UnequipMainWeapon()
    {
        if (!IsMainEquipped())
        {
            return;
        }
        MainWeapon.UnequipWeapon(player);
        MainWeapon.isEquipped = false;
        MainWeapon.DestroyModel();
        //MainWeapon.isMain = false;
        //MainWeapon.isOff = false;
        //MainWeapon.isDrawn = false;
        //MainWeapon.is2h = false;
        MainWeapon = null;
        player.ResetMainRotation();
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
        OffWeapon.UnequipWeapon(player);
        OffWeapon.isEquipped = false;
        OffWeapon.DestroyModel();
        //OffWeapon.isMain = false;
        //OffWeapon.isOff = false;
        //OffWeapon.isDrawn = false;
        //OffWeapon.is2h = false;
        OffWeapon = null;
        player.RotateOffWeapon(0f);
        PositionWeapon();

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void UnequipRangedWeapon()
    {
        if (!IsRangedEquipped())
        {
            return;
        }
        RangedWeapon.UnequipWeapon(player);
        RangedWeapon.isEquipped = false;
        RangedWeapon.DestroyModel();
        //MainWeapon.isMain = false;
        //MainWeapon.isOff = false;
        //MainWeapon.isDrawn = false;
        //MainWeapon.is2h = false;
        RangedWeapon = null;

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

    public void GenerateRangedModel()
    {
        if (IsRangedEquipped())
        {
            RangedWeapon.GenerateModel();
        }
    }
    public void GenerateModels()
    {
        GenerateMainModel();
        GenerateOffModel();
        GenerateRangedModel();
    }

    
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

        if (IsOffEquipped())
        {
            if (GetMainWeapon().TwoHandOnly())
            {
                UnequipOffHandWeapon();
            }
            else if (IsOffDrawn() && TwoHanding)
            {
                player.TriggerSheath(false, GetOffWeapon().OffHandEquipSlot, false);
                //SetDrawn(false, !TwoHanding);
            }
        }
        
        OnChange.Invoke();
        weaponChanged = true;
        
    }
    
    public void PositionWeapon()
    {
        // main
        if (IsMainEquipped())
        {
            GameObject parent;
            bool mIdentity = false;
            if (IsMainDrawn() && !MainWeapon.ParentLeftAsMain)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsMainDrawn() && MainWeapon.ParentLeftAsMain)
            {
                parent = player.positionReference.OffHand;
            }
            else
            {
                parent = player.positionReference.GetPositionRefSlot(MainWeapon.MainHandEquipSlot);
                if (MainWeapon.MainHandEquipSlot == EquipSlot.cBack)
                {
                    mIdentity = true;
                }
            }
            MainWeapon.model.transform.position = parent.transform.position;
            MainWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            if (mIdentity) MainWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.forward, -parent.transform.up);
            MainWeapon.model.transform.SetParent(parent.transform, true);
        }
        // off
        if (IsOffEquipped())
        {
            GameObject parent;
            bool oIdentity = false;
            if (IsOffDrawn() && OffWeapon.ParentRightAsOff)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsOffDrawn() && !OffWeapon.ParentRightAsOff)
            {
                parent = player.positionReference.OffHand;
            }
            else
            {
                parent = player.positionReference.GetPositionRefSlot(OffWeapon.OffHandEquipSlot);
                if (OffWeapon.OffHandEquipSlot == EquipSlot.cBack)
                {
                    oIdentity = true;
                }
            }
            OffWeapon.model.transform.position = parent.transform.position;
            OffWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            if (oIdentity) OffWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.forward, -parent.transform.up);
            OffWeapon.model.transform.SetParent(parent.transform, true);
        }
        // ranged
        if (IsRangedEquipped())
        {
            GameObject parent;
            bool rIdentity = false;
            if (IsRangedDrawn() && !RangedWeapon.ParentLeftAsMain)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsRangedDrawn() && RangedWeapon.ParentLeftAsMain)
            {
                parent = player.positionReference.OffHand;
            }
            else
            {
                parent = player.positionReference.GetPositionRefSlot(RangedWeapon.RangedEquipSlot);
                if (RangedWeapon.RangedEquipSlot == EquipSlot.cBack)
                {
                    rIdentity = true;
                }

            }
            RangedWeapon.model.transform.position = parent.transform.position;
            RangedWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            if (rIdentity) RangedWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.forward, -parent.transform.up);
            RangedWeapon.model.transform.SetParent(parent.transform, true);
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

    public bool IsRangedDrawn()
    {
        return IsRangedEquipped() && RangedIsDrawn;
    }

    public void UpdateWeapon()
    {
        if (IsMainEquipped())
        {
            MainWeapon.UpdateWeapon(player);
        }
        if (IsOffEquipped())
        {
            OffWeapon.UpdateWeapon(player);
        }
        if (IsRangedEquipped())
        {
            RangedWeapon.UpdateWeapon(player);
        }
    }
// during fixed update
    public void FixedUpdateWeapon()
    {
        if (IsMainEquipped())
        {
            MainWeapon.FixedUpdateWeapon(player);
        }
        if (IsOffEquipped())
        {
            OffWeapon.FixedUpdateWeapon(player);
        }
        if (IsRangedEquipped())
        {
            RangedWeapon.FixedUpdateWeapon(player);
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

    public EquippableWeapon GetOffWeapon()
    {
        if (IsOffEquipped())
        {
            return OffWeapon;
        }
        return null;
    }

    public EquippableWeapon GetRangedWeapon()
    {
        if (IsRangedEquipped())
        {
            return RangedWeapon;
        }
        return null;
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
            weight += GetOffWeapon().GetWeight();
        }

        if (IsRangedEquipped())
        {
            weight += GetRangedWeapon().GetWeight();
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


    public void SetDrawn(int type, bool drawn)
    {
        switch (type)
        {
            case Inventory.MainType: // main
                if (IsMainEquipped())
                {
                    MainIsDrawn = drawn;
                }
                break;
            case Inventory.OffType: // off
                if (IsOffEquipped())
                {
                    OffIsDrawn = drawn;
                }
                break;
            case Inventory.RangedType: // ranged
                if (IsRangedEquipped())
                {
                    RangedIsDrawn = drawn;
                }
                break;
        }
        PositionWeapon();
        weaponChanged = true;
    }

    public void SetDrawn(bool main, bool drawn)
    {
        SetDrawn((main) ? 0 : 1, drawn);
    }

    public void EquipToSlot(EquippableWeapon weapon, int slot)
    {
        // remove from old slot
        int currentSlot = FindSlotFromWeapon(weapon);
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
        UnequipRangedWeapon();
    }

    public int FindSlotFromWeapon(EquippableWeapon weapon)
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

    public EquippableWeapon FindWeaponFromSlot(int slot)
    {
        EquippableWeapon weapon = null;
        if (slot == 0)
        {
            weapon = Slot0Weapon;
        }
        else if (slot == 1)
        {
            weapon = Slot1Weapon;
        }
        else if (slot == 2)
        {
            weapon = Slot2Weapon;
        }
        else if (slot == 3)
        {
            weapon = Slot3Weapon;
        }
        return weapon;
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

    public bool IsRangedEquipped()
    {
        if (RangedWeapon == null)
        {
            return false;
        }
        if (RangedWeapon.itemName == "")
        {
            return false;
        }
        return true;
    }


    public void InputOnSlot(int slot)
    {
        if (InventoryUI2.invUI.awaitingQuickSlotEquipInput)
        {
            Item item = InventoryUI2.invUI.quickSlotItem;
            if (item != null && item is EquippableWeapon weapon)
            {
                EquipToSlot(weapon, slot);
                OnChange.Invoke();
            }
            InventoryUI2.invUI.EndQuickSlotEquip();
        }
        else
        {
            EquippableWeapon weapon = FindWeaponFromSlot(slot);

            if (weapon != null)
            {
                bool isMain = weapon == MainWeapon;
                bool isOff = weapon == OffWeapon;
                bool isRanged = weapon == RangedWeapon;
                if (isMain)
                {
                    if (!IsMainDrawn())
                    {
                        player.TriggerSheath(true, MainWeapon.MainHandEquipSlot, true);
                    }
                    else
                    {
                        //UpdateTwoHand(!TwoHanding);
                    }
                }
                else if (isOff)
                {
                    if (!IsOffDrawn())
                    {
                        player.TriggerSheath(true, OffWeapon.OffHandEquipSlot, false);
                    }
                    else if (weapon.EquippableMain)
                    {
                        if (IsMainEquipped())
                        {
                            if (MainWeapon.EquippableOff)
                            {
                                EquippableWeapon mw = GetMainWeapon();
                                EquipOffHandWeapon(mw);
                            }
                        }
                        EquipMainWeapon(weapon);
                    }
                }
                else if (isRanged)
                {
                    if (weapon.EquippableOff)
                    {
                        EquipOffHandWeapon(weapon);
                    }
                    else if (weapon.EquippableMain)
                    {
                        EquipMainWeapon(weapon);
                    }
                }
                else
                {
                    if (weapon.EquippableRanged && weapon is RangedWeapon)
                    {
                        EquipRangedWeapon(weapon);
                    }
                    else if ((IsMainEquipped() && weapon.EquippableOff && !MainWeapon.TwoHandOnly()) || (weapon.EquippableOff && !weapon.EquippableMain && (!IsMainEquipped() || !MainWeapon.TwoHandOnly())))
                    {
                        EquipOffHandWeapon(weapon, IsMainDrawn());
                    }
                    else if (weapon.EquippableMain)
                    {
                        if (weapon.TwoHandOnly())
                        {
                            UnequipOffHandWeapon();
                        }
                        EquipMainWeapon(weapon);
                    }
                    else if (weapon.EquippableOff && MainWeapon.TwoHandOnly())
                    {
                        if (Slot0Weapon != null && Slot0Weapon != weapon && Slot0Weapon.EquippableMain && !Slot0Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot0Weapon, false);
                        }
                        else if (Slot1Weapon != null && Slot1Weapon != weapon && Slot1Weapon.EquippableMain && !Slot1Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot1Weapon, false);
                        }
                        else if (Slot2Weapon != null && Slot2Weapon != weapon && Slot2Weapon.EquippableMain && !Slot2Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot2Weapon, false);
                        }
                        else
                        {
                            UnequipMainWeapon();
                        }
                        EquipOffHandWeapon(weapon);
                    }
                }
            }
        }
    }

    public void UnequipOnSlot(int slot)
    {
        UnequipWeapon(FindWeaponFromSlot(slot));
    }
    public GameObject GetOffhandModel()
    {
        return OffWeapon.model;
    }

    public GameObject GetWeaponModel()
    {
        return MainWeapon.model;
    }
    
    public GameObject GetRangedModel()
    {
        return RangedWeapon.model;
    }

    public void UnequipWeapon(EquippableWeapon weapon)
    {
        if (weapon == null) return;
        switch (GetItemEquipType(weapon))
        {
            case Inventory.MainType:
                UnequipMainWeapon();
                break;
            case Inventory.OffType:
                UnequipOffHandWeapon();
                break;
            case Inventory.RangedType:
                UnequipRangedWeapon();
                break;
        }
    }
    
    public EquippableWeapon GetBlockWeapon()
    {
        if (IsOffEquipped() && GetOffWeapon().GetMoveset().overridesBlock)
        {
            return GetOffWeapon();
        }
        else if (IsMainEquipped() && GetMainWeapon().GetMoveset().overridesBlock)
        {
            return GetMainWeapon();
        }
        else
        {
            return null;
        }
    }
    public DamageResistance[] GetBlockResistance()
    {
        if (GetBlockWeapon() != null)
        {
            return GetBlockWeapon().blockResistances;
        }
        else
        {
            return new DamageResistance[0];
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
            return GetOffWeapon().GetBlockPoiseDamage();
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

    public int GetItemEquipType(EquippableWeapon weapon)
    {
        if (weapon == this.MainWeapon)
        {
            return Inventory.MainType;
        }
        else if (weapon == this.OffWeapon)
        {
            return Inventory.OffType;
        }
        else if (weapon == this.RangedWeapon)
        {
            return Inventory.RangedType;
        }
        else
        {
            return -1; // not equipped
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