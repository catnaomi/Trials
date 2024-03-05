using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController instance;

    public float desiredTimescale = 1f;

    private bool _paused;
    public bool paused
    {
        get => _paused;
        set
        {
            Debug.Log($"Setting paused {paused}");
            _paused = value;
        }
    }

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
