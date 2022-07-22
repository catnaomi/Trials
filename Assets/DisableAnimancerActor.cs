using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAnimancerActor : MonoBehaviour
{
    public void DisableComponents()
    {
        this.GetComponent<AnimancerComponent>().enabled = false;
        this.GetComponent<Actor>().enabled = false;
    }

    public void EnableComponents()
    {
        this.GetComponent<AnimancerComponent>().enabled = true;
        this.GetComponent<Actor>().enabled = true;
    }
}
