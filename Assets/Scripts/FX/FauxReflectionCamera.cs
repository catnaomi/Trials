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
    [Header("Time Travel Settings")]
    public bool precisionMod = true;
    IAffectedByTimeTravel timeTravelHandler;
    double time;
    // Start is called before the first frame update
    void Start()
    {
        InitTimeTravelHandler();
        plane = new Plane(Vector3.up, new Vector3(0, mirrorHeight, 0));
        rt = new RenderTexture(1024, 576, 24);
        rt.name = "_fauxreflection";
        block = new MaterialPropertyBlock();
        block.SetTexture("_ReflectionMap", rt);
        block.SetFloat("_UseTime", 1f);
        block.SetFloat("_InputTime", (float)time);

        foreach (Renderer r in reflectedRenderers)
        {
            r.SetPropertyBlock(block);
        }
        cam.targetTexture = rt;
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main == null) return;
        cam.fieldOfView = Camera.main.fieldOfView;
        Vector3 mainHeading = Camera.main.transform.forward;
        Vector3 planeProject = Vector3.ProjectOnPlane(mainHeading, Vector3.up);
        Vector3 reflectedHeading = Quaternion.AngleAxis(180f, planeProject) * mainHeading;

        this.transform.rotation = Quaternion.LookRotation(reflectedHeading);


        float dist = Camera.main.transform.position.y - mirrorHeight;
        this.transform.position = Camera.main.transform.position - (Vector3.up * dist * 2f);

        if (timeTravelHandler != null && !timeTravelHandler.IsFrozen())
        {
            time += Time.deltaTime;
            if (precisionMod) time %= 86400;
            block.SetFloat("_UseTime", 1f);
            block.SetFloat("_InputTime", (float)(time));

            foreach (Renderer r in reflectedRenderers)
            {
                r.SetPropertyBlock(block);
            }
        }
    }

    void InitTimeTravelHandler()
    {
        timeTravelHandler = GetComponent<IAffectedByTimeTravel>();
    }
}
