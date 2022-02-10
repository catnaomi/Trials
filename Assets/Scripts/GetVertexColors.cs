using System.Collections;
using UnityEngine;

public class GetVertexColors : MonoBehaviour
{

    public Color[] colors;
    // Use this for initialization
    void Start()
    {
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        colors = meshFilter.sharedMesh.colors;
    }

    // Update is called once per frame
    void Update()
    {

    }
}