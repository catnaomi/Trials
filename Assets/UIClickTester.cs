using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class UIClickTester : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        InputSystemUIInputModule input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        input.actionsAsset["Click"].performed += (c) => { Debug.Log("click"); };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
