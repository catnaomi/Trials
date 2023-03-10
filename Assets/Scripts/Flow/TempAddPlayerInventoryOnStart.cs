using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAddPlayerInventoryOnStart : MonoBehaviour
{
    bool hasAdded;

    private void Start()
    {
        hasAdded = false;
    }

    private void Update()
    {
        if (PlayerActor.player != null && !hasAdded)
        {
            hasAdded = true;
            DebugReflectionMethods.AddPlayerItems();
        }
    }

}
