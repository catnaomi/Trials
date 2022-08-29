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

    private bool RangedIsMain;
    private bool RangedIsOff;

    EquippableWeapon BlockingWeapon;

    public bool equipOnStart = true;
    public bool initialized = false;
    [Header("Inspector-set Weapons")]
    [Tooltip("Up Slot. Equips to Mainhand.")]
    public Equippable Slot0Equippable; // starts equipped. up
    [Tooltip("Left Slot. Equips to Offhand.")]
    public Equippable Slot1Equippable; // left
    [Tooltip("Right Slot. Equips to Ranged.")]
    public Equippable Slot2Equippable; // right
    [Tooltip("Botton Slot. Does not Equip.")]
    public Equippable Slot3Equippable; // down

    public bool weaponChanged;

    public PassItemEvent OnAddItem;
    void Awake()
    {
        this.player = GetComponent<PlayerActor>();
        contents = new List<Item>();
        foreach (Item item in StartingContents)
        {
            if (item != null)
            {
                Item newItem = ScriptableObject.Instantiate(item);
                Add(newItem);
            }
        }
        OnChange.AddListener(() => { lastChanged = Time.time; });
        StartingContents.Clear();

        if (Slot0Equippable != null)
        {
            Equippable slot0 = Instantiate(Slot0Equippable);
            Add(slot0);
            Slot0Equippable = slot0;

        }

        if (Slot1Equippable != null)
        {
            Equippable slot1 = Instantiate(Slot1Equippable);
            Add(slot1);
            Slot1Equippable = slot1;
        }

        if (Slot2Equippable != null)
        {
            Equippable slot2 = Instantiate(Slot2Equippable);
            Add(slot2);
            Slot2Equippable = slot2;
        }

        if (Slot3Equippable != null)
        {
            Equippable slot3 = Instantiate(Slot3Equippable);
            Add(slot3);
            Slot3Equippable = slot3;
        }
    }
    void Start()
    {
        
        MainWeapon = null;
        OffWeapon = null;
        
    }

    void Update()
    {
        if (!initialized)
        {
            if (equipOnStart)
            {
                EquippableWeapon mweapon = (Slot0Equippable != null && Slot0Equippable is EquippableWeapon) ? (EquippableWeapon)Slot0Equippable : null;
                EquippableWeapon oweapon = (Slot1Equippable != null && Slot1Equippable is EquippableWeapon) ? (EquippableWeapon)Slot1Equippable : null;
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
    public override bool Add(Item item)
    {
        OnAddItem.Invoke(item);
        return base.Add(item);
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

        if (weapon.EquippableRanged && weapon is RangedWeapon rweapon && !(IsOffEquipped() && RangedIsOff))
        {
            //RangedWeapon = rweapon;
            RangedIsMain = true;
            RangedIsOff = false;
        }
        else
        {
            RangedIsMain = false;
        }
        MainIsDrawn = false;
        GenerateMainModel();
        //ValidateHandedness(Inventory.MainType);
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(this.player);

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
        if (weapon.EquippableRanged && weapon is RangedWeapon rweapon)
        {
            RangedIsMain = false;
            RangedIsOff = true;
        }
        else
        {
            RangedIsOff = false;
        }
        OffIsDrawn = false;
        
        GenerateModels();
        //ValidateHandedness(Inventory.OffType);
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(player);

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
        RangedIsMain = false;
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
        RangedIsOff = false;
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
        if (RangedIsMain)
        {
            UnequipMainWeapon();
        }
        else if (RangedIsOff)
        {
            UnequipOffHandWeapon();
        }
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

    
    public void ValidateHandedness(int prioritySlot)
    {
        bool isConflict = (IsOffDrawn() && IsMainDrawn()) && (GetMainWeapon().TwoHandOnly() || GetOffWeapon().TwoHandOnly());

        if (isConflict)
        {
            if (prioritySlot == Inventory.MainType)
            {
                if (RangedIsOff)
                {
                    SetDrawn(false, false);
                }
                else
                {
                    UnequipOffHandWeapon();
                }
            }
            else if (prioritySlot == Inventory.OffType)
            {
                SetDrawn(true, false);
            }
            else if (prioritySlot == Inventory.RangedType)
            {
                if (RangedIsMain)
                {
                    SetDrawn(false, false);
                }
                else if (RangedIsOff)
                {
                    SetDrawn(true, false);
                }
            }
        }   
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
        return IsRangedEquipped() && ((RangedIsMain && IsMainDrawn()) || (RangedIsOff && IsOffDrawn()));
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
            if (RangedIsMain)
            {
                return MainWeapon;
            }
            else if (RangedIsOff)
            {
                return OffWeapon;
            }
            else
            {
                return null;
            }
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
                    if (RangedIsMain)
                    {
                        MainIsDrawn = drawn;
                    }
                    else if (RangedIsOff)
                    {
                        OffIsDrawn = drawn;
                    }
                }
                break;
        }
        ValidateHandedness(type);
        PositionWeapon();
        weaponChanged = true;
    }

    public void SetDrawn(bool main, bool drawn)
    {
        SetDrawn((main) ? 0 : 1, drawn);
    }

    public void EquipToSlot(Equippable weapon, int slot)
    {
        // remove from old slot
        int currentSlot = FindSlotFromWeapon(weapon);
        if (currentSlot == 0)
        {
            Slot0Equippable = null;
        }
        else if (currentSlot == 1)
        {
            Slot1Equippable = null;
        }
        else if (currentSlot == 2)
        {
            Slot2Equippable = null;
        }
        else if (currentSlot == 3)
        {
            Slot3Equippable = null;
        }

        // add to new slot
        if (slot == 0)
        {
            Slot0Equippable = weapon;
        }
        else if (slot == 1)
        {
            Slot1Equippable = weapon;
        }
        else if (slot == 2)
        {
            Slot2Equippable = weapon;
        }
        else if (slot == 3)
        {
            Slot3Equippable = weapon;
        }

        bool changed = false;
        if (FindSlotFromWeapon(GetMainWeapon()) < 0)
        {
            UnequipMainWeapon();
            changed = true;
        }
        if (FindSlotFromWeapon(GetOffWeapon()) < 0)
        {
            UnequipOffHandWeapon();
            changed = true;
        }
        if (FindSlotFromWeapon(GetRangedWeapon()) < 0)
        {
            UnequipRangedWeapon();
            changed = true;
        }
        if (!changed)
        {
            OnChange.Invoke();
        }
    }

    public int FindSlotFromWeapon(Equippable weapon)
    {
        if (Slot0Equippable == weapon)
        {
            return 0;
        }
        else if (Slot1Equippable == weapon)
        {
            return 1;
        }
        else if (Slot2Equippable == weapon)
        {
            return 2;
        }
        else if (Slot3Equippable == weapon)
        {
            return 3;
        }
        return -1;
    }

    public Equippable FindItemFromSlot(int slot)
    {
        Equippable item = null;
        if (slot == 0)
        {
            item = Slot0Equippable;
        }
        else if (slot == 1)
        {
            item = Slot1Equippable;
        }
        else if (slot == 2)
        {
            item = Slot2Equippable;
        }
        else if (slot == 3)
        {
            item = Slot3Equippable;
        }
        return item;
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
        if (RangedIsMain)
        {
            if (IsMainEquipped())
            {
                RangedIsOff = false;
                return true;
            }
            else
            {
                RangedIsMain = false;
                return false;
            }
        }
        else if (RangedIsOff)
        {
            if (IsOffEquipped())
            {
                RangedIsMain = false;
                return true;
            }
            else
            {
                RangedIsOff = false;
                return false;
            }
        }
        else
        {
            return false;
        }
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
            Equippable item = FindItemFromSlot(slot);
            EquippableWeapon weapon = item as EquippableWeapon;
            Consumable consumable = item as Consumable;
            if (weapon != null)
            {
                bool isMain = weapon == MainWeapon;
                bool isOff = weapon == OffWeapon;
                bool isRanged = weapon == GetRangedWeapon();
                if (isMain)
                {
                    player.TriggerSheath(!IsMainDrawn(), MainWeapon.MainHandEquipSlot, true);
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
                    else if (IsOffDrawn())
                    {
                        player.TriggerSheath(false, OffWeapon.OffHandEquipSlot, false);
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
                    if ((IsMainEquipped() && weapon.EquippableOff && !MainWeapon.TwoHandOnly()) || (weapon.EquippableOff && !weapon.EquippableMain && (!IsMainEquipped() || !MainWeapon.TwoHandOnly())))
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
                        if (Slot0Equippable != null && Slot0Equippable != weapon && Slot0Equippable is EquippableWeapon Slot0Weapon && Slot0Weapon.EquippableMain && !Slot0Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot0Weapon, false);
                        }
                        else if (Slot1Equippable != null && Slot1Equippable != weapon && Slot1Equippable is EquippableWeapon Slot1Weapon && Slot1Weapon.EquippableMain && !Slot1Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot1Weapon, false);
                        }
                        else if (Slot2Equippable != null && Slot2Equippable != weapon && Slot2Equippable is EquippableWeapon Slot2Weapon && Slot2Weapon.EquippableMain && !Slot2Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot2Weapon, false);
                        }
                        else if (Slot3Equippable != null && Slot3Equippable != weapon && Slot3Equippable is EquippableWeapon Slot3Weapon && Slot3Weapon.EquippableMain && !Slot3Weapon.TwoHandOnly())
                        {
                            EquipMainWeapon(Slot3Weapon, false);
                        }
                        else
                        {
                            UnequipMainWeapon();
                        }
                        EquipOffHandWeapon(weapon);
                    }
                }
            }
            else if (consumable != null)
            {
                player.StartUsingConsumable(consumable);
            }
        }
    }

    public void UnequipOnSlot(int slot)
    {
        Equippable item = FindItemFromSlot(slot);
        if (item is EquippableWeapon weapon)
        {
            UnequipWeapon(weapon);
        }
        
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
        return GetRangedWeapon().model;
    }

    public bool IsAnyWeaponDrawn()
    {
        return IsMainDrawn() || IsOffDrawn() || IsRangedDrawn();
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
    public DamageResistance GetBlockResistance()
    {
        if (GetBlockWeapon() != null)
        {
            return GetBlockWeapon().blockResistances;
        }
        else
        {
            return new DamageResistance();
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
        else if (weapon == this.GetRangedWeapon())
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