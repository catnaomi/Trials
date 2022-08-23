using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskReference : MonoBehaviour
{
    public LayerMask TerrainMask;
    public static LayerMask Terrain;
    // Start is called before the first frame update
    void Start()
    {
        Terrain = TerrainMask;
    }

}
