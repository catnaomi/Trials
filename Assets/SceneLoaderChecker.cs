using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderChecker : MonoBehaviour
{
    public GameObject sceneLoaderPrefab;
    // Start is called before the first frame update
    void Start()
    {
        SceneLoader loader = FindObjectOfType<SceneLoader>();
        if (loader == null)
        {
            Instantiate(sceneLoaderPrefab);
        }   
    }
}
