using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossMessageHandler : MonoBehaviour
{
    [Header("Particles")]
    public ParticleSystem dash;
    public ParticleSystem dashStart;
    public ParticleSystem jump;
    bool dashing;
    public void StartDash()
    {
        dashStart.transform.position = GetPositionHoriz();
        dashStart.Play();
        dash.transform.position = GetPositionHoriz();
        dash.Play();

    }

    public void StopDash()
    {
        dash.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void StartPillarHighJump()
    {
        jump.transform.position = GetPositionHoriz();
        jump.Play();
    }
    void Update()
    {
        Vector3 pos = this.transform.position;
        pos.y = 0;
        dash.transform.position = pos;
    }

    Vector3 GetPositionHoriz(float y = 0f)
    {
        Vector3 pos = this.transform.position;
        pos.y = y;
        return pos;
    }
}
