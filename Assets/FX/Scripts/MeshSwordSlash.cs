using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JPBotelho;
using static JPBotelho.CatmullRom;

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
    [Header("Mesh Settings")] 
    public Material material;
    MeshFilter meshFilter;
    MeshRenderer renderer;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    Mesh mesh;

    public int MAX_QUADS = 20;
    float TRAIL_FPS = 60f;
    //[Header("Line Settings")]
    List<Vector3> lineVertices;
    LineRenderer lineRenderer;


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
        vertices = new List<Vector3>(MAX_QUADS * 4);
        triangles = new List<int>(MAX_QUADS * 6);
        uvs = new List<Vector2>(MAX_QUADS * 4);

        lineRenderer = this.GetComponentInChildren<LineRenderer>();
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
        //topCurve.DrawSpline(Color.white);
        //bottomCurve.DrawSpline(Color.black);
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
        this.transform.position = pseudoParent.position;
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

                    lineRenderer.SetPosition(i, topCurvePoints[i].position - this.transform.position);
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
    }
}
