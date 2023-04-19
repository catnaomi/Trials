using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObservationPointController : MonoBehaviour
{
    MaterialPropertyBlock block;
    Renderer renderer;
    Collider collider;
    public bool highlighted;
    public bool inUse;
    public CinemachineVirtualCamera vcam;
    public UnityEvent OnStartHighlight;
    public UnityEvent OnEndHighlight;
    public UnityEvent OnStartObserve;
    public UnityEvent OnEndObserve;
    // Start is called before the first frame update
    void Start()
    {
        renderer = this.GetComponent<Renderer>();
        collider = this.GetComponent<Collider>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_Highlighted", 0f);
        renderer.SetPropertyBlock(block);
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldHighlight = highlighted;
        if (!inUse)
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (collider.Raycast(cameraRay, out RaycastHit hit, 100f))
            {
                shouldHighlight = true;

            }
            else
            {
                shouldHighlight = false;
            }
        }
        
        if (shouldHighlight != highlighted)
        {
            if (shouldHighlight)
            {
                highlighted = true;
                OnStartHighlight.Invoke();
                MarkObservationHighlight();
            }
            else
            {
                highlighted = false;
                OnEndHighlight.Invoke();
                MarkObservationHighlight();
            }
        }
        block.SetFloat("_Highlighted", highlighted ? 1f : 0f);
        renderer.SetPropertyBlock(block);
    }

    public void MarkObservationHighlight()
    {
        TimeTravelController.time.MarkObservationPoint(highlighted, this);
    }

    public void StartObserve()
    {
        vcam.Priority = 11;
        OnStartObserve.Invoke();
    }

    public void StopObserve()
    {
        vcam.Priority = -1;
        OnEndObserve.Invoke();
    }
}
