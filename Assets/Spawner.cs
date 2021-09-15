using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public int count;
    public bool spawn;
    public bool spawnOnDeath;
    GameObject last;
    // Update is called once per frame
    void Update()
    {
        if (spawn)
        {
            if (last != null && last.TryGetComponent<HumanoidActor>(out HumanoidActor lactor))
            {
                lactor.OnDie.RemoveListener(OnDeath);
            }
            last = GameObject.Instantiate(prefabs[count % prefabs.Length], this.transform.position, Quaternion.identity);
            if (spawnOnDeath && last.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
            {
                actor.OnDie.AddListener(OnDeath);
            }
            count++;
            spawn = false;
        }
    }

    public void OnDeath()
    {
        Debug.Log("die");
        spawn = true;
    }
}
