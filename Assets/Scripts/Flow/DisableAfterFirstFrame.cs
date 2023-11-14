using System.Collections;
using UnityEngine;

public class DisableAfterFirstFrame : MonoBehaviour
{
    public void LateUpdate()
    {
        this.gameObject.SetActive(false);
        this.enabled = false;
    }
}