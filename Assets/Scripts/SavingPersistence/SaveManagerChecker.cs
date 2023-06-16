using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManagerChecker : MonoBehaviour
{
    public GameObject saveManagerPrefab;
    // Start is called before the first frame update
    void Awake()
    {
        PlayerSaveDataManager loader = FindObjectOfType<PlayerSaveDataManager>();
        if (loader == null)
        {
            Instantiate(saveManagerPrefab);
        }
    }

    private void Start()
    {
        PlayerSaveDataManager loader = FindObjectOfType<PlayerSaveDataManager>();
        if (loader != null)
        {
            loader.OnSceneStart();
        }
    }
}
