using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumRigidbody : MonoBehaviour
{
    public float speed = 1f;
    public float angle = 30f;
    public float pendulumHeight = 10f;
    public Rigidbody rigidbody;
    RigidbodyTimeTravelHandler timeTravelHandler;
    bool usingTimeTravel;
    Vector3 anchorPosition;
    // Start is called before the first frame update
    void Start()
    {
        if (rigidbody == null) rigidbody = this.GetComponent<Rigidbody>();
        timeTravelHandler = rigidbody.GetComponent<RigidbodyTimeTravelHandler>();
        usingTimeTravel = timeTravelHandler != null;
        anchorPosition = this.transform.position + Vector3.up * pendulumHeight;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float rotAngle = Mathf.Sin(GetFixedTime() * speed) * angle;

        Vector3 dir = Quaternion.AngleAxis(rotAngle, this.transform.forward) * (Vector3.up * pendulumHeight);

        rigidbody.MovePosition(anchorPosition + -dir);
        rigidbody.MoveRotation(Quaternion.LookRotation(this.transform.forward, Quaternion.AngleAxis(rotAngle, this.transform.forward) * Vector3.up));
    }

    float GetFixedDeltaTime()
    {
        return !usingTimeTravel ? Time.fixedDeltaTime : TimeTravelController.GetTimeAffectedFixedDeltaTime();
    }

    float GetFixedTime()
    {
        return !usingTimeTravel ? Time.fixedTime : timeTravelHandler.GetFixedTime();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;

        if (Application.isPlaying)
        {
            Gizmos.DrawRay(anchorPosition, Quaternion.AngleAxis(angle, this.transform.forward) * (-Vector3.up * 10f));
            Gizmos.DrawRay(anchorPosition, Quaternion.AngleAxis(-angle, this.transform.forward) * (-Vector3.up * 10f));
        }
        else
        {
            Gizmos.DrawRay(this.transform.position + Vector3.up * pendulumHeight, Quaternion.AngleAxis(angle, this.transform.forward) * (-Vector3.up * 10f));
            Gizmos.DrawRay(this.transform.position + Vector3.up * pendulumHeight, Quaternion.AngleAxis(-angle, this.transform.forward) * (-Vector3.up * 10f));
        }
        

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(this.transform.position, this.transform.forward);
    }
}
