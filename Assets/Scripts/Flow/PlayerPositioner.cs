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
        if (player != null && !SceneLoader.IsMovingPlayer())
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
    }
}
