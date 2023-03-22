using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCamera : MonoBehaviour
{
    public static FocusCamera instance;
    public CinemachineVirtualCamera vcam;

    public float focusDuration = 5f;
    public int camPriority = 11;
    private void Awake()
    {
        instance = this;
    }
    
    public static void FocusOnTransform(Transform target)
    {
        if (instance != null)
        {
            instance.FocusOnTransformInstance(target);
        }
    }
    public void FocusOnTransformInstance(Transform target)
    {

        vcam.transform.position = Camera.main.transform.position;
        vcam.transform.rotation = Camera.main.transform.rotation;
        vcam.LookAt = target;

        vcam.Priority = camPriority;
        StartCoroutine(UnfocusAfterDuration());
    }

    IEnumerator UnfocusAfterDuration()
    {
        yield return new WaitForSecondsRealtime(focusDuration);
        vcam.Priority = -1;
    }
}
