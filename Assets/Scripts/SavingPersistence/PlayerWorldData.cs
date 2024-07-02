using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerWorldData
{
    public string activeScene;
    public float[] position;
    public float[] rotation;
    public float[] cameraPosition;
    public float[] cameraRotation;
    public bool inWorld2;

    public void GetWorldData(PortalManager portal, PlayerActor player)
    {
        inWorld2 = portal.inWorld2;
        position = player.transform.position.toFloatArray();
        rotation = player.transform.rotation.toFloatArray();
        cameraPosition = Camera.main.transform.position.toFloatArray();
        cameraRotation = Camera.main.transform.rotation.toFloatArray();
        activeScene = SceneManager.GetActiveScene().name;
    }
}
