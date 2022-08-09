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
    // Start is called before the first frame update
    void Start()
    {
        freeLook = this.GetComponent<CinemachineFreeLook>();
        ShouldRecenter();
        //recenterDuration = Mathf.Max(freeLook.m_RecenterToTargetHeading.m_RecenteringTime, freeLook.m_YAxisRecentering.m_RecenteringTime, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (recentering)
        {
            if (PlayerActor.player.IsTargetHeld())
            {
                recenterClock = 0f;
            }
            else if (recenterClock > recenterDuration)
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
        }

        freeLook.m_RecenterToTargetHeading.m_enabled = recentering;
        freeLook.m_YAxisRecentering.m_enabled = recentering;

    }

    public void ShouldRecenter()
    {
        recentering = true;
        recenterClock = 0f;
    }
}
