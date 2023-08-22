using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentOnAwake : MonoBehaviour
{
    private void Awake()
    {
        if (this.transform.parent != null)
        {
            this.transform.SetParent(null, true);
        }
    }
}
