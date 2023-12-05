using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositioner : MonoBehaviour
{
    static bool hasOverridePosition = false;
    static Vector3 overridePosition;
    static Quaternion overrideRotation;

    public Transform spawnPoint;
    bool hasSpawned;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    // Start is called before the first frame update
    void Start()
    {
        
        
        hasSpawned = false;
    }

    void Update()
    {
        if (!hasSpawned)
        {
            PlayerActor player = FindObjectOfType<PlayerActor>();
            if (player != null)
            {
                hasSpawned = true;
                if (!player.HasBeenSpawned())
                {
                    if (HasOverridePosition())
                    {
                        spawnPosition = overridePosition;
                        spawnRotation = overrideRotation;
                    }
                    else
                    {
                        spawnPosition = spawnPoint.position;
                        spawnRotation = spawnPoint.rotation;
                    }
                    SpawnPlayer(player);
                }
            }
        }
    }

    public void SpawnPlayer(PlayerActor player)
    {
        player.transform.rotation = spawnRotation;
        player.WarpTo(spawnPosition);
        player.SetNewSafePoint();
        player.SetSpawned();
    }

    bool HasOverridePosition()
    {
        bool over = hasOverridePosition;
        hasOverridePosition = false;
        return over;
    }

    // use this to load player at specific locations. used for loading saves.
    public static void SetNextOverridePosition(Vector3 position, Quaternion rotation)
    {
        overridePosition = position;
        overrideRotation = rotation;
        hasOverridePosition = true;
    }
}
