using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnObject : MonoBehaviour
{

    public GameObject obj;
    public float delay;
    float clock;
    Vector3 targetPosition;
    Quaternion targetRotation;
    GameObject workingInstance;
    GameObject copy;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = obj.transform.position;
        targetRotation = obj.transform.rotation;
        copy = Instantiate(obj, this.transform.position, this.transform.rotation);
        copy.SetActive(false);
        workingInstance = obj;
        clock = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (workingInstance == null)
        {
            if (clock <= 0)
            {
                workingInstance = Instantiate(copy, targetPosition, targetRotation);
                workingInstance.SetActive(true);
                clock = delay;
            }
            else
            {
                clock -= Time.deltaTime;
            }
        }
        else
        {
            clock = delay;
        }
    }
}
