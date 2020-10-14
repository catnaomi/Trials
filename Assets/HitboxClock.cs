using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxClock : MonoBehaviour
{
    HitboxController hitbox;
    float clock;
    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<HitboxController>();
        hitbox.Activate();
    }

    // Update is called once per frame
    void Update()
    {
        clock += Time.deltaTime;
        if (clock > 1f)
        {
            hitbox.GetNewID();
            clock = 0f;
        }
    }
}
