using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxReflectionCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam;
    public float mirrorHeight = 0f;
    Plane plane;
    [Header("Rendering Settings")]
    public Renderer[] reflectedRenderers;
    RenderTexture rt;
    MaterialPropertyBlock block;
    // Start is called before the first frame update
    void Start()
    {
        plane = new Plane(Vector3.up, new Vector3(0, mirrorHeight, 0));
        rt = new RenderTexture(1024, 576, 24);
        block = new MaterialPropertyBlock();
        block.SetTexture("_ReflectionMap", rt);
        foreach(Renderer r in reflectedRenderers)
        {
            r.SetPropertyBlock(block);
        }
        cam.targetTexture = rt;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mainHeading = Camera.main.transform.forward;
        Vector3 planeProject = Vector3.ProjectOnPlane(mainHeading, Vector3.up);
        Vector3 reflectedHeading = Quaternion.AngleAxis(180f, planeProject) * mainHeading;

        this.transform.rotation = Quaternion.LookRotation(reflectedHeading);


        float dist = Camera.main.transform.position.y - mirrorHeight;
        this.transform.position = Camera.main.transform.position - (Vector3.up * dist * 2f);
    }

    private void OnDestroy()
    {
        //Destroy(rt);
    }
}
