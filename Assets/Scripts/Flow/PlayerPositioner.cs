using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositioner : MonoBehaviour
{
    public Transform spawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        
        PlayerActor player = FindObjectOfType<PlayerActor>();
        
        /*if (player != null && SceneLoader.ShouldRespawnPlayer() && this.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
        {
            SpawnPlayer(player);
        }*/
        if (!player.HasBeenSpawned())
        {
            SpawnPlayer(player);
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
