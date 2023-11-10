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


    public static bool IsTerrain(int layer)
    {
        return (layer & Terrain) != 0;
    }

    public static bool IsTerrain(GameObject obj)
    {
        return IsTerrain(obj.layer);
    }

    public static bool IsTerrain(Collider c)
    {
        return IsTerrain(c.gameObject);
    }
}
