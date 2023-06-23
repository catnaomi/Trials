using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorSetWorld : MonoBehaviour
{

    [SerializeField] World world;
    enum World
    {
        Default,
        World1,
        World2
    }

    private void Awake()
    {
        SetRenderersToWorld();
    }

    public void SetRenderersToWorld()
    {
        string terrainL = "Terrain";
        string defaultL = "Default";
        string actorL = "Actors";
        if (world == World.World1)
        {
            terrainL = "Terrain_World1Only";
            actorL = "Actors_World1Only";
            defaultL = "World1Only";
        }
        else if (world == World.World2)
        {
            terrainL = "Terrain_World2Only";
            actorL = "Actors_World2Only";
            defaultL = "World2Only";
        }
        foreach (Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            if (LayerMask.LayerToName(r.gameObject.layer).ToLower().Contains("interactionnode"))
            {
                continue;
            }
            else if (LayerMask.LayerToName(r.gameObject.layer).ToLower().Contains("terrain"))
            {
                r.gameObject.layer = LayerMask.NameToLayer(terrainL);
            }
            else if (LayerMask.LayerToName(r.gameObject.layer).ToLower().Contains("actor"))
            {
                r.gameObject.layer = LayerMask.NameToLayer(actorL);
            }
            else
            {
                r.gameObject.layer = LayerMask.NameToLayer(defaultL);
            }
        }
    }
}
