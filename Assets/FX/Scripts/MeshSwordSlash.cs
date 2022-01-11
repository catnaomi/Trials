using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JPBotelho;
using static JPBotelho.CatmullRom;
using UnityEngine.Events;
using Cinemachine;

public class MeshSwordSlash : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;
    public Transform pseudoParent;
    CatmullRom topCurve;
    List<Vector3> topPoints;
    List<Vector3> bottomPoints;
    CatmullRom bottomCurve;
    List<Vector3> emptyPoints = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    Vector3[] controlPoints;
    public int resolution = 2;
    public float fadeoutTime = 1f;
    public float lineFadeTime = 0.5f;
    float fadeoutTimer = 1f;
    float lineTimer = 0.5f;
    public Color color = Color.white;
    [Header("Mesh Settings")] 
    public Material material;
    MaterialPropertyBlock block;
    MeshFilter meshFilter;
    MeshRenderer renderer;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    Mesh mesh;

    public int MAX_QUADS = 20;
    float TRAIL_FPS = 60f;
    [Header("Line Settings")]
    public LineRenderer lineRenderer;
    public LineRenderer bloodlineRenderer;
    public float bloodFadeTime = 1f;
    public float bloodFadeDelay = 2f;
    Vector3 contactPoint;
    Vector3 contactDir;
    int contactIndex;
    float bloodTimer;
    bool bleeding;
    List<Vector3> lineVertices;

    public CinemachineImpulseSource impulse;
    public float impulseMag = 0.2f;
    public UnityEvent OnBleed;

    [ReadOnly] public bool slashing = false;
    int slashFrames = 0;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        renderer = gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        renderer.sharedMaterial = material;
        block = new MaterialPropertyBlock();
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
        vertices = new List<Vector3>(MAX_QUADS * 4);
        triangles = new List<int>(MAX_QUADS * 6);
        uvs = new List<Vector2>(MAX_QUADS * 4);

        //lineRenderer = this.GetComponentInChildren<LineRenderer>();
        lineVertices = new List<Vector3>(MAX_QUADS * 2);
        StartCoroutine("UpdateAtFPS");

        topPoints = new List<Vector3>();
        bottomPoints = new List<Vector3>();
        topCurve = new CatmullRom(emptyPoints, resolution, false);
        bottomCurve = new CatmullRom(emptyPoints, resolution, false);

    }

    public void BeginSlash()
    {
        slashing = true;
        slashFrames = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        lineVertices.Clear();
        topPoints.Clear();
        bottomPoints.Clear();
        mesh.Clear();
        StopBleeding();
    }

    public void EndSlash()
    {
        slashing = false;
    }

    public void OnDestroy()
    {
        StopCoroutine("UpdateAtFPS");
    }

    public void SetTopPoint(Transform t)
    {
        topPoint = t;
    }

    public void SetBottomPoint(Transform t)
    {
        bottomPoint = t;
    }
    private void Update()
    {
        //UpdateTrail();
        topCurve.DrawSpline(Color.white);
        topCurve.DrawTangents(0.25f, Color.blue);
        //bottomCurve.DrawSpline(Color.black);
        if (slashing)
        {
            fadeoutTimer = fadeoutTime;
            lineTimer = lineFadeTime;
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }
        else
        {
            if (fadeoutTimer > 0)
            {
                fadeoutTimer -= Time.deltaTime;
            }
            else
            {
                fadeoutTimer = 0f;
            }
            if (lineTimer > 0)
            {
                lineTimer -= Time.deltaTime;
            }
            else
            {
                lineTimer = 0f;
            }
            if (bloodTimer > 0)
            {
                bloodTimer -= Time.deltaTime;
            }
            else
            {
                bloodTimer = 0f;
                if (bleeding) StopBleeding();
            }

            float alpha = fadeoutTimer / fadeoutTime;
            block.SetColor("_BaseColor", new Color(color.r, color.g, color.b, alpha));
            renderer.SetPropertyBlock(block);
            lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, alpha);
            lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, alpha);
        }
        if (slashing || fadeoutTimer > 0 || lineTimer > 0)
        {
            this.transform.position = pseudoParent.position;
        }
        CatmullRomPoint[] points = topCurve.GetPoints();
        if (contactIndex > 0 && points.Length > contactIndex & bleeding)
        {
            contactPoint = points[contactIndex].position;
            //contactDir = Vector3.ProjectOnPlane(points[contactIndex].tangent.normalized, Camera.main.transform.forward).normalized;
            contactDir = points[contactIndex].tangent.normalized;
        }
        if (bleeding)
        {
            bloodlineRenderer.transform.position = contactPoint;
            bloodlineRenderer.transform.rotation = Quaternion.LookRotation(contactDir);
        }
        else
        {
            if (bloodlineRenderer.positionCount < bloodlinePoints.Length)
            {
                Debug.Log("reset");
                bloodlineRenderer.positionCount = bloodlinePoints.Length;
                bloodlineRenderer.SetPositions(bloodlinePoints);
            }
        }
    }
    IEnumerator UpdateAtFPS()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / TRAIL_FPS);
            yield return new WaitForEndOfFrame();
            UpdateTrail();
        }
    }
    private void UpdateTrail()
    {
        
        if (slashing)
        {
            topPoints.Add(topPoint.position);
            bottomPoints.Add(bottomPoint.position);
            topCurve.Update(topPoints);
            bottomCurve.Update(bottomPoints);
            slashFrames++;
            // mesh
            
            if (slashFrames >= 2)
            {
                vertices.Clear();
                uvs.Clear();
                triangles.Clear();
                CatmullRomPoint[] topCurvePoints = topCurve.GetPoints();
                CatmullRomPoint[] bottomCurvePoints = bottomCurve.GetPoints();
                lineRenderer.positionCount = topCurvePoints.Length;
                //bloodlineRenderer.positionCount = topCurvePoints.Length;
                for (int i = 0; i < topCurvePoints.Length; i++)
                {
                    vertices.Add(topCurvePoints[i].position - this.transform.position);
                    vertices.Add(bottomCurvePoints[i].position - this.transform.position);
                    //vertices.Add(topCurvePoints[i].position + Vector3.up);
                    float x = (float)i / (float)(topCurvePoints.Length - 1);
                    uvs.Add(new Vector2(x, 0));
                    uvs.Add(new Vector2(x, 1));


                    
                    
                    if (vertices.Count >= 4)
                    {
                        int c = vertices.Count;
                        // 0 = (c-4)
                        // 1 = (c-3)
                        // 2 = (c-2)
                        // 3 = (c-1)
                        int i0 = c - 4;
                        int i1 = c - 3;
                        int i2 = c - 2;
                        int i3 = c - 1;

                        triangles.Add(i0);
                        triangles.Add(i2);
                        triangles.Add(i1);

                        triangles.Add(i3);
                        triangles.Add(i1);
                        triangles.Add(i2);
                    }

                    lineRenderer.SetPosition(topCurvePoints.Length - 1 - i, topCurvePoints[i].position - this.transform.position);
                    //bloodlineRenderer.SetPosition(topCurvePoints.Length - 1 - i, topCurvePoints[i].position - this.transform.position);
                }
                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.SetUVs(0, uvs);
                controlPoints = topCurve.GetControlPoints();
            }

            /*
            vertices.Add(topPoint.position - this.transform.position);
            vertices.Add(bottomPoint.position - this.transform.position);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            

            
            if (vertices.Count >= 4)
            {
                int c = vertices.Count;
                // 0 = (c-4)
                // 1 = (c-3)
                // 2 = (c-2)
                // 3 = (c-1)
                int i0 = c - 4;
                int i1 = c - 3;
                int i2 = c - 2;
                int i3 = c - 1;
                for (int i = 0; i < slashFrames; i++)
                {
                    float x = (float)i / (float)(slashFrames - 1);
                    uvs[2 * i] = new Vector2(x, 0);
                    uvs[(2 * i) + 1] = new Vector2(x, 1);
                    //uvs[i-1] = new Vector2(
                }

                triangles.Add(i0);
                triangles.Add(i2);
                triangles.Add(i1);

                triangles.Add(i3);
                triangles.Add(i1);
                triangles.Add(i2);

                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.SetUVs(0, uvs);
            }
            */

        }
        else
        {
            //if (fadeoutTimer <= 0)
            //{
            int count = Mathf.FloorToInt(topCurve.GetPoints().Length * (lineTimer / lineFadeTime));
            if (count < 0)
            {
                count = 0;
            }
            lineRenderer.positionCount = count;
            
            if (bleeding)
            {
                if (bloodTimer <= (bloodFadeTime))
                {
                    int bcount = Mathf.FloorToInt(bloodlinePoints.Length * (bloodTimer / bloodFadeTime));
                    if (bcount < 0)
                    {
                        bcount = 0;
                    }
                     bloodlineRenderer.positionCount = bcount;
                }
            }
        }
    }
    Vector3[] bloodlinePoints = { 0.75f * Vector3.forward, 0.5f * Vector3.forward, 0.25f * Vector3.forward, Vector3.zero, -0.25f * Vector3.forward, -0.5f * Vector3.forward, -0.75f * Vector3.forward };
    public void Bleed()
    {
        bloodlineRenderer.gameObject.SetActive(true);
        ParticleSystem particles = bloodlineRenderer.GetComponentInChildren<ParticleSystem>();
        bloodlineRenderer.transform.position = contactPoint;
        bloodlineRenderer.transform.rotation = Quaternion.LookRotation(contactDir);
        particles.Play();
        bloodTimer = bloodFadeDelay + bloodFadeTime;
        bleeding = true;
        this.GetComponent<AudioSource>().Play();
        OnBleed.Invoke();
        Shake();
    }

    public void StopBleeding()
    {
        bloodlineRenderer.GetComponentInChildren<ParticleSystem>().Stop();
        bloodlineRenderer.gameObject.SetActive(false);
        bleeding = false;
    }

    public void Shake()
    {
        impulse.GenerateImpulseWithVelocity(contactDir.normalized * impulseMag);
    }

    public void SetContactPoint(Vector3 position)
    {
        contactPoint = position;
        CatmullRomPoint[] points = topCurve.GetPoints();
        if (points.Length > 0)
        {
            

            Vector3 leadingPoint = position;
            float leadingDist = Mathf.Infinity;
            Vector3 tangent = pseudoParent.transform.right;
            for (int i = 0; i < points.Length; i++)
            {
                CatmullRomPoint point = points[i];
                float dist = Vector3.Distance(point.position, position);
                if (dist < leadingDist)
                {
                    leadingDist = dist;
                    leadingPoint = point.position;
                    tangent = point.tangent;
                    contactIndex = i;
                }
            }
            contactPoint = leadingPoint;
            contactDir = Vector3.ProjectOnPlane(tangent.normalized, pseudoParent.transform.forward).normalized;
            //contactDir = tangent.normalized;
        }
        else
        {
            contactIndex = -1;
            contactDir = pseudoParent.transform.right;
            Debug.Log("empty on hit");
        }

        /*
        Vector3 leadingPoint = position;
        float leadingDist = Mathf.Infinity;
        foreach (CatmullRomPoint point in topCurve.GetPoints())
        {
            float dist = Vector3.Distance(point.position, position);
            if (dist < leadingDist)
            {
                dist = leadingDist;
                leadingPoint = point.position;
            }
        }
        contactPoint = leadingPoint;
        */
    }
}
