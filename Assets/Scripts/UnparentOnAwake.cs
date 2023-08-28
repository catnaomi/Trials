using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentOnAwake : MonoBehaviour
{
    public bool alsoDisable;
    private void Awake()
    {
        if (this.transform.parent != null)
        {
            this.transform.SetParent(null, true);
        }
        if (alsoDisable)
        {
            this.StartCoroutine(DisableAfterFirstFrame());
        }
    }

    IEnumerator DisableAfterFirstFrame()
    {
        yield return new WaitForEndOfFrame();
        this.gameObject.SetActive(false);
    }
}
