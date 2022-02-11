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
    public float timeUpdateDelay = 0.1f;
    [SerializeField, ReadOnly] private Material skybox;
    [Header("Light Colors & Angles")]
    public Color dayColor = Color.cyan;
    public Color duskColor = Color.yellow;
    public Color nightColor = Color.blue;
    [SerializeField, ReadOnly] private Light dsun;
    [SerializeField, ReadOnly] private Light nsun;
    public Color sunColor;
    public float dsunAngle = 0f;
    public float nsunAngle = 0f;
    public float sunAngleClampInner = 30f;
    public float sunAngleClampOuter = 15f;
    public float dsunIntensity;
    public float nsunIntensity;
    public float dsunShadow;
    public float nsunShadow;
    // Start is called before the first frame update
    void Start()
    {
        Material realSkybox = RenderSettings.skybox;
        skybox = Instantiate(realSkybox);
        RenderSettings.skybox = skybox;
        GameObject dsunObj = GameObject.Find("_DaySunLight");
        
        if (dsunObj != null)
        {
            dsun = dsunObj.GetComponent<Light>();
            dsunIntensity = dsun.intensity;
            dsunShadow = dsun.shadowStrength;
        }
        GameObject nsunObj = GameObject.Find("_NightSunLight");
        if (nsunObj != null)
        {
            nsun = nsunObj.GetComponent<Light>();
            nsunIntensity = nsun.intensity;
            nsunShadow = nsun.shadowStrength;
        }
        StartCoroutine(UpdateTime());
    }

    // Update is called once per frame
    IEnumerator UpdateTime()
    {
        do
        {
            currentTime += timeScale / SECONDS_TO_HOURS * timeUpdateDelay;
            currentTime %= MAX_TIME;
            skybox.SetFloat("_ClockTime", currentTime);
            float t;
            if (InRange(currentTime, 0, 5f))
            {
                sunColor = nightColor;
                nsunAngle = Mathf.Lerp(sunAngleClampInner, 180f - sunAngleClampInner, T(currentTime, -6f, 5f));
                nsun.intensity = nsunIntensity;
                dsun.intensity = 0f;
                nsun.shadowStrength = nsunShadow;
                dsun.shadowStrength = 0f;
            }
            else if (InRange(currentTime, 5f, 5.25f))
            {
                sunColor = Color.Lerp(nightColor, duskColor, T(currentTime, 5f, 5.25f));
                nsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 5f, 6f));
                nsun.intensity = nsunIntensity;
                dsun.intensity = 0f;
                nsun.shadowStrength = Mathf.Lerp(nsunShadow, 0f, T(currentTime, 5f, 5.5f));
                dsun.shadowStrength = 0f;
            }
            else if (InRange(currentTime, 5.25f, 5.75f))
            {
                sunColor = duskColor;
                nsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 5f, 6f));
                dsunAngle = Mathf.Lerp(sunAngleClampOuter, sunAngleClampInner, T(currentTime, 5.25f, 6f));
                dsun.intensity = Mathf.Lerp(0f, dsunIntensity, T(currentTime, 5.25f, 6f));
                nsun.intensity = Mathf.Lerp(nsunIntensity, 0f, T(currentTime, 5.25f, 6f));
                nsun.shadowStrength = Mathf.Lerp(nsunShadow, 0f, T(currentTime, 5f, 5.5f));
                dsun.shadowStrength = Mathf.Lerp(0f, dsunShadow, T(currentTime, 5.5f, 6f));
            }
            else if (InRange(currentTime, 5.75f, 6f))
            {
                sunColor = Color.Lerp(duskColor, dayColor, T(currentTime, 5.75f, 6f));
                nsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 5f, 6f));
                dsunAngle = Mathf.Lerp(sunAngleClampOuter, sunAngleClampInner, T(currentTime, 5.25f, 6f));
                dsun.intensity = Mathf.Lerp(0f, dsunIntensity, T(currentTime, 5.25f, 6f));
                nsun.intensity = Mathf.Lerp(nsunIntensity, 0f, T(currentTime, 5.25f, 6f));
                nsun.shadowStrength = 0f;
                dsun.shadowStrength = Mathf.Lerp(0f, dsunShadow, T(currentTime, 5.5f, 6f));
            }
            else if (InRange(currentTime, 6f, 17f))
            {
                sunColor = dayColor;
                dsunAngle = Mathf.Lerp(sunAngleClampInner, 180f - sunAngleClampInner, T(currentTime, 6f, 17f));
                dsun.intensity = dsunIntensity;
                nsun.intensity = 0f;
                nsun.shadowStrength = 0f;
                dsun.shadowStrength = dsunShadow;
            }
            else if (InRange(currentTime, 17f, 17.25f))
            {
                sunColor = Color.Lerp(dayColor, duskColor, T(currentTime, 17f, 17.25f));
                dsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 17f, 18f));
                dsun.intensity = dsunIntensity;
                nsun.intensity = 0f;
                dsun.shadowStrength = Mathf.Lerp(dsunShadow, 0f, T(currentTime, 17f, 17.75f));
                nsun.shadowStrength = 0f;
            }
            else if (InRange(currentTime, 17.25f, 17.75f))
            {
                sunColor = duskColor;
                dsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 17f, 18f));
                nsunAngle = Mathf.Lerp(sunAngleClampOuter, sunAngleClampInner, T(currentTime, 17.25f, 18f));
                nsun.intensity = Mathf.Lerp(0f, nsunIntensity, T(currentTime, 17.25f, 18f));
                dsun.intensity = Mathf.Lerp(dsunIntensity, 0f, T(currentTime, 17.25f, 18f));
                dsun.shadowStrength = Mathf.Lerp(dsunShadow, 0f, T(currentTime, 17f, 17.75f));
                nsun.shadowStrength = 0f;
            }
            else if (InRange(currentTime, 17.75f, 18f))
            {
                sunColor = Color.Lerp(duskColor, nightColor, T(currentTime, 17.75f, 18f));
                dsunAngle = Mathf.Lerp(180f - sunAngleClampInner, 180f - sunAngleClampOuter, T(currentTime, 17f, 18f));
                nsunAngle = Mathf.Lerp(sunAngleClampOuter, sunAngleClampInner, T(currentTime, 17.75f, 18f));
                nsun.intensity = Mathf.Lerp(0f, nsunIntensity, T(currentTime, 17.25f, 18f));
                dsun.intensity = Mathf.Lerp(dsunIntensity, 0f, T(currentTime, 17.25f, 18f));
                dsun.shadowStrength = 0f;
                nsun.shadowStrength = Mathf.Lerp(0f, nsunShadow, T(currentTime, 17.75f, 18f));
            }
            else if (InRange(currentTime, 18f, 24f))
            {
                sunColor = nightColor;
                nsunAngle = Mathf.Lerp(sunAngleClampInner, 180f - sunAngleClampInner, T(currentTime, 18f, 29f));
                nsun.intensity = nsunIntensity;
                dsun.intensity = 0f;
                dsun.shadowStrength = 0f;
                nsun.shadowStrength = nsunShadow;
            }
            else
            {
                Debug.LogWarning("time is not within any time range!");
            }
            nsun.color = sunColor;
            dsun.color = sunColor;
            //sunAngle = ((currentTime + 6f) % 12f) / 12f;
            //sunAngle = currentTime / 24f;
            Vector3 sunDir = Vector3.right;
            //sunDir = Quaternion.Euler(sunAngle * Mathf.PI * Mathf.Rad2Deg, 0f, 0f) * sunDir; Vector3.rot
            dsun.transform.rotation = Quaternion.Euler(dsunAngle, 0f, 0f);
            nsun.transform.rotation = Quaternion.Euler(nsunAngle, 0f, 0f);
            dsun.enabled = (dsun.intensity != 0);
            nsun.enabled = (nsun.intensity != 0);

            yield return new WaitForSecondsRealtime(timeUpdateDelay);
        }
        while (true);
    }

    private void OnDestroy()
    {
        Destroy(skybox);
    }

    bool InRange(float x, float min, float max)
    {
        return x >= min && x < max;
    }

    float T(float x, float min, float max)
    {
        return (x - min) / (max - min);
    }
}
