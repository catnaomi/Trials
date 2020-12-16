using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Crafting/New CraftableWeapon", order = 1)]
public class CraftableWeapon : BladeWeapon
{
    public Hilt hilt;
    public Blade blade;
    public Adornment adornment;

    #region stats

    public override float GetLength()
    {
        return GetTotalLength();
    }
    public float GetTotalLength()
    {
        return GetHiltLength() + GetBladeLength();
    }

    public override float GetHiltLength()
    {
        if (hilt != null)
        {
            return hilt.length;
        }
        return 0f;
    }

    public override float GetBladeLength()
    {
        if (blade != null)
        {
            return blade.length;
        }
        return 0f;
    }

    public override float GetWidth()
    {
        if (blade != null)
        {
            return blade.width;
        }
        return 0f;
    }

    public override float GetWeight()
    {
        return GetTotalWeight();
    }
    public float GetTotalWeight()
    {
        float weight = 0f;
        if (blade != null)
        {
            weight += blade.GetWeight();
            /*
            foreach (Inset inset in blade.insets)
            {
                if (inset != null)
                {
                    weight += inset.weight;
                }
            }
            */
        }
        if (hilt != null)
        {
            weight += hilt.GetWeight();
            /*
            foreach (Inset inset in hilt.insets)
            {
                if (inset != null)
                {
                    weight += inset.weight;
                }
            }
            */
        }
        if (adornment != null)
        {
            weight += adornment.GetWeight();

        }
        return weight;
    }

    public override float GetBalance()
    {
        float b = 0;
        if (blade != null)
        {
            b = blade.GetWeight();
        }
        float h = 0;
        if (hilt != null)
        {
            h = hilt.GetWeight();
        }
        return b-h;
    }

    public override float GetPiercingModifier()
    {
        if (blade != null)
        {
            return blade.piercingModifier;
        }
        return 0f;
    }

    public override float GetSlashingModifier()
    {
        if (blade != null)
        {
            return blade.slashingModifier;
        }
        return 0f;
    }

    public override float GetAttackSpeed(bool twoHand)
    {
        float WEIGHT_1H_OFFSET = 3f;
        float WEIGHT_2H_OFFSET = 5f;
        float WEIGHT_MOD = 0.1f;

        float BALANCE_MODIFIER = 0.2f;

        float weightOffset = (twoHand ? WEIGHT_2H_OFFSET : WEIGHT_1H_OFFSET) - GetTotalWeight();
        weightOffset *= WEIGHT_MOD;

        float balanceOffset = -GetBalance() * BALANCE_MODIFIER;

        return 1f + weightOffset + balanceOffset;
    }
    #endregion

    public void SetProperties()
    {
        this.PrfStance = new StanceHandler();

        if (hilt != null)
        {
            this.EquippableMain = hilt.MainHanded;
            this.EquippableOff = hilt.OffHanded;

            this.PrfStance.ApplyMoveset(hilt.PrfStance);
        }

        if (blade != null)
        {
            this.PrfStance.ApplyMoveset(blade.PrfStance);
        }

        if (adornment != null)
        {
            this.PrfStance.ApplyMoveset(adornment.PrfStance);
        }

        if (GetTotalLength() > 2 || GetTotalWeight() > 3)
        {
            this.MainHandEquipSlot = Inventory.EquipSlot.rBack;
            this.OffHandEquipSlot = Inventory.EquipSlot.lBack;
        }
        else
        {
            this.MainHandEquipSlot = Inventory.EquipSlot.rHip;
            this.OffHandEquipSlot = Inventory.EquipSlot.lHip;
        }
        this.elementRatios = new Damage();
        /*
        int leadSP = -9;
        foreach (WeaponComponent component in GetAllComponents())
        {
            if (component != null)
            {
                if (component.ratios != null)
                {
                    elementRatios.Add(component.ratios);
                }
                if (component.PrfMainHandStance.specialAttack != null && component.PrfMainHandStance.specialPriority > leadSP)
                {
                    this.PrfStance.specialAttack = component.PrfMainHandStance.specialAttack;
                    leadSP = component.PrfMainHandStance.specialPriority;
                }
            } 
        }
        */
    }

    public List<WeaponComponent> GetAllComponents()
    {
        List<WeaponComponent> comps = new List<WeaponComponent>();
        if (hilt != null)
        {
            comps.Add(hilt);
            comps.AddRange(hilt.insets);
        }
        if (blade != null)
        {
            comps.Add(blade);
            comps.AddRange(blade.insets);
        }
        if (adornment != null)
        {
            comps.Add(adornment);
        }
        return comps;
    }

    #region rendering
    public override GameObject GenerateModel()
    {
        if (model != null)
        {
            DestroyModel();
        }

        if (prefab != null)
        {
            model = GameObject.Instantiate(prefab);
        }
        else
        {
            // generate model from components
            model = new GameObject("parent: " + itemName);

            GameObject hiltModel = null;
            GameObject bladeModel = null;
            if (hilt != null)
            {
                hiltModel = hilt.GenerateModel();
            }
            if (blade != null)
            {
                bladeModel = blade.GenerateModel();
            }
            if (hiltModel != null)
            {
                hiltModel.transform.SetParent(model.transform);
            }
            if (bladeModel != null)
            {
                bladeModel.transform.SetParent(model.transform);
                if (hiltModel != null)
                {
                    bladeModel.transform.position = hiltModel.transform.Find("_mount").position;
                }
            }
        }

        return model;
    }

    public override GameObject GetModel()
    {
        return model;
    }

    public override void DestroyModel()
    {
        if (model != null)
        {
            GameObject.Destroy(model);
        }
        model = null;
    }
    #endregion

    public string ToString()
    {
        string bladeTxt = "none equipped";
        if (blade != null)
        {
            bladeTxt = blade.ToString();
        }
        string hiltTxt = "none equipped";
        if (hilt != null)
        {
            hiltTxt = hilt.ToString();
        }
        string adornTxt = "none equipped";
        if (adornment != null)
        {
            adornTxt = adornment.ToString();
        }
        return String.Format("{0}:" +
            "\n---Weight:{1}" +
            "\n---Length:{2}" +
            "\n---Width:{3}" +
            "\n---Balance:{4}" +
            "\n---Blade:{5}" +
            "\n---Hilt:{6}" +
            "\n---Adornment:{7}" +
            "\n---Pierce:{8} / Slash:{9}" +
            "\n---AttackSpeed:{10} / {11}", itemName, this.GetTotalWeight(), this.GetTotalLength(), this.GetWidth(), this.GetBalance(), bladeTxt, hiltTxt, adornTxt, GetPiercingModifier(), GetSlashingModifier(), GetAttackSpeed(false), GetAttackSpeed(true));
    }
}
