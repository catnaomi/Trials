using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoPanelMaintainView : MonoBehaviour
{
    public Transform origin;
    public Transform front;
    public Transform back;
    public float radius;
    public float camInitialDistance = 1f;
    public bool maintainSize = true;
    Vector3 initialScale;
    // Start is called before the first frame update
    void Start()
    {
        initialScale = front.transform.localScale;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 dir = origin.transform.position - Camera.main.transform.position;
        //dir.y = 0f;
        front.transform.position = origin.transform.position + dir.normalized * -radius;
        front.transform.rotation = Quaternion.LookRotation(dir.normalized);
        float fdist = Vector3.Distance(front.transform.position, Camera.main.transform.position);
        if (maintainSize) front.transform.localScale = initialScale * fdist / camInitialDistance;

        back.transform.position = origin.transform.position + dir.normalized * radius;
        back.transform.rotation = Quaternion.LookRotation(dir.normalized);
        float bdist = Vector3.Distance(back.transform.position, Camera.main.transform.position);
        if (maintainSize) back.transform.localScale = initialScale * bdist / camInitialDistance;
    }
}
