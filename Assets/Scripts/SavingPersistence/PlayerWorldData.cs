using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerWorldData
{
    public string activeScene;
    public float[] position;
    public float[] rotation;
    public bool inWorld2;

    public void GetWorldData(PortalManager portal, PlayerActor player)
    {
        inWorld2 = portal.inWorld2;
        position = new float[] { player.transform.position.x , player.transform.position.y, player.transform.position.z };
        rotation = new float[] { player.transform.rotation.x, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w };
        activeScene = SceneManager.GetActiveScene().name;
    }
}
