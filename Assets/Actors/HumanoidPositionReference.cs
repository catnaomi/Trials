using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtilities;

public class HumanoidPositionReference : MonoBehaviour
{
    [Header("Body & Joint Positions")]
    public Transform Hips;
    public Transform Spine;
    public Transform Head;
    [Space(5)]
    public Transform FootL;
    public Transform FootR;
    [Space(5)]
    public Transform ShoulderR;
    public Transform ShoulderL;
    [Header("Weapon Positions")]
    public GameObject MainHand;
    public GameObject OffHand;
    [Space(5)]
    public GameObject rHip;
    public GameObject rBack;
    public GameObject lHip;
    public GameObject lBack;
    public GameObject cBack;
    [Space(5)]
    public float eyeHeight;
    [Space(5)]
    public Transform centerTarget;
    public Transform eyeTarget;
    [Header("Masks")]
    public AvatarMask upperBodyMask;
    public AvatarMask fullBodyMask;

    // Use this for initialization
    void Awake()
    {
        AnimancerPlayable.LayerList.SetMinDefaultCapacity(10);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LocateSlotsByName()
    {
        LocateSlotsByName(false);
    }
    public void LocateSlotsByName(bool assetLocate)
    {
        string HAND_R_NAME = "_equipHandR";
        string HAND_L_NAME = "_equipHandL";

        string HIPS_NAME = "Hips";
        string SPINE_NAME = "Spine";
        string HEAD_NAME = "Head";

        string FOOT_R_NAME = "RightFoot";
        string FOOT_L_NAME = "LeftFoot";

        string SHOULDER_R_NAME = "RightShoulder";
        string SHOULDER_L_NAME = "LeftShoulder";
        Dictionary<Inventory.EquipSlot, string> SLOT_NAMES = new Dictionary<Inventory.EquipSlot, string> {
            {Inventory.EquipSlot.rHip, "_equipSheathR" },
            {Inventory.EquipSlot.lHip, "_equipSheathL" },
            {Inventory.EquipSlot.rBack, "_equipBackR" },
            {Inventory.EquipSlot.lBack, "_equipBackL" },
            {Inventory.EquipSlot.cBack, "_equipBackC" }
        };

        Transform current;

        if (this.Spine == null)
        {
            //current = LocateSlotsRecursive(this.transform, SPINE_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, SPINE_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, SPINE_NAME);
            }
            if (current != null)
            {
                this.Spine = current;
            }
        }

        if (this.Hips == null)
        {
            //current = LocateSlotsRecursive(this.transform, HIPS_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, HIPS_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, HIPS_NAME);
            }
            if (current != null)
            {
                this.Hips = current;
            }
        }

        if (this.Head == null)
        {
            //current = LocateSlotsRecursive(this.transform, HEAD_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, HEAD_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, HEAD_NAME);
            }
            if (current != null)
            {
                this.Head = current;
            }
        }

        if (this.MainHand == null)
        {
            //current = LocateSlotsRecursive(this.transform, HAND_R_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, HAND_R_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, HAND_R_NAME);
            }
            if (current != null)
            {
                this.MainHand = current.gameObject;
            }
        }
        if (this.OffHand == null)
        {
            //current = LocateSlotsRecursive(this.transform, HAND_L_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, HAND_L_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, HAND_L_NAME);
            }
            if (current != null)
            {
                this.OffHand = current.gameObject;
            }
        }
        if (this.FootR == null)
        {
            //current = LocateSlotsRecursive(this.transform, FOOT_R_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, FOOT_R_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, FOOT_R_NAME);
            }
            if (current != null)
            {
                this.FootR = current;
            }
        }
        if (this.FootL == null)
        {
            //current = LocateSlotsRecursive(this.transform, FOOT_L_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, FOOT_L_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, FOOT_L_NAME);
            }
            if (current != null)
            {
                this.FootL = current;
            }
        }
        if (this.ShoulderR == null)
        {
            //current = LocateSlotsRecursive(this.transform, SHOULDER_R_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, SHOULDER_R_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, SHOULDER_R_NAME);
            }
            if (current != null)
            {
                this.ShoulderR = current;
            }
        }
        if (this.ShoulderL == null)
        {
            //current = LocateSlotsRecursive(this.transform, SHOULDER_L_NAME);
            if (assetLocate)
            {
                current = LocateAssetSlot(this.transform, SHOULDER_L_NAME);
            }
            else
            {
                current = LocateSlotsRecursive(this.transform, SHOULDER_L_NAME);
            }
            if (current != null)
            {
                this.ShoulderL = current;
            }
        }

        foreach (Inventory.EquipSlot slot in SLOT_NAMES.Keys)
        {
            if (this.GetPositionRefSlot(slot) == null)
            {
                //current = LocateSlotsRecursive(this.transform, SLOT_NAMES[slot]);
                if (assetLocate)
                {
                    current = LocateAssetSlot(this.transform, SLOT_NAMES[slot]);
                }
                else
                {
                    current = LocateSlotsRecursive(this.transform, SLOT_NAMES[slot]);
                }
                if (current != null)
                {
                    this.SetPositionRefSlot(slot, current.gameObject);
                }
            }
        }
    }
    /*
    Transform LocateSlotsRecursive(Transform t, string n)
    {
        Transform s = t.Find(n);
        if (s == null)
        {
            foreach (Transform c in t)
            {
                Transform found = LocateSlotsRecursive(c, n);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        else
        {
            return s;
        }
    }
    */
    Transform LocateSlotsRecursive(Transform t, string n)
    {
        if (t.name.ToLower().Contains(n.ToLower()))
        {
            return t;
        }
        else
        {
            if (t.childCount == 0)
            {
                return null;
            }
            else
            {
                foreach (Transform c in t)
                {
                    Transform found = LocateSlotsRecursive(c, n);
                    if (found != null)
                    {
                        return found;
                    }
                }
                return null;
            }
            
        }
        
    }

    Transform LocateAssetSlot(Transform t, string n)
    {
        if (t.name.ToLower().Contains(n.ToLower()))
        {
            return t;
        }
        else
        {
            if (t.childCount == 0)
            {
                return null;
            }
            else
            {
                Transform[] cs = t.gameObject.GetComponentsInChildrenOfAsset<Transform>();
                foreach (Transform c in cs)
                {
                    if (c.name.ToLower().Contains(n.ToLower()))
                    {
                        return c;
                    }
                }
                return null;
            }

        }
    }
    public GameObject GetPositionRefSlot(Inventory.EquipSlot slot)
    {
        switch (slot)
        {
            case Inventory.EquipSlot.rHip:
                return this.rHip;
            case Inventory.EquipSlot.lHip:
                return this.lHip;
            case Inventory.EquipSlot.rBack:
                return this.rBack;
            case Inventory.EquipSlot.lBack:
                return this.lBack;
            case Inventory.EquipSlot.cBack:
                return this.cBack;
            default:
                return null;
        }
    }

    public void SetPositionRefSlot(Inventory.EquipSlot slot, GameObject newSlot)
    {
        switch (slot)
        {
            case Inventory.EquipSlot.rHip:
                this.rHip = newSlot;
                break;
            case Inventory.EquipSlot.lHip:
                this.lHip = newSlot;
                break;
            case Inventory.EquipSlot.rBack:
                this.rBack = newSlot;
                break;
            case Inventory.EquipSlot.lBack:
                this.lBack = newSlot;
                break;
            case Inventory.EquipSlot.cBack:
                this.cBack = newSlot;
                break;
        }
    }
}