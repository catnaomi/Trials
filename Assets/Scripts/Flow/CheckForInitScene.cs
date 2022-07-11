using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckForInitScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (SceneManager.GetSceneByName("_InitScene").buildIndex < 0)
        {
            SceneManager.LoadScene("_InitScene", LoadSceneMode.Additive);
            Debug.LogWarning("Loading init scene!");
        }
    }
}
