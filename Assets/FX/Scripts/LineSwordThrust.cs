using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSwordThrust : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;
    [ReadOnly] public List<Vector3> points;
    public Transform pseudoParent;
    ParticleSystem bloodParticles;
    LineRenderer lineRenderer;
    float TRAIL_FPS = 60f;
    public float lineFadeTime = 0.5f;
    float lineTimer = 0f;
    public float bloodFadeTime = 0.5f;
    public float bloodFadeDelay = 0.5f;
    float bloodTimer;
    bool thrusting;
    bool bleeding;
    bool first;
    Vector3 contactPoint;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = this.GetComponentInChildren<LineRenderer>();
        first = true;
        StartCoroutine("UpdateAtFPS");
    }

    // Update is called once per frame
    void Update()
    {
        if (thrusting)
        {
            lineTimer = lineFadeTime;
        }
        else
        {
            if (lineTimer > 0)
            {
                lineTimer -= Time.deltaTime;
            }
            else
            {
                lineTimer = 0f;
                if (points.Count > 0)
                {
                    first = true;
                    points.Clear();
                }

            }

        }
    }

    public void OnDestroy()
    {
        StopCoroutine("UpdateAtFPS");
    }
    public void BeginThrust()
    {
        thrusting = true;
        points.Clear();
        points.Add(bottomPoint.position);
        points.Add(topPoint.position);
    }

    public void EndThrust()
    {
        if (thrusting) first = false;
        thrusting = false;
        
    }

    IEnumerator UpdateAtFPS()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / TRAIL_FPS);
            UpdateTrail();
        }
    }
    private void UpdateTrail()
    {
        if (thrusting)
        {
            if (true)//first)
            {
                points[points.Count - 1] = topPoint.position;
            }
            else
            {
                points.Add(topPoint.position);
            }
            
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

    }

    public void SetTopPoint(Transform t)
    {
        topPoint = t;
    }

    public void SetBottomPoint(Transform t)
    {
        bottomPoint = t;
    }

    bool IsActive()
    {
        return thrusting || lineTimer > 0f;
    }

    public void Bleed()
    {
        bloodParticles = this.GetComponentInChildren<ParticleSystem>();
        bloodParticles.transform.position = contactPoint;
        bloodParticles.Play();
        bloodTimer = bloodFadeDelay + bloodFadeTime;
        bleeding = true;
    }

    public void StopBleeding()
    {
        bloodParticles.Stop();
        bleeding = false;
    }

    public void SetContactPoint(Vector3 position)
    {
        contactPoint = position;
    }
}
