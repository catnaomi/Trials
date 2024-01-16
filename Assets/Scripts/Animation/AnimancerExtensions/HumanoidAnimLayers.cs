using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HumanoidAnimLayers
{
    public const int Base = 0;
    public const int BilayerBlend = 1;
    public const int BlockBlend = 2; //used for typed blocks
    public const int Open2 = 3; // used by player for idle blends
    public const int UpperBody = 4;
    public const int Flinch = 5;
    public const int TimeEffects = 6;

    public static void InitLayers(AnimancerComponent animancer)
    {
        animancer.Layers.Capacity = 10;
    }
}
