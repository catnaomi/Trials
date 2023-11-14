using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGolemPlayPuddle : MonoBehaviour, IEventVisualizable
{
    public IceGolemMecanimActor actor;
    bool started;

    public GameObject[] GetEventTargets()
    {
        return new GameObject[] { actor.gameObject };
    }

    private void OnEnable()
    {
        if (started)
        {
            actor.PlayPuddle();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        started = true;
    }
}
