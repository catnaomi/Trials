using UnityEngine;
using System.Collections;

public class Ledge : ClimbDetector
{
    Rigidbody ledge;

    public Transform snap;
    [Range(-1f,1f)]
    public float snapPoint;
    [SerializeField, ReadOnly]
    private float length;
    [SerializeField, ReadOnly]
    private float dot;

    public bool linkedLeft;
    public Ledge left;
    public bool linkedRight;
    public Ledge right;
    // Use this for initialization
    void Awake()
    {
        ledge = this.GetComponent<Rigidbody>();
        collider = ledge.GetComponent<Collider>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.SetLedge(this);
            inUse = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.UnsetClimb(this);
            inUse = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, this.transform.forward);
    }
    public float GetLength()
    {
        length = snap.lossyScale.x;
        return length;
    }

    public Vector3 GetSnapPoint(float climberWidth)
    {
        return snap.transform.position + snap.transform.right * -snapPoint * (GetLength() - climberWidth) * 0.5f;
    }


    public Vector3 GetSnapPointDot(float climberWidth, Vector3 climberPosition, PlayerActor player, int dir)
    {
        //if (linkedLeft) left.snapPoint = -0.9f;
        //if (linkedRight) right.snapPoint = 0.9f;
        dot = Vector3.Dot(snap.transform.right, snap.transform.position - climberPosition);
        
        snapPoint = dot * 2f / (GetLength() - climberWidth);
        //Debug.Log("" + (snap.transform.position - climberPosition) + " dot " + snap.transform.right + " = " + dot + " snap: " + snapPoint);

        if (snapPoint > 1 && linkedLeft && dir > 0)
        {
            player.SetLedge(left);
            left.snapPoint = -0.9f;
            inUse = false;
            return left.GetSnapPoint(climberWidth);
        }
        if (snapPoint < -1 && linkedRight && dir < 0)
        {
            player.SetLedge(right);
            right.snapPoint = 0.9f;
            inUse = false;
            return right.GetSnapPoint(climberWidth);
        }
        snapPoint = Mathf.Clamp(snapPoint, -1f, 1f);
        
        return GetSnapPoint(climberWidth);
    }

    private void OnValidate()
    {
        if (linkedLeft && left != null)
        {
            if (!left.linkedRight || left.right != this)
            {
                Debug.LogWarning(string.Format("{0} not properly linked to {1}", this, left));
            }
        }
        if (linkedRight && right != null)
        {
            if (!right.linkedLeft || right.left != this)
            {
                Debug.LogWarning(string.Format("{0} not properly linked to {1}", this, right));
            }
        }
    }
}
