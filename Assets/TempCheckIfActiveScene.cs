using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempCheckIfActiveScene : MonoBehaviour
{
    float clock = 0f;

    private void Start()
    {
        SceneLoader.GetOnActiveSceneChange().AddListener(Check);
        SceneLoader.GetOnFinishLoad().AddListener(Check);
    }
    // Start is called before the first frame update
    void OnEnable()
    {
        Check();
    }

    // Update is called once per frame
    void Update()
    {
        clock += Time.deltaTime;
        if (clock > 60f)
        {
            Check();
            
        }
    }

    public void Check()
    {
        if (this.gameObject.scene == SceneManager.GetActiveScene())
        {
            this.GetComponent<Light>().enabled = true;
        }
        else
        {
            this.GetComponent<Light>().enabled = false;
        }
        clock = 0f;
    }
}
