using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController instance;
    public float scale = 1f;
    float fixedDeltaTime;
    float lastScale = 1f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            this.enabled = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != scale && lastScale != scale)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
        lastScale = scale;
    }
}
