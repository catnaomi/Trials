using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public bool spawnOnDebug;
    public int count;
    // Update is called once per frame
    void Update()
    {
        if (spawnOnDebug && Input.GetButtonDown("Debug"))
        {
            GameObject.Instantiate(prefabs[count % prefabs.Length], this.transform.position, Quaternion.identity);
            count++;
        }
    }
}
