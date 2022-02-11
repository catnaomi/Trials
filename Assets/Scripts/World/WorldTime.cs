using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTime : MonoBehaviour
{

    public static float MAX_TIME = 24f;
    float SECONDS_TO_HOURS = 3600f;
    [Range(0f, 24f)]
    public float currentTime;
    public float timeScale;
    /*
     * Notes on timeScale:
     * 1 = 1:1 with real time (24 hours per 24 hours)
     * 24 = 1 hour to complete (1 hour per 24 hours)
     * 60 = 24 minutes to complete (1 minute per hour)
     * 1440 = 1 minute to complete (1 minute per 24 hours)
     */
    [SerializeField, ReadOnly] private Material skybox;
    // Start is called before the first frame update
    void Start()
    {
        Material realSkybox = RenderSettings.skybox;
        skybox = Instantiate(realSkybox);
        RenderSettings.skybox = skybox;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += timeScale / SECONDS_TO_HOURS * Time.deltaTime;
        currentTime %= MAX_TIME;
        skybox.SetFloat("_ClockTime", currentTime);
    }

    private void OnDestroy()
    {
        Destroy(skybox);
    }
}
