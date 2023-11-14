using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeTransformRoot : MonoBehaviour, IEventVisualizable
{
    public Transform target;
     
    public void MakeRoot()
    {
        target.SetParent(null, true);
    }
    public GameObject[] GetEventTargets()
    {
        return new GameObject[] { target.gameObject };
    }

}
