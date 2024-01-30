using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPickup : Pickup
{
    public Pickup[] pickups;
    int index = 0;
    public override void OnPickup()
    {
        pickups[index].OnPickup();
    }

    public override void OnStart()
    {
        base.OnStart();
        index = Random.Range(0, pickups.Length);
        for (int i = 0; i < pickups.Length; i++)
        {
            pickups[i].gameObject.SetActive(i == index);
        }

    }
}
