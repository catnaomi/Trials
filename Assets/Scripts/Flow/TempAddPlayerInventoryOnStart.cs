using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAddPlayerInventoryOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DebugReflectionMethods.AddPlayerItems();
    }

}
