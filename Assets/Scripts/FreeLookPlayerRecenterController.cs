using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookPlayerRecenterController : MonoBehaviour
{
    CinemachineFreeLook freeLook;
    public bool recentering;
    public float recenterDuration;
    float recenterClock;
    bool wasRecenteringLastFrame;

    public Cinemachine.AxisState.Recentering y_axisRecenterTarget;
    Cinemachine.AxisState.Recentering y_axisRecenterBase;
    public Cinemachine.AxisState.Recentering heading_axisRecenterTarget;
    Cinemachine.AxisState.Recentering heading_axisRecenterBase;
    // Start is called before the first frame update
    void Start()
    {
        freeLook = this.GetComponent<CinemachineFreeLook>();
        ShouldRecenter();
        y_axisRecenterBase = freeLook.m_YAxisRecentering;
        heading_axisRecenterBase = freeLook.m_RecenterToTargetHeading;
        //recenterDuration = Mathf.Max(freeLook.m_RecenterToTargetHeading.m_RecenteringTime, freeLook.m_YAxisRecentering.m_RecenteringTime, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (recentering)
        {
            /*
            if (PlayerActor.player.IsTargetHeld())
            {
                recenterClock = 0f;
            }
            else*/ 
            if (recenterClock > recenterDuration)
            {
                recentering = false;
            }
            else
            {
                recenterClock += Time.deltaTime;
            }
            if (PlayerActor.player.look.magnitude > 0.01f)
            {
                recentering = false; 
            }
            recenterClock += Time.deltaTime;
            if (recenterClock > recenterDuration)
            {
                recentering = false;
            }
            if (recentering)
            {
                Debug.Log("recentering target camera!");
            }

        }
        if (recentering && !wasRecenteringLastFrame)
        {
            freeLook.m_RecenterToTargetHeading = heading_axisRecenterTarget;
            freeLook.m_YAxisRecentering = y_axisRecenterTarget;
        }
        else if (!recentering && wasRecenteringLastFrame)
        {
            freeLook.m_RecenterToTargetHeading = heading_axisRecenterBase;
            freeLook.m_YAxisRecentering = y_axisRecenterBase;
        }
        wasRecenteringLastFrame = recentering;

        //freeLook.m_RecenterToTargetHeading.m_enabled = recentering;
        //freeLook.m_YAxisRecentering.m_enabled = recentering;

    }

    public void ShouldRecenter()
    {
        recentering = true;
        recenterClock = 0f;
    }
}
