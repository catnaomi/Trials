using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using CustomUtilities;

public class PlayerInventory : Inventory, IInventory, IHumanoidInventory
{
    public static int invID;

    PlayerActor player;
    [ReadOnly] public EquippableWeapon MainWeapon;
    private bool MainIsDrawn;
    //private bool TwoHanding;
    
    [ReadOnly] public EquippableWeapon OffWeapon;
    private bool OffIsDrawn;

    [ReadOnly] public RangedWeapon RangedWeapon;
    private bool RangedIsDrawn;

    [NonSerialized]
    public bool equipOnStart = false;
    [NonSerialized]
    public bool initialized = false;
    [Header("Inspector-set Weapons")]
    [Tooltip("Up Slot.")]
    public Equippable Slot0Equippable;
    [Tooltip("Left Slot.")]
    public Equippable Slot1Equippable;
    [Tooltip("Right Slot.")]
    public Equippable Slot2Equippable;
    [Tooltip("Botton Slot.")]
    public Equippable Slot3Equippable;

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
        else if (weapon == RangedWeapon)
        {
            UnequipRangedWeapon();
        }
        MainWeapon = weapon;
        //slot.slot = slot.weapon.MainHandEquipSlot;
        /*
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
        */
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
        MarkChanged();
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
        else if (weapon == RangedWeapon)
        {
            UnequipRangedWeapon();
        }
        OffWeapon = weapon;
        //offHandModel = GameObject.Instantiate(weapon.prefab);
        //slot.isMain = false;
        /*
        if (weapon.EquippableRanged && weapon is RangedWeapon rweapon)
        {
            RangedIsMain = false;
            RangedIsOff = true;
        }
        else
        {
            RangedIsOff = false;
        }
        */
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
        MarkChanged();
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
        //RangedIsMain = false;
        player.ResetMainRotation();
        PositionWeapon();

        OnChange.Invoke();
        MarkChanged();
        
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
        //RangedIsOff = false;
        player.RotateOffWeapon(0f);
        PositionWeapon();

        OnChange.Invoke();
        MarkChanged();
    }

    public void EquipRangedWeapon(RangedWeapon weapon)
    {
        EquipRangedWeapon(weapon, true);
    }


    public void EquipRangedWeapon(RangedWeapon weapon, bool draw)
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
        if (weapon == OffWeapon)
        {
            UnequipOffHandWeapon();
        }
        else if (weapon == MainWeapon)
        {
            UnequipMainWeapon();
        }
        RangedWeapon = weapon;
        //slot.slot = slot.weapon.MainHandEquipSlot;
        /*
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
        */
        RangedIsDrawn = false;
        GenerateRangedModel();
        //ValidateHandedness(Inventory.MainType);
        PositionWeapon();

        weapon.isEquipped = true;
        weapon.EquipWeapon(this.player);

        //player.moveset = weapon.moveset;

        if (draw)
        {
            this.player.TriggerSheath(true, RangedWeapon.RangedEquipSlot, Inventory.RangedType);
        }

        OnChange.Invoke();
        MarkChanged();
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
        //OffWeapon.isMain = false;
        //OffWeapon.isOff = false;
        //OffWeapon.isDrawn = false;
        //OffWeapon.is2h = false;
        //RangedIsOff = false;
        if (RangedWeapon.ParentLeft)
        {
            player.ResetOffRotation();
        }
        else if (!RangedWeapon.ParentLeft)
        {
            player.ResetMainRotation();
        }
        RangedWeapon = null;
        //player.RotateOffWeapon(0f);
        PositionWeapon();

        OnChange.Invoke();
        MarkChanged();
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


    public void ValidateHandedness(int prioritySlot)
    {
        bool isMultipleDrawn = (IsOffDrawn() && IsMainDrawn()) || (IsOffDrawn() && IsRangedDrawn()) || (IsRangedDrawn() && IsMainDrawn());
        bool isAnyTwoHanded = (IsOffDrawn() && GetOffWeapon().TwoHandOnly()) || (IsMainDrawn() && GetMainWeapon().TwoHandOnly()) || (IsRangedDrawn() && GetRangedWeapon().TwoHandOnly());
        int numberEquippedToRight = 0;
        int numberEquippedToLeft = 0;

        EquippableWeapon priorityWeapon = GetWeaponFromEquipType(prioritySlot);

        if (IsMainDrawn())
        {
            if (!GetMainWeapon().ParentLeft)
            {
                numberEquippedToRight++;
            }
            else
            {
                numberEquippedToLeft++;
            }
        }
        if (IsOffDrawn())
        {
            if (!GetOffWeapon().ParentLeft)
            {
                numberEquippedToRight++;
            }
            else
            {
                numberEquippedToLeft++;
            }
        }
        if (IsRangedDrawn())
        {
            if (!GetRangedWeapon().ParentLeft)
            {
                numberEquippedToRight++;
            }
            else if (GetRangedWeapon().ParentLeft)
            {
                numberEquippedToLeft++;
            }
        }

        bool isConflict = 
            (isMultipleDrawn && isAnyTwoHanded) ||
            (numberEquippedToRight >= 2) ||
            (numberEquippedToLeft >= 2);


        if (isConflict)
        {
            MainIsDrawn = (GetMainWeapon() != null && prioritySlot == Inventory.MainType);
            OffIsDrawn = (GetOffWeapon() != null && prioritySlot == Inventory.OffType);
            RangedIsDrawn = (GetRangedWeapon() != null && prioritySlot == Inventory.RangedType);
            PositionWeapon();
            MarkChanged();
        }   
    }
    
    public void PositionWeapon()
    {
        // main
        if (IsMainEquipped())
        {
            GameObject parent;
            bool mIdentity = false;
            if (IsMainDrawn() && !MainWeapon.ParentLeft)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsMainDrawn() && MainWeapon.ParentLeft)
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
            if (IsOffDrawn() && !OffWeapon.ParentLeft)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsOffDrawn() && OffWeapon.ParentLeft)
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

        if (IsRangedEquipped())
        {
            GameObject parent;
            bool rIdentity = false;
            if (IsRangedDrawn() && !RangedWeapon.ParentLeft)
            {
                parent = player.positionReference.MainHand;
            }
            else if (IsRangedDrawn() && RangedWeapon.ParentLeft)
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
            RangedWeapon.UpdateWeapon(player);
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

    public RangedWeapon GetRangedWeapon()
    {
        if (IsRangedEquipped())
        {
            return RangedWeapon;
        }
        return null;
    }

    public bool TryGetRightHandedWeapon(out EquippableWeapon weapon)
    {
        weapon = null;
        if (IsMainDrawn())
        {
            weapon = GetMainWeapon();
            if (!weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        if (IsOffDrawn())
        {
            weapon = GetOffWeapon();
            if (!weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        if (IsRangedDrawn())
        {
            weapon = GetRangedWeapon();
            if (!weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        return false;
    }

    public bool HasRightHandedWeapon()
    {
        return TryGetRightHandedWeapon(out EquippableWeapon weapon);
    }

    public bool TryGetLeftHandedWeapon(out EquippableWeapon weapon)
    {
        weapon = null;
        if (IsMainDrawn())
        {
            weapon = GetMainWeapon();
            if (weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        if (IsOffDrawn())
        {
            weapon = GetOffWeapon();
            if (weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        if (IsRangedDrawn())
        {
            weapon = GetRangedWeapon();
            if (weapon.ParentLeft || weapon.TwoHandOnly())
            {
                return true;
            }
        }
        return false;
    }

    public bool HasLeftHandedWeapon()
    {
        return TryGetLeftHandedWeapon(out EquippableWeapon weapon);
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
        ValidateHandedness(type);
        PositionWeapon();
        MarkChanged();
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
        /*
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
        */
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


    /*
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
    */

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
                if (weapon.EquippableMain)
                {
                    EquippableWeapon currentMain = GetMainWeapon();
                    if (currentMain != null)
                    {
                        UnequipMainWeapon();
                        EquipToSlot(currentMain, slot);
                    }
                    else
                    {
                        EquipToSlot(null, slot);
                    }
                    EquipMainWeapon(weapon, true);
                }
                else if (weapon.EquippableOff)
                {
                    EquippableWeapon currentOff = GetOffWeapon();
                    if (currentOff != null)
                    {
                        UnequipOffHandWeapon();
                        EquipToSlot(currentOff, slot);
                    }
                    else
                    {
                        EquipToSlot(null, slot);
                    }
                    EquipOffHandWeapon(weapon, true);
                }
                else if (weapon.EquippableRanged)
                {
                    EquippableWeapon currentRanged = GetRangedWeapon();
                    if (currentRanged != null)
                    {
                        UnequipRangedWeapon();
                        EquipToSlot(currentRanged, slot);
                    }
                    else
                    {
                        EquipToSlot(null, slot);
                    }
                    EquipRangedWeapon((RangedWeapon)weapon, true);
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
            return GetBlockWeapon().GetBlockResistance();
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
    //  -9 = not equipped
    public int GetItemHand(EquippableWeapon weapon)
    {
        if (weapon == this.MainWeapon)
        {
            if (weapon.ParentLeft)
            {
                return Inventory.OffType;
            }
            else
            {
                return Inventory.MainType;
            }
        }
        else if (weapon == this.OffWeapon)
        {
            if (weapon.ParentLeft)
            {
                return Inventory.OffType;
            }
            else
            {
                return Inventory.MainType;
            }
        }
        else if (weapon == this.RangedWeapon)
        {
            return Inventory.RangedType;
        }
        else
        {
            return -9;
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

    public EquippableWeapon GetWeaponFromEquipType(int equipType)
    {
        if (equipType == Inventory.MainType)
        {
            return GetMainWeapon();
        }
        else if (equipType == Inventory.OffType)
        {
            return GetOffWeapon();
        }
        else if (equipType == Inventory.RangedType)
        {
            return GetRangedWeapon();
        }
        else
        {
            return null;
        }
    }

    public bool IsWeaponOnAnyEquipSlot(Equippable equippable)
    {
        return (equippable is not EquippableWeapon weapon || GetItemHand(weapon) < -1) && FindSlotFromWeapon(equippable) <= -1;
    }

    public void MarkChanged()
    {
        weaponChanged = true;
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

[Serializable]
public class PlayerInventoryData
{
    // format: item_name$amount
    public string MainWeapon;
    public string OffWeapon;
    public string RangedWeapon;

    public string Slot0;
    public string Slot1;
    public string Slot2;
    public string Slot3;

    public string[] contents;

    public static PlayerInventoryData GetDataFromPlayerInventory(PlayerInventory inventory)
    {
        PlayerInventoryData data = new PlayerInventoryData();
        if (inventory.MainWeapon != null)
        {
            data.MainWeapon = inventory.MainWeapon.GetItemSaveString();
        }
        if (inventory.OffWeapon != null)
        {
            data.OffWeapon = inventory.OffWeapon.GetItemSaveString();
        }
        if (inventory.RangedWeapon != null)
        {
            data.RangedWeapon = inventory.RangedWeapon.GetItemSaveString();
        }
        if (inventory.Slot0Equippable != null)
        {
            data.Slot0 = inventory.Slot0Equippable.GetItemSaveString();
        }
        if (inventory.Slot1Equippable != null)
        {
            data.Slot1 = inventory.Slot1Equippable.GetItemSaveString();
        }
        if (inventory.Slot2Equippable != null)
        {
            data.Slot2 = inventory.Slot2Equippable.GetItemSaveString();
        }
        if (inventory.Slot3Equippable != null)
        {
            data.Slot3 = inventory.Slot3Equippable.GetItemSaveString();
        }
        List<Item> invContents = inventory.GetContents();
        data.contents = new string[invContents.Count];
        for (int i = 0; i < invContents.Count; i++)
        {
            Item item = invContents[i];
            if (item is not Equippable equippable || !inventory.IsWeaponOnAnyEquipSlot(equippable))
            {
                data.contents[i] = invContents[i].GetItemSaveString();
            }
           
        }

        return data;
    }
}