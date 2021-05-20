using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public int count;
    public bool spawn;
    // Update is called once per frame
    void Update()
    {
        if (spawn)
        {
            GameObject.Instantiate(prefabs[count % prefabs.Length], this.transform.position, Quaternion.identity);
            count++;
            spawn = false;
        }
    }
}
