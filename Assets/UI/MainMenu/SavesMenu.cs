using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavesMenu : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject saveControllerPrefab;
    [Header("References")]
    public CanvasGroup group;
    

    // Start is called before the first frame update
    void Start()
    {
        group.alpha = 0;
    }


    public void Init()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
