using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskReference : MonoBehaviour
{
    public LayerMask TerrainMask;
    public LayerMask ActorMask;
    public static LayerMask Terrain;
    public static LayerMask Actors;

    private void Awake()
    {
        Terrain = TerrainMask;
        Actors = ActorMask;
    }

}
