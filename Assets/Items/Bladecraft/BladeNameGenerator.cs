using UnityEngine;
using System.Collections;

public class BladeNameGenerator
{

    // [variation*] [material] [bladetype] [magic*]
    public static string GenerateName(CraftableWeapon weapon)
    {
        if (weapon.hilt == null && weapon.blade == null)
        {
            return "Incomplete Weapon";
        }
        else if (weapon.hilt == null)
        {
            return "Hiltless Blade";
        }
        else if (weapon.blade == null)
        {
            return "Empty Hilt";
        }

        string material = GetMaterialName(weapon);

        string variation = GetVariationName(weapon);

        string blade = weapon.blade.bladeDescriptor;

        return string.Format("{0}{1}{2}",variation,material,blade);
    }

    static string GetMaterialName(CraftableWeapon weapon)
    {
        return "Dbgium ";
    }

    static string GetVariationName(CraftableWeapon weapon)
    {
        string variation = "";

        if (weapon.GetBalance() >= 1)
        {
            variation = "Heavy";
        }
        else if (weapon.GetBalance() <= -1)
        {
            variation = "Unwieldy";
        }
        else if (weapon.GetTotalWeight() < weapon.hilt.weight + weapon.blade.weight)
        {
            variation = "Light";
        }

        if (variation != "")
        {
            variation += " ";
        }

        return variation;
    }
}
