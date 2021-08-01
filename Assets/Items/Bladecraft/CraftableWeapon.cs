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
    // base damage
    // type/moveset TODO
    // attack speed (from EquippableWeapon)
    // length (below)
    // weight (below)
    // width (below)
    // balance (below)
    // slash damage (below)
    // thrust damage (below)
    // durability
    // elements
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
        float b = 0f;
        float h = 0f;
        if (blade != null)
        {
            b = blade.GetWidth();
        }
        if (hilt != null)
        {
            h = hilt.GetWidth();
        }
        return Mathf.Max(b, h);
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
        }
        if (hilt != null)
        {
            weight += hilt.GetWeight();
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
            return blade.GetPiercingModifier();
        }
        return 0f;
    }

    public override float GetSlashingModifier()
    {
        if (blade != null)
        {
            return blade.GetSlashingModifier();
        }
        return 0f;
    }

    public override int GetDurability()
    {
        List<int> durabilities = new List<int>();
        List<WeaponComponent> components = this.GetAllComponents();
        foreach (WeaponComponent component in components)
        {
            if (component.durability != -999)
            {
                durabilities.Add(component.durability);
            }
        }
        return Mathf.Min(durabilities.ToArray());
    }
    public override float GetAttackSpeed(bool twoHand)
    {
        return AttackSpeedFormula(GetTotalWeight(), GetBalance(), twoHand);
    }

    public static float AttackSpeedFormula(float weight, float balance, bool twoHand)
    {
        float WEIGHT_1H_OFFSET = 3f;
        float WEIGHT_2H_OFFSET = 5f;
        float WEIGHT_MOD = 0.1f;

        float BALANCE_MODIFIER = 0.2f;

        //float weightOffset = (twoHand ? WEIGHT_2H_OFFSET : WEIGHT_1H_OFFSET) - GetTotalWeight();
        float weightOffset = (twoHand ? WEIGHT_2H_OFFSET : WEIGHT_1H_OFFSET) - weight;
        weightOffset *= WEIGHT_MOD;

        float balanceOffset = -balance * BALANCE_MODIFIER;

        return 1f + weightOffset + balanceOffset;
    }
    #endregion

    public void SetProperties()
    {
        this.stance = new StanceHandler();

        if (hilt != null)
        {
            this.EquippableMain = hilt.MainHanded;
            this.EquippableOff = hilt.OffHanded;

            //this.stance.ApplyMoveset(hilt.PrfStance);
        }

        if (blade != null)
        {
            //this.stance.ApplyMoveset(blade.PrfStance);
        }

        if (adornment != null)
        {
            //this.stance.ApplyMoveset(adornment.PrfStance);
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
        // this.elements = something or other;
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
            //comps.AddRange(hilt.insets); // commented out to avoid null referenced components
            foreach(Inset inset in hilt.insets)
            {
                if (inset != null)
                {
                    comps.Add(inset);
                }
            }
        }
        if (blade != null)
        {
            comps.Add(blade);
            //comps.AddRange(blade.insets); // commented out to avoid null referenced components
            foreach (Inset inset in blade.insets)
            {
                if (inset != null)
                {
                    comps.Add(inset);
                }
            }
        }
        if (adornment != null)
        {
            comps.Add(adornment);
        }
        return comps;
    }



    public void GetStatDifferencesHiltChange(Hilt proposedHilt, ref WeaponStatBlock statBlock)
    {
        float weight_nohilt = 0;
        float width_nohilt = 0;
        float length_nohilt = 0;
        float bladeWeight = 0f;
        if (blade != null)
        {
            bladeWeight = blade.GetWeight();
            weight_nohilt += blade.GetWeight();
            width_nohilt = Mathf.Max(blade.width, width_nohilt);
            length_nohilt += blade.GetLength();
        }
        if (adornment != null)
        {
            weight_nohilt += adornment.GetWeight();
        }
        statBlock.stat_Weight.comparisonValue = weight_nohilt + proposedHilt.GetWeight();
        statBlock.stat_Length.comparisonValue = length_nohilt + proposedHilt.GetLength();
        statBlock.stat_Width.comparisonValue = Mathf.Max(width_nohilt, proposedHilt.GetWidth());

        statBlock.stat_Balance.comparisonValue = bladeWeight - proposedHilt.GetWeight();
        statBlock.stat_AttackSpeed.comparisonValue = AttackSpeedFormula(weight_nohilt + proposedHilt.GetWeight(), bladeWeight - proposedHilt.GetWeight(), false);
        statBlock.stat_Durability.comparisonValue = Mathf.Max(proposedHilt.durability, this.GetDurability());
        

        statBlock.stat_Weight.compare = true;
        statBlock.stat_Length.compare = true;
        statBlock.stat_Width.compare = true;
        statBlock.stat_Balance.compare = true;
        statBlock.stat_AttackSpeed.compare = true;
        statBlock.stat_Durability.compare = true;
    }

    public void GetStatDifferencesBladeChange(Blade proposedBlade, ref WeaponStatBlock statBlock)
    {
        float weight_noblade = 0;
        float width_noblade = 0;
        float length_noblade = 0;
        float hiltWeight = 0f;
        if (hilt != null)
        {
            hiltWeight = hilt.GetWeight();
            weight_noblade += hilt.GetWeight();
            width_noblade = Mathf.Max(hilt.width, width_noblade);
            length_noblade += hilt.GetLength();
        }
        if (adornment != null)
        {
            weight_noblade += adornment.GetWeight();
        }
        statBlock.stat_Weight.comparisonValue = weight_noblade + proposedBlade.GetWeight();
        statBlock.stat_Length.comparisonValue = length_noblade + proposedBlade.GetLength();
        statBlock.stat_Width.comparisonValue = Mathf.Max(width_noblade, proposedBlade.GetWidth());

        statBlock.stat_Balance.comparisonValue = proposedBlade.GetWeight() - hiltWeight;

        statBlock.stat_AttackSpeed.comparisonValue = AttackSpeedFormula(weight_noblade + proposedBlade.GetWeight(), hiltWeight - proposedBlade.GetWeight(), false);

        statBlock.stat_Durability.comparisonValue = Mathf.Max(proposedBlade.durability, this.GetDurability());

        statBlock.stat_BaseDamage.comparisonValue = proposedBlade.GetBaseDamage();
        statBlock.stat_PierceMod.comparisonValue = proposedBlade.GetPiercingModifier();
        statBlock.stat_SlashMod.comparisonValue = proposedBlade.GetSlashingModifier();

        statBlock.stat_Weight.compare = true;
        statBlock.stat_Length.compare = true;
        statBlock.stat_Width.compare = true;
        statBlock.stat_Balance.compare = true;
        statBlock.stat_AttackSpeed.compare = true;
        statBlock.stat_Durability.compare = true;
        statBlock.stat_BaseDamage.compare = true;
        statBlock.stat_PierceMod.compare = true;
        statBlock.stat_SlashMod.compare = true;
    }

    public void GetStatDifferencesAdornmentChange(Adornment proposedAdornment, ref WeaponStatBlock statBlock)
    {
        float weight_noadorn = 0;
        if (hilt != null)
        {
            weight_noadorn += hilt.GetWeight();
        }
        if (blade != null)
        {
            weight_noadorn += blade.GetWeight();
        }
        statBlock.stat_Weight.comparisonValue = weight_noadorn + proposedAdornment.GetWeight();
        statBlock.stat_Weight.compare = true;
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
