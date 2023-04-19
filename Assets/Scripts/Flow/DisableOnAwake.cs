using System.Collections;
using UnityEngine;

public class DisableOnAwake : MonoBehaviour
{
    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
}