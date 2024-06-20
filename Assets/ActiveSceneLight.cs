using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Light))]
public class ActiveSceneLight : MonoBehaviour
{
    float clock = 0f;
    bool listening;
    Light light;
    private void Start()
    {
        light = this.GetComponent<Light>();
        CheckListeners();
        StartCheckTimer();
    }
    // Start is called before the first frame update
    void OnEnable()
    {
        Check();
    }

    void StartCheckTimer()
    {
        if (!listening)
        {
            this.StartTimer(60f, true, Check);
        }
    }
    public void CheckListeners()
    {
        if (!listening)
        {
            if (SceneLoader.instance != null)
            {
                SceneLoader.GetOnActiveSceneChange().AddListener(Check);
                SceneLoader.GetOnFinishLoad().AddListener(Check);
                listening = true;
            }
        }
    }
    public void Check()
    {
        if (this.gameObject.scene == SceneManager.GetActiveScene())
        {
            light.enabled = true;
        }
        else
        {
            light.enabled = false;
        }
        CheckListeners();
    }
}
