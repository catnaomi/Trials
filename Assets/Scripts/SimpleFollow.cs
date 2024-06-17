using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    public Transform target;
    Vector3 offset;
    [SerializeField] private UpdateMode updateMode = UpdateMode.Update;
    enum UpdateMode
    {
        Update,
        LateUpdate,
        FixedUpdate
    }
    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target set for SimpleFollow on object " + this.gameObject.name, this);
            this.enabled = false;
            return;
        }
        offset = this.transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (updateMode == UpdateMode.Update)
        {
            this.transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate)
        {
            this.transform.position = target.position + offset;
        }
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate)
        {
            this.transform.position = target.position + offset;
        }
    }
}
