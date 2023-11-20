using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthbarController : MonoBehaviour
{
    public bool setOnStart;
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        if (setOnStart)
        {
            ShowHealthbar();
        }
    }

    public void ShowHealthbar()
    {
        if (target == null)
        {
            target = this.gameObject;
        }
        BossHealthIndicator.SetTarget(target);
    }

    public void HideHealthbar()
    {
        BossHealthIndicator.Hide();
    }
}
