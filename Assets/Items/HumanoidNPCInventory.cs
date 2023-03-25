using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HumanoidNPCInventory : Inventory, IInventory, IHumanoidInventory
{
    Actor actor;
    HumanoidPositionReference positionReference;

    [SerializeField, ReadOnly] private List<Item> drops;
    [SerializeField, ReadOnly] private EquippableWeapon MainWeapon;
    [SerializeField, ReadOnly] private bool MainIsDrawn;
    [SerializeField, ReadOnly] private EquippableWeapon OffWeapon;
    [SerializeField, ReadOnly] private bool OffIsDrawn;
    [SerializeField, ReadOnly] private EquippableWeapon RangedWeapon;
    [SerializeField, ReadOnly] private bool RangedIsDrawn;

    [Header("Starting Inventory Contents")]
    public List<Item> DroppedContents;

    [Space(10)]
    public EquippableWeapon equipToMain;
    public bool shouldEquipMain = false;
    [Space(5)]
    public EquippableWeapon equipToOff;
    public bool shouldEquipOff = false;
    [Space(5)]
    public EquippableWeapon equipToRanged;
    public bool shouldEquipRanged = false;

    bool weaponChanged;
    void Awake()
    {
        this.actor = this.GetComponent<Actor>();
        this.positionReference = this.GetComponent<HumanoidPositionReference>();
        contents = new List<Item>();
        drops = new List<Item>();
        foreach (Item item in StartingContents)
        {
            if (item != null)
            {
                Item newItem = ScriptableObject.Instantiate(item);
                Add(newItem);
            }
        }
        foreach (Item item in DroppedContents)
        {
            if (item != null)
            {
                Item newItem = ScriptableObject.Instantiate(item);
                Add(newItem);
                drops.Add(newItem);
            }
        }
        StartingContents.Clear();
        DroppedContents.Clear();
    }
    void Start()
    {

        MainWeapon = null;
        OffWeapon = null;
        RangedWeapon = null;

        EquippableWeapon mweapon = null;
        EquippableWeapon oweapon = null;
        EquippableWeapon rweapon = null;

        if (equipToMain != null)
        {
            mweapon = Instantiate(equipToMain);
            equipToMain = mweapon;
            Add(mweapon);
            if (shouldEquipMain)
            {
                EquipMainWeapon(mweapon);
            }
        }
        if (equipToOff != null)
        {
            oweapon = Instantiate(equipToOff);
            equipToOff = oweapon;
            Add(oweapon);
            if (shouldEquipOff)
            {
                EquipOffHandWeapon(oweapon);
            }
        }
        if (equipToRanged != null)
        {
            rweapon = Instantiate(equipToRanged);
            equipToRanged = rweapon;
            Add(rweapon);
            if (shouldEquipRanged)
            {
                EquipRangedWeapon(rweapon);
            }
        }

        actor.OnDie.AddListener(CreateDrops);
    }

    public bool Add(Item item)
    {
        if (item != null)
        {
            contents.Add(item);
            item.holder = actor;
            OnChange.Invoke();
            return true;
        }
        return false;
    }

    public bool Remove(Item item)
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

    public List<Item> GetContents()
    {
        return contents;
    }

    public bool Contains(Item item)
    {
        return contents.Contains(item);
    }

    public void Clear()
    {
        contents.Clear();
    }

    public UnityEvent GetChangeEvent()
    {
        return OnChange;
    }

    #region EQUIPS

    public void EquipMainWeapon(EquippableWeapon weapon)
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
        weapon.EquipWeapon(actor);

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void EquipOffHandWeapon(EquippableWeapon weapon)
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
        weapon.EquipWeapon(actor);

        OnChange.Invoke();
        weaponChanged = true;
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
        weapon.EquipWeapon(actor);

        OnChange.Invoke();
        weaponChanged = true;
    }

    public void UnequipRangedWeapon()
    {
        RangedWeapon.UnequipWeapon(actor);
        RangedWeapon.isEquipped = false;
        RangedWeapon.DestroyModel();
        RangedWeapon = null;

        PositionWeapon();

        OnChange.Invoke();
        weaponChanged = true;

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

    #endregion;

    #region MODELS
    public void GenerateModels()
    {
        GenerateMainModel();
        GenerateOffModel();
        GenerateRangedModel();
    }
    public void PositionWeapon()
    {
        // main
        if (IsMainEquipped())
        {
            GameObject parent;
            if (IsMainDrawn() && !MainWeapon.ParentLeft)
            {
                parent = positionReference.MainHand;
            }
            else if (IsMainDrawn() && MainWeapon.ParentLeft)
            {
                parent = positionReference.OffHand;
            }
            else
            {
                parent = positionReference.GetPositionRefSlot(MainWeapon.MainHandEquipSlot);
            }
            MainWeapon.model.transform.position = parent.transform.position;
            MainWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            MainWeapon.model.transform.SetParent(parent.transform, true);
        }
        // off
        if (IsOffEquipped())
        {
            GameObject parent;
            if (IsOffDrawn() && !OffWeapon.ParentLeft)
            {
                parent = positionReference.MainHand;
            }
            else if (IsOffDrawn() && OffWeapon.ParentLeft)
            {
                parent = positionReference.OffHand;
            }
            else
            {
                parent = positionReference.GetPositionRefSlot(OffWeapon.OffHandEquipSlot);
            }
            OffWeapon.model.transform.position = parent.transform.position;
            OffWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            OffWeapon.model.transform.SetParent(parent.transform, true);
        }
        // ranged
        if (IsRangedEquipped())
        {
            GameObject parent;
            if (IsRangedDrawn() && !RangedWeapon.ParentLeft)
            {
                parent = positionReference.MainHand;
            }
            else if (IsRangedDrawn() && RangedWeapon.ParentLeft)
            {
                parent = positionReference.OffHand;
            }
            else
            {
                parent = positionReference.GetPositionRefSlot(RangedWeapon.RangedEquipSlot);
            }
            RangedWeapon.model.transform.position = parent.transform.position;
            RangedWeapon.model.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            RangedWeapon.model.transform.SetParent(parent.transform, true);
        }
    }

    public bool IsWeaponDrawn()
    {
        return IsMainDrawn();
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

    #endregion

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

    public void SetDrawn(bool main, bool drawn)
    {
        SetDrawn((main) ? 0 : 1, drawn);
    }

    public void SetDrawn(int type, bool drawn)
    {
        switch (type)
        {
            case Inventory.MainType: // main
                if (IsMainEquipped())
                {
                    MainIsDrawn = drawn;
                    if (IsRangedEquipped() && MainWeapon == RangedWeapon)
                    {
                        RangedIsDrawn = drawn;
                    }
                }
                break;
            case Inventory.OffType: // off
                if (IsOffEquipped())
                {
                    OffIsDrawn = drawn;
                    if (IsRangedEquipped() && OffWeapon == RangedWeapon)
                    {
                        RangedIsDrawn = drawn;
                    }
                }
                break;
            case Inventory.RangedType: // ranged
                if (IsRangedEquipped())
                {
                    RangedIsDrawn = drawn;
                    if (IsMainEquipped() && MainWeapon == RangedWeapon)
                    {
                        MainIsDrawn = drawn;
                    }
                    else if (IsOffEquipped() && OffWeapon == RangedWeapon)
                    {
                        OffIsDrawn = drawn;
                    }
                }
                break;
        }
        PositionWeapon();
        weaponChanged = true;
    }

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

    public void CreateDrops()
    {
        if (drops == null || drops.Count == 0) return;


        float mag = 0.2f * drops.Count;
        for (int i = 0; i < drops.Count; i++)
        {
            Item item = drops[i];
            LooseItem looseItem = LooseItem.CreateLooseItem(item);
            Rigidbody rigidbody = looseItem.GetComponent<Rigidbody>();

            float angle = (i / drops.Count) * 360f;

            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * mag;

            rigidbody.position = actor.transform.position + Vector3.up * 0.5f + dir;
        }
    }
}