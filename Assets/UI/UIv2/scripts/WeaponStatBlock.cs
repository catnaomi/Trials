using System.Collections;
using TMPro;
using UnityEngine;
public class WeaponStatBlock : MonoBehaviour
{
    public TMP_Text displayName;
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

    string weaponName;
    [Space(5)]
    public StatComparisonDisplay stat_BaseDamage;
    //StatBlockDisplay statTypeMoveset;
    public StatComparisonDisplay stat_AttackSpeed;
    public StatComparisonDisplay stat_Length;
    public StatComparisonDisplay stat_Weight;
    public StatComparisonDisplay stat_Width;
    public StatComparisonDisplay stat_Balance;
    public StatComparisonDisplay stat_SlashMod;
    public StatComparisonDisplay stat_PierceMod;
    public StatComparisonDisplay stat_Durability;
    // elements
    [Space(5)]
    public StatElementsDisplay stat_Elements;

    StatComparisonDisplay[] statComparisonDisplays;
    public void Awake()
    {
        statComparisonDisplays = new StatComparisonDisplay[] { stat_BaseDamage, stat_AttackSpeed, stat_Length, stat_Weight, stat_Width, stat_Balance, stat_SlashMod, stat_PierceMod, stat_Durability };
    }
    public void SetWeapon(BladeWeapon weapon)
    {
        weaponName = weapon.itemName;
        stat_BaseDamage.statValue = weapon.GetBaseDamage();
        stat_AttackSpeed.statValue = weapon.GetAttackSpeed(false);
        stat_Length.statValue = weapon.GetLength();
        stat_Weight.statValue = weapon.GetWeight();
        stat_Width.statValue = weapon.GetWidth();
        stat_Balance.statValue = weapon.GetBalance();
        stat_SlashMod.statValue = weapon.GetSlashingModifier();
        stat_PierceMod.statValue = weapon.GetPiercingModifier();
        stat_Durability.statValue = weapon.GetDurability();

        stat_Elements.SetElements(weapon.GetElements().ToArray());
        UpdateDisplay();
    }

    public void SetCompare(bool compare)
    {
        foreach (StatComparisonDisplay statComparisonDisplay in statComparisonDisplays)
        {
            statComparisonDisplay.compare = compare;
        }
    }

    public void UpdateDisplay()
    {
        displayName.text = weaponName;

        foreach (StatComparisonDisplay statComparisonDisplay in statComparisonDisplays)
        {
            statComparisonDisplay.gameObject.SetActive(true);
            statComparisonDisplay.UpdateDisplay();
        }
        stat_Elements.gameObject.SetActive(true);
        stat_Elements.GenerateElements();
    }

    public void Clear()
    {
        displayName.text = "";

        foreach (StatComparisonDisplay statComparisonDisplay in statComparisonDisplays)
        {
            statComparisonDisplay.gameObject.SetActive(false);
        }
        stat_Elements.gameObject.SetActive(false);
    }
}