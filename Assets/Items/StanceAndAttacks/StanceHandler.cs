using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StanceHandler
{
    public StanceStyle stanceStyle;
    public GripStyle rightGrip;
    public GripStyle leftGrip;
    public GripStyle twohandGrip;
    public BlockStyle blockStyle;

    public enum StanceStyle // determines idle stance and walk/run anims
    {
        None,//  0:
        UnarmedCombat, //  1:
        Axe_MainF,          //  2: axe standing idle
        Axe_OffF,           //  3: axe standing idle
        Bow_OffF,          //  4: bow standing idle
        Bow_MainF,           //  5: bow standing idle
        Greatsword_MainF,   //  6: greatsword idle
        Greatsword_OffF,    //  7: greatsword idle
        Shield_MainF,       //  8: shield idle 
        Shield_OffF,        //  9: shield idle
        TwoHandSide_MainF,  //  10: 2hand sword 
        TwoHandSide_OffF,   //  11:2hand sword
        Shoulder_MainF,     //  12:
        Shoulder_OffF,      //  13: 
        Casual_OffF,        // 14
    }

    public enum GripStyle
    {
        None,            // 0 - hands relaxed.
        Unarmed,     // 1
        Axe,                // 2
        Axe_Mirror,         // 3
        Bow,                // 4
        Bow_Mirror,         // 5
        Greatsword,         // 6
        Greatsword_Mirror,  // 7
        Shield,             // 8
        Shield_Mirror,      // 9
        TwoHandSide,        // 10
        TwoHandSide_Mirror, // 11
        Shoulder,           // 12
        Shoulder_Mirror,    // 13
        Casual,             // 14
        Casual_Mirror,      // 15
        TwoHandPoise,       // 16
        // TODO: add heavy (over shoulder) variations, and polearm variations
    }

    public enum BlockStyle
    {
        None,         // 0
        Bracing,      // 1
        Shield,       // 2
        Light_2H,     // 3
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
        //this.leftHandStance = original.leftHandStance;
        //this.rightHandStance = original.rightHandStance;
        //this.moveset = original.moveset;
    }
    public StanceHandler() {
    }
    public void Merge(StanceHandler stance, bool originalPriority)
    {
        if (stance == null) return;
        if (stance.stanceStyle != StanceStyle.None)
        {
            if (!originalPriority || this.stanceStyle == StanceStyle.None)
            {
                this.stanceStyle = stance.stanceStyle;
            }
        }
        if (stance.rightGrip != GripStyle.None)
        {
            if (!originalPriority || this.rightGrip == GripStyle.None)
            {
                this.rightGrip = stance.rightGrip;
            }
        }
        if (stance.leftGrip != GripStyle.None)
        {
            if (!originalPriority || this.leftGrip == GripStyle.None)
            {
                this.leftGrip = stance.leftGrip;
            }
        }
        if (stance.twohandGrip != GripStyle.None)
        {
            if (!originalPriority || this.twohandGrip == GripStyle.None)
            {
                this.twohandGrip = stance.twohandGrip;
            }
        }
        if (stance.blockStyle != BlockStyle.None)
        {
            if (!originalPriority || this.blockStyle == BlockStyle.None)
            {
                this.blockStyle = stance.blockStyle;
            }
        }
    }
}
