using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StanceHandler
{
    public static int BaseLayer = 0;
    public static int ActionLayer = 6;
    //public static int ImpactLayer = 7;
    public static int AILayer = 8;

    public static bool ResourcesLoaded = false;
    private static Dictionary<AnimatorStance, AnimatorOverrideController> stanceDict;

    //public AnimatorStance animatorStance; // TODO: remove
    //public LightThrustStyle lightThrustStyle; // TODO: remove
    //public LightSlashStyle lightSlashStyle; // TODO: remove
    public GripStyle rightHandStance;
    public GripStyle leftHandStance;
    public GripStyle twoHandStance;
    public BlockStyle blockStyle;
    [Space(10)]
    public Moveset moveset;
    //[Space(10)]
    //public HeavyAttack heavyAttack; // TODO: remove, adapt to new attack system
    //[Space(5)]
    //public HeavyAttack specialAttack; // TODO: remove
    //public int specialPriority;

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

    public enum GripStyle
    {
        Unarmed,            // 0 - hands relaxed.
        OneHand_Light,      // 1 - blade out and down
        TwoHand_Light,      // 2 - blade out in front
        Shield,             // 3
        Bow                 // 4
        // TODO: add heavy (over shoulder) variations, and polearm variations
    }

    public enum BlockStyle
    {
        None,         // 0
        MainHeavy,    // 1
        MainLight,    // 2
        Shield,       // 3
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
        this.leftHandStance = original.leftHandStance;
        this.rightHandStance = original.rightHandStance;
        this.moveset = original.moveset;
    }
    public StanceHandler() {
    }
    public void MergeStance(StanceHandler main, StanceHandler off)
    {
        bool twoHanding = (off == null);

        this.rightHandStance = GripStyle.Unarmed;
        this.leftHandStance = GripStyle.Unarmed;
        this.twoHandStance = GripStyle.Unarmed;

        if (main != null)
        {

            this.moveset.mainQuick = main.moveset.mainQuick;
            this.moveset.mainStrong = main.moveset.mainStrong;

            this.moveset.twoQuick = main.moveset.twoQuick;
            this.moveset.twoStrong = main.moveset.twoStrong;

            
            this.rightHandStance = main.rightHandStance;

            this.twoHandStance = main.twoHandStance;
        }
        if (off != null)
        {
            this.moveset.offQuick = off.moveset.offQuick;
            this.moveset.offStrong = off.moveset.offStrong;
            this.leftHandStance = off.leftHandStance;
        }
    }

    public void ApplyMoveset(StanceHandler newStance)
    {
        foreach (Moveset.AttackStyle style in Enum.GetValues(typeof(Moveset.AttackStyle)))
        {
            InputAttack atk = newStance.moveset.GetAttackFromInput(style);
            if (atk != null)
            {
                this.moveset.SetAttackFromInput(style, atk);
            }
        }
    }

    /*
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
    */

    public BlockStyle GetBlockStyle()
    {
        return this.blockStyle;
    }

    /*
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
    */

    public bool BlockWithMain()
    {
        // TODO: implement this based on block style
        return true;
    }

    /*
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
    */
}
