using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DojoBossMecanimActor), typeof(HumanoidPositionReference))]
public class DojoBossInventoryTransformingController : MonoBehaviour, IHumanoidInventory, IInventory
{

    DojoBossMecanimActor actor;
    HumanoidPositionReference positionReference;
    [Header("Transforming Weapon Stats")]
    [SerializeField] WeaponStats[] weapons;
    [Space(10)]
    [SerializeField, ReadOnly] WeaponStats currentWeapon;
    [Header("Weapon References")]
    public TransformingWeapon transformingWeapon;
    [ReadOnly] public TransformingWeapon weaponMainInstance;
    [ReadOnly] public TransformingWeapon weaponOffInstance;
    GameObject emptyMain;
    GameObject emptyOff;
    public UnityEvent OnChange;

    void Awake()
    {
        if (transformingWeapon != null)
        {
            weaponMainInstance = Instantiate(transformingWeapon);
            weaponOffInstance = Instantiate(transformingWeapon);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<DojoBossMecanimActor>();
        positionReference = this.GetComponent<HumanoidPositionReference>();
        emptyMain = new GameObject("Empty Main Hand");
        emptyOff = new GameObject("Empty Off Hand");
        if (weaponMainInstance != null && weaponOffInstance != null)
        {
            weaponMainInstance.holder = actor;
            weaponOffInstance.holder = actor;
            SetWeapon(weapons[0]);

            weaponMainInstance.EquipWeapon(actor);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetWeapon(WeaponStats stats)
    {
        if (weaponMainInstance == null || weaponOffInstance == null) return;

        currentWeapon = stats;
        // TODO: prefab pooling

        // generate models
        GenerateModels(stats);

        // set weapon stats (length, width)
        SetInstanceWeaponStats(stats);

        // generate hitboxes
        GenerateHitboxes(stats);

        // position weapons
        PositionWeapons(stats);

        // update FX
        UpdateBladeFX();
    }

    public void SetWeaponByName(string name)
    {
        foreach (WeaponStats stats in weapons)
        {
            if (name.ToLower() == stats.name.ToLower())
            {
                SetWeapon(stats);
                break;
            }
        }
    }
    public void SetWeaponByIndex(int index)
    {
        SetWeapon(weapons[index]);
    }

    void GenerateModels(WeaponStats stats)
    {
        if (GetWeaponModel() != null)
        {
            GetWeaponModel().SetActive(false);
        }
        if (GetOffhandModel() != null)
        {
            GetOffhandModel().SetActive(false);
        }
        if (stats.mainHandModel != null)
        {
            stats.mainHandModel.SetActive(true);
            weaponMainInstance.model = stats.mainHandModel;
        }
        else if (stats.mainHandPrefab == null)
        {
            stats.mainHandModel = emptyMain;
            emptyMain.SetActive(true);
        }
        else
        {
            stats.mainHandModel = Instantiate(stats.mainHandPrefab);
            stats.mainHandModel.name = stats.name + "_main";
            weaponMainInstance.model = stats.mainHandModel;
            weaponMainInstance.SetModelLayer();
        }

        if (stats.offHandModel != null)
        {
            stats.offHandModel.SetActive(true);
            weaponOffInstance.model = stats.offHandModel;
        }
        else if (stats.offHandPrefab == null)
        {
            stats.offHandModel = emptyOff;
            emptyOff.SetActive(true);
        }
        else
        {
            stats.offHandModel = Instantiate(stats.offHandPrefab);
            stats.offHandModel.name = stats.name + "_off";
            weaponOffInstance.model = stats.offHandModel;
            weaponOffInstance.SetModelLayer();
        }
    }

    void SetInstanceWeaponStats(WeaponStats stats)
    {
        weaponMainInstance.length = stats.mainLength;
        weaponMainInstance.width = stats.mainWidth;
        weaponMainInstance.doubleSided = stats.mainDoubleSided;

        weaponOffInstance.length = stats.mainLength;
        weaponOffInstance.width = stats.mainWidth;
        weaponOffInstance.doubleSided = stats.offDoubleSided;
    }

    void GenerateHitboxes(WeaponStats stats)
    {
        if (stats.mainHandModel != null)
        {
            weaponMainInstance.GenerateHitboxes();
        }

        if (stats.offHandModel != null)
        {
            weaponOffInstance.GenerateHitboxes();
        }
    }

    void PositionWeapons(WeaponStats stats)
    {
        if (stats.mainHandModel != null)
        {
            GameObject parent = positionReference.MainHand;
            stats.mainHandModel.transform.position = parent.transform.position;
            stats.mainHandModel.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            stats.mainHandModel.transform.SetParent(parent.transform, true);
        }
        if (stats.offHandModel != null)
        {
            GameObject parent = positionReference.OffHand;
            stats.offHandModel.transform.position = parent.transform.position;
            stats.offHandModel.transform.rotation = Quaternion.LookRotation(parent.transform.up, parent.transform.forward);
            stats.offHandModel.transform.SetParent(parent.transform, true);
        }
    }

    void UpdateBladeFX()
    {
        weaponMainInstance.UpdateFXPoints();
        weaponOffInstance.UpdateFXPoints();
    }
    public UnityEvent GetChangeEvent()
    {
        return OnChange;
    }


    public int GetItemHand(EquippableWeapon equippableWeapon)
    {
        if (equippableWeapon == weaponMainInstance)
        {
            return Inventory.MainType;
        }
        else if (equippableWeapon == weaponOffInstance)
        {
            return Inventory.OffType;
        }
        return -1;
    }

    public GameObject GetOffhandModel()
    {
        return weaponOffInstance.model;
    }

    public GameObject GetWeaponModel()
    {
        return weaponMainInstance.model;
    }

    public float GetCurrentLength()
    {
        return currentWeapon.mainLength;
    }
    #region unused
    public bool Add(Item item)
    {
        return false;
    }

    public void Clear()
    {
        return;
    }

    public bool Contains(Item item)
    {
        return false;
    }

    public int GetAmountOf(Item item)
    {
        throw new System.NotImplementedException();
    }

    public List<Item> GetContents()
    {
        return null;
    }

    public int GetCount()
    {
        return -1;
    }

    public bool Remove(Item item)
    {
        return false;
    }

    public bool RemoveOne(Item item)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    [Serializable]
    public class WeaponStats
    {
        public string name;
        [Space(10)]
        public GameObject mainHandPrefab;
        public float mainLength;
        public bool mainDoubleSided;
        public float mainWidth;
        [Space(10)]
        public GameObject offHandPrefab;
        public float offLength;
        public bool offDoubleSided;
        public float offWidth;
        [Space(10)]
        [ReadOnly]public GameObject mainHandModel;
        [ReadOnly]public GameObject offHandModel;
    }
}
