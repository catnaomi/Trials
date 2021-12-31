using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSwordThrust : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;
    [ReadOnly]public Vector3 startPoint;
    [ReadOnly]public Vector3 endPoint;
    public Transform pseudoParent;
    LineRenderer lineRenderer;
    float TRAIL_FPS = 60f;
    public float lineFadeTime = 0.5f;
    float lineTimer = 0.5f;
    bool thrusting;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        //StartCoroutine("UpdateAtFPS");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTrail();
    }

    public void BeginThrust()
    {
        thrusting = true;
        startPoint = bottomPoint.position;

    }

    public void EndThrust()
    {
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
            endPoint = topPoint.position;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
        
    }

    public void SetTopPoint(Transform t)
    {
        topPoint = t;
    }

    public void SetBottomPoint(Transform t)
    {
        bottomPoint = t;
    }
}
