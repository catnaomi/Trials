using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageHitboxListener : MonoBehaviour
{
    void HitboxActive(int active)
    {
        if (active == 0)
        {
            this.GetComponent<AnimancerComponent>().States.Current.Speed = 0f;
        }
    }
}
