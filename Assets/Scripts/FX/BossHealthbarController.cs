using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthbarController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        BossHealthIndicator.SetTarget(this.gameObject);
    }
}
