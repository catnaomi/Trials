using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PrefabPopulator : MonoBehaviour
{
    public string partialSearch;
    public GameObject prefab;
    public bool go;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (go)
        {
            go = false;
            var allObjs = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjs)
            {
                if (obj.name.Contains(partialSearch))
                {
                    GameObject spawned = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    spawned.transform.SetParent(obj.transform, false);
                }
            }
        }
    }
}
