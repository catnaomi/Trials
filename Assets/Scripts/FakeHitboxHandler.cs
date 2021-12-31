using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FakeHitboxHandler : MonoBehaviour
{
    public UnityEvent OnActive;
    public UnityEvent OnDeactive;
    private void Update()
    {
        
    }
    public void HitboxActive(int active)
    {
        if (active > 0)
        {
            OnActive.Invoke();
            Debug.Log("start");
        }
        else
        {
            OnDeactive.Invoke();
        }

    }
}