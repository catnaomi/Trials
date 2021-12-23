using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Animancer;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Stances & Movesets/New Moveset", order = 1)]
public class Moveset : ScriptableObject
{
    [Header("Main Hand Attacks")]
    public InputAttack quickSlash1h;
    public InputAttack strongSlash1h;
    public InputAttack quickThrust1h;
    public InputAttack strongThrust1h;
    [Header("Stance Attacks")]
    public InputAttack stanceSlash;
    public InputAttack stanceThrust;
    [Header("Aim Attack")]
    public AimAttack aimAttack;

    [Header("Movement-Based Attacks")]
    public InputAttack dashSlash;
    public InputAttack dashThrust;
    public InputAttack plungeSlash;
    public InputAttack plungeThrust;
    public InputAttack rollSlash;
    public InputAttack rollThrust;
    [Header("Animancer")]
    public ClipTransition draw;
    public ClipTransition sheathe;
    public MixerTransition2DAsset moveAnim;
    public bool overridesBlock;
    public MixerTransition2DAsset blockMove;
    public ClipTransition blockAnim;
    public ClipTransition blockAnimStart;
    public bool overridesSprint;
    public ClipTransition sprintAnim;

    public static Dictionary<string, Moveset> movesetTable;
    public static bool movesetsLoaded = false;
    public static void LoadMovesetCombinations()
    {
        Debug.Log("loading moveset combinations");

        movesetTable = new Dictionary<string, Moveset>();
        try
        {
            string path = "data/moveset_combinations";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            string text = textAsset.text;

            string[] rows = text.Split('\n');

            movesetTable["default"] = Resources.Load<Moveset>("Stances/movesets/moveset_default");

            foreach (string row in rows)
            {
                string[] split = row.Split(',');

                string key = GetMovesetKey(split[0], split[1]);
                Moveset moveset = Resources.Load<Moveset>("Stances/movesets/moveset_" + split[2]);
                if (moveset != null)
                {
                    movesetTable[key] = moveset;
                }
                else
                {
                    movesetTable[key] = movesetTable["default"];
                }
            }
            movesetsLoaded = true;
            Debug.Log("moveset loading successful.");
        }
        catch (Exception ex)
        {
            Debug.LogError("failed to load moveset");
            Debug.LogError(ex);
        }
    }

    public static Moveset GetMoveset(string hiltType, string bladeType)
    {
        if (!movesetsLoaded) LoadMovesetCombinations();

        string key = GetMovesetKey(hiltType, bladeType);

        if (movesetTable.TryGetValue(key, out Moveset moveset))
        {
            return moveset;
        }
        else if (movesetTable.TryGetValue("default", out Moveset def))
        {
            return def;
        }
        else
        {
            Moveset m = ScriptableObject.CreateInstance<Moveset>();
            m.name = "Empty Moveset";
            return m;
        }
    }

    private static string GetMovesetKey(string hiltType, string bladeType)
    {
        return hiltType.Replace(' ','-').ToLower() + "+" + bladeType.Replace(' ', '-').ToLower();
    }
}
