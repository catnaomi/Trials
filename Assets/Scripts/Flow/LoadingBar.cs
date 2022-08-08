using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public Image bar;
    public TMP_Text text;
    public float progress;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        if (SceneLoader.instance != null)
        {
            progress = SceneLoader.instance.GetSceneLoadingProgress();
        }
    }

    private void OnGUI()
    {
        text.text = progress.ToString("P0");
        bar.fillAmount = progress;
    }
}
