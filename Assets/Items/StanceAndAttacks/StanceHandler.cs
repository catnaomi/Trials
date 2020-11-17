using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StanceHandler
{
    public static int BaseLayer = 0;
    public static int ActionLayer = 3;
    //public static int ImpactLayer = 7;
    public static int AILayer = 8;

    public static bool ResourcesLoaded = false;
    private static Dictionary<AnimatorStance, AnimatorOverrideController> stanceDict;

    public AnimatorStance animatorStance;
    public LightThrustStyle lightThrustStyle;
    public LightSlashStyle lightSlashStyle;
    [Space(10)]
    public HeavyAttack heavyAttack;
    [Space(5)]
    public HeavyAttack specialAttack;
    public int specialPriority;

    public enum AnimatorStance
    {
        None,           // 0
        OneHand_Front,  // 1
        TwoHand,        // 2
        OneHand_Shield, // 3
        OneHand_Rear,   // 4
        Bow,            // 5
        Dual            // 6
    }

    public enum LightThrustStyle
    {
        None,       // 0
        RapierLunge_1H,   // 1
        Stab_2H,    // 2
        BackThrust_1H, // 3
        AlternatingThrust // 4
    }

    public enum LightSlashStyle
    {
        None,           // 0
        ComboSlash_1H,  // 1
        WitchSlash_2H,  // 2
        AgileSlash_2H,  // 3
        DualCombo,      // 4
        Highlander_2H,  // 5
        SteadyCombo_1H, // 6
        SteadyCombo_2H  // 7
    }
    public enum BlockStyle
    {
        None,         // 0
        OneHand,      // 1
        TwoHand,      // 2
        Shield,       // 3
        TwoHand_Shaft,// 4
    }

    public enum ArmedStyle
    {
        None,           // 0
        OneHand_Front,  // 1
        TwoHand,        // 2
        OneHand_Rear,   // 3
        Bow             // 4
    }

    public enum HeavyStyle
    {
        None,           // 0
        Single,         // 1
        Charge_Release, // 2
        Aim             // 3
    }
    public StanceHandler(StanceHandler original)
    {
        this.animatorStance = original.animatorStance;
        this.lightSlashStyle = original.lightSlashStyle;
        this.lightThrustStyle = original.lightThrustStyle;
        this.heavyAttack = original.heavyAttack;
        this.specialAttack = original.specialAttack;
        this.specialPriority = original.specialPriority;
    }
    public StanceHandler() {
    }
    public static StanceHandler MergeStances(StanceHandler lowPriority, StanceHandler highPriority)
    {
        StanceHandler stance = new StanceHandler(lowPriority);

        if ((int)highPriority.animatorStance != 0)
        {
            stance.animatorStance = highPriority.animatorStance;
        }

        if ((int)highPriority.lightSlashStyle != 0)
        {
            stance.lightSlashStyle = highPriority.lightSlashStyle;
        }

        if ((int)highPriority.lightThrustStyle != 0)
        {
            stance.lightThrustStyle = highPriority.lightThrustStyle;
        }

        if (highPriority.heavyAttack != null)
        {
            stance.heavyAttack = highPriority.heavyAttack;
        }

        if (highPriority.specialAttack != null)
        {
            stance.specialAttack = highPriority.specialAttack;
        }

        return stance;
    }

    public AnimatorOverrideController GetController()
    {
        if (!ResourcesLoaded)
        {
            InitResources();
        }
        AnimatorOverrideController animatorOverrideController = AnimatorOverrideController.Instantiate(stanceDict[animatorStance]);
        if (heavyAttack != null)
        {
            heavyAttack.SetOverrides(animatorOverrideController);
        }
        if (specialAttack != null)
        {
            specialAttack.SetOverrides(animatorOverrideController);
        }
        return animatorOverrideController;
    }
    
    public static void InitResources()
    {
        stanceDict = new Dictionary<AnimatorStance, AnimatorOverrideController>();

        stanceDict[AnimatorStance.OneHand_Front] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_1H-front");
        stanceDict[AnimatorStance.OneHand_Rear] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_1H-front");
        stanceDict[AnimatorStance.None] = stanceDict[AnimatorStance.OneHand_Front];
        stanceDict[AnimatorStance.TwoHand] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_2H");
        stanceDict[AnimatorStance.OneHand_Shield] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_1H-shield");
        stanceDict[AnimatorStance.Bow] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_bow");
        stanceDict[AnimatorStance.Dual] = Resources.Load<AnimatorOverrideController>("Stances/stance_controller_1H-front");
        ResourcesLoaded = true;
    }

    public LightThrustStyle GetLightThrustStyle()
    {
        return this.lightThrustStyle;
        switch (animatorStance)
        {
            case AnimatorStance.OneHand_Front:
                return LightThrustStyle.RapierLunge_1H;

            case AnimatorStance.OneHand_Shield:
                return LightThrustStyle.RapierLunge_1H;

            case AnimatorStance.OneHand_Rear:
                return LightThrustStyle.RapierLunge_1H;

            case AnimatorStance.TwoHand:
                return LightThrustStyle.Stab_2H;

            default:
                return LightThrustStyle.RapierLunge_1H;
        }
    }

    public LightSlashStyle GetLightSlashStyle()
    {
        return this.lightSlashStyle;
        switch (animatorStance)
        {
            case AnimatorStance.OneHand_Front:
                return LightSlashStyle.ComboSlash_1H;

            case AnimatorStance.OneHand_Shield:
                return LightSlashStyle.ComboSlash_1H;

            case AnimatorStance.OneHand_Rear:
                return LightSlashStyle.ComboSlash_1H;

            case AnimatorStance.TwoHand:
                return LightSlashStyle.WitchSlash_2H;

            default:
                return LightSlashStyle.ComboSlash_1H;
        }
    }

    public BlockStyle GetBlockStyle()
    {
        switch (animatorStance)
        {
            case AnimatorStance.OneHand_Front:
                return BlockStyle.OneHand;

            case AnimatorStance.OneHand_Shield:
                return BlockStyle.Shield;

            case AnimatorStance.OneHand_Rear:
                return BlockStyle.TwoHand;

            case AnimatorStance.TwoHand:
                return BlockStyle.TwoHand;

            case AnimatorStance.Bow:
                return BlockStyle.TwoHand_Shaft;

            default:
                return BlockStyle.TwoHand;
        }
    }

    public ArmedStyle GetArmedStyle()
    {
        switch (animatorStance)
        {
            case AnimatorStance.OneHand_Front:
                return ArmedStyle.OneHand_Front;

            case AnimatorStance.OneHand_Shield:
                return ArmedStyle.OneHand_Rear;

            case AnimatorStance.OneHand_Rear:
                return ArmedStyle.OneHand_Rear;

            case AnimatorStance.TwoHand:
                return ArmedStyle.TwoHand;

            case AnimatorStance.Bow:
                return ArmedStyle.Bow;

            default:
                return ArmedStyle.OneHand_Rear;
        }
    }

    public bool BlockWithMain()
    {
        switch (animatorStance)
        {
            case AnimatorStance.OneHand_Front:
                return true;

            case AnimatorStance.OneHand_Shield:
                return false;

            case AnimatorStance.OneHand_Rear:
                return true;

            case AnimatorStance.TwoHand:
                return true;

            case AnimatorStance.Bow:
                return true;

            case AnimatorStance.Dual:
                return false;

            default:
                return true;
        }
    }

    public void ApplyHeavyAttack(HumanoidActor actor)
    {
        if (heavyAttack != null)
        {
            heavyAttack.OnEquip(actor);
        }
    }

    public void RemoveHeavyAttack(HumanoidActor actor)
    {
        if (heavyAttack != null)
        {
            heavyAttack.OnUnequip(actor);
        }
    }

    public HeavyStyle GetHeavyStyle()
    {
        if (heavyAttack != null)
        {
            return heavyAttack.GetHeavyStyle();
        }
        return HeavyStyle.None;
    }

    public bool ShouldHeavyBackStep()
    {
        if (heavyAttack != null)
        {
            return heavyAttack.shouldBackStep;
        }
        return false;
    }

    public void SetHeavyAttack(HeavyAttack heavyAttack)
    {
        if (!heavyAttack.isSpecialAttack)
        {
            this.heavyAttack = heavyAttack;
        }
    }

    public void ApplySpecialAttack(HumanoidActor actor)
    {
        if (specialAttack != null)
        {
            specialAttack.OnEquip(actor);
        }
    }

    public void RemoveSpecialAttack(HumanoidActor actor)
    {
        if (specialAttack != null)
        {
            specialAttack.OnUnequip(actor);
        }
    }

    public HeavyStyle GetSpecialStyle()
    {
        if (specialAttack != null)
        {
            return specialAttack.GetHeavyStyle();
        }
        return HeavyStyle.None;
    }

    public void SetSpecialAttack(HeavyAttack specialAttack)
    {
        if (specialAttack.isSpecialAttack)
        {
            this.specialAttack = specialAttack;
        }
    }
}
