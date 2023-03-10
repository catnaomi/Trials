using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositioner : MonoBehaviour
{
    public Transform spawnPoint;
    bool hasSpawned;
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
                    SpawnPlayer(player);
                }
            }
        }
    }

    public void SpawnPlayer(PlayerActor player)
    {
        player.transform.rotation = spawnPoint.rotation;
        player.WarpTo(spawnPoint.position);
        player.SetNewSafePoint();
        player.SetSpawned();
    }
}
