using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutThenLoadScene : MonoBehaviour
{
    public float duration = 1f;
    public string scene;
    public void StartFadeTrigger()
    {
        FadeToBlackController.FadeOut(duration, () =>
        {
            SceneLoader.LoadWithProgressBar(scene);
        });
    } 
}
