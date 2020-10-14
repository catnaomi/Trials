using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomUtilities;
using System;

public class ActionsLibrary {
    /*
     * 0: no actions
     * 1-10: basic weapon swings
     * 11-24: dodges & movement
     * 33-48: special actions
     * 49-64: unused
     * 65-128: item animations
     * 129-256: blade attacks
     */
    private static List<InputAction> library = new List<InputAction>
    {
        new InputAction()
        {
            id = 0,
            animId = 0,
            desc = "None",
        },
        new InputAction()
        {
            id = 1,
            animId = 1,
            desc = "Player Light Attack",
        },
        new InputAction()
        {
            id = 2,
            animId = 2,
            desc = "Player Heavy Attack",
        },
        /*
        new InputAction()
        {
            id = 3,
            animId = 3,
            desc = "Plunge Attack",
        },
        new InputAction()
        {
            id = 4,
            animId = 4,
            desc = "Player Charge Slashes",
        },
        */
        new InputAction()
        {
            id = 5,
            animId = 5,
            desc = "2h Combo 1",
        },
        new InputAction()
        {
            id = 6,
            animId = 6,
            desc = "2h Heavy Strike",
        },
        new InputAction()
        {
            id = 7,
            animId = 7,
            desc = "2h Riposte",
        },
        new InputAction()
        {
            id = 8,
            animId = 8,
            desc = "Jumping Heavy Slash",
        },
        new InputAction()
        {
            id = 10,
            animId = 10,
            desc = "Parry",
        },
        new InputAction()
        {
            id = 11,
            animId = 11,
            desc = "Jump Forwards",
        },
        new InputAction()
        {
            id = 12,
            animId = 12,
            desc = "Jump Backwards",
        },
        new InputAction()
        {
            id = 13,
            animId = 13,
            desc = "Jump Left",
        },
        new InputAction()
        {
            id = 14,
            animId = 14,
            desc = "Jump Right",
        },
        new InputAction()
        {
            id = 15,
            animId = 15,
            desc = "Roll",
        },
        new InputAction()
        {
            id = 16,
            animId = 16,
            desc = "Backflip",
        },
        new InputAction()
        {
            id = 17,
            animId = 17,
            desc = "Player Sprint",
        },
        new InputAction()
        {
            id = 33,
            animId = 33,
            desc = "Draw Weapon",
        },
        new InputAction()
        {
            id = 34,
            animId = 34,
            desc = "Sheathe Weapon",
        },
        new InputAction()
        {
            id = 35,
            animId = 35,
            desc = "Slay",
        },
        new InputAction()
        {
            id = 36,
            animId = 36,
            desc = "Light Stagger",
        },
        new InputAction()
        {
            id = 37,
            animId = 37,
            desc = "Heavy Stagger",
        },
        new InputAction()
        {
            id = 39,
            animId = 39,
            desc = "Standing Collapse",
        },
        new InputAction()
        {
            id = 40,
            animId = 40,
            desc = "Kneel",
        },
        new InputAction()
        {
            id = 41,
            animId = 41,
            desc = "Kneel End",
        },
        new InputAction()
        {
            id = 65,
            animId = 65,
            desc = "Shield Bash",
        },
        new InputAction()
        {
            id = 66,
            animId = 66,
            desc = "Bow",
        },
        new InputAttack()
        {
            id = 129,
            animId = 129,
            attackId = 129,
            desc = "Short Thrust",
        },
        new InputAttack()
        {
            id = 130,
            animId = 130,
            attackId = 130,
            desc = "Combo 3 Slash",
        },
        new InputAttack()
        {
            id = 131,
            animId = 131,
            attackId = 131,
            desc = "Lunge",
        },
        new InputAttack()
        {
            id = 132,
            animId = 132,
            attackId = 132,
            desc = "Circle Slash",
        },
        new InputAttack()
        {
            id = 135,
            animId = -1,
            attackId = 135,
            desc = "Plunge Stab",
        },
        new InputAttack
        {
            id = 136,
            animId = -1,
            attackId = 136,
            desc = "Plunge Slash",
        },
        new InputAttack()
        {
            id = 137,
            animId = -1,
            attackId = 137,
            desc = "Jump Slash",
        },
        new InputAttack()
        {
            id = 138,
            animId = -1,
            attackId = 138,
            desc = "Slide Lunge",
        },
        new InputAttack()
        {
            id = 139,
            animId = 139,
            attackId = 139,
            desc = "Walk Slash R",
        },  
        new InputAttack()
        {
            id = 140,
            animId = 140,
            attackId = 140,
            desc = "Walk Slash L",
        },
        new InputAttack()
        {
            id = 141,
            animId = 141,
            attackId = 141,
            desc = "Spin Slash",
        },
    };

    private static bool initialized;
    private static Dictionary<string, InputAction> cacheByDesc;
    private static Dictionary<int, InputAction> cacheById;

    public static void Initialize()
    {
        if (initialized) return;

        int count = 0;
        cacheByDesc = new Dictionary<string, InputAction>();
        cacheById = new Dictionary<int, InputAction>();

        foreach (InputAction InputAction in library)
        {
            cacheByDesc[InputAction.desc] = InputAction;
            cacheById[InputAction.id] = InputAction;
            count++;
        }

        initialized = true;
        Debug.Log(count + " InputAction(s) initialized");
    }

    public static InputAction GetInputAction(string name, bool exact)
    {
        Initialize();

        if (exact)
        {
            return cacheByDesc[name];
        }
        else
        {
            return GetInputAction(name);
        }
    }

    public static InputAction GetInputAction(string name)
    {
        Initialize();

        string found = InterfaceUtilities.GetClosestString(name, cacheByDesc.Keys);
        try
        {
            return cacheByDesc[found];
        }
        catch (Exception ex)
        {
            return library[0];
        }
    }

    public static InputAction GetInputAction(int id)
    {
        Initialize();

        return cacheById[id];
    }

    public static bool TryGetInputAttack(string name, out InputAttack attack)
    {
        attack = null;
        Initialize();

        if (cacheByDesc.TryGetValue(name, out InputAction action))
        {
            if (action is InputAttack)
            {
                attack = (InputAttack)action;
                return true;
            }
        }
        return false;
    }

    public static string[] GetAllDescriptions()
    {
        Initialize();

        string[] descs = new string[library.Count];
        
        for (int i = 0; i < library.Count; i++)
        {
            descs[i] = library[i].desc;
        }

        return descs;
    }


    public enum BlockType
    {
        Sword,
        Shield
    }

    public static BlockType GetDefaultBlockType()
    {
        return BlockType.Sword;
    }

    public enum AimType
    {
        Bow
    }
}