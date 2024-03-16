using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController instance;

    public float desiredTimescale = 1f;
    public bool paused;

    float defaultFixedDeltaTime;

    void Awake()
    {
        instance = this;
        paused = false;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        Time.timeScale = paused ? 0f : desiredTimescale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }
}
