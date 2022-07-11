using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempRestartOnButton : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            SceneLoader.ShouldReloadScenes(true);
            SceneLoader.instance.StartCoroutine(SceneLoader.instance.DelayReloadRoutine(0.25f));
        }
    }
}
