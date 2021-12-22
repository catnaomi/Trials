using UnityEngine;
using System.Collections;

public class Ladder : ClimbDetector
{
    Rigidbody ladder;

    public Transform snap;
    public Transform endpoint;
    [Range(-1f, 1f)]
    public float snapPoint;
    [SerializeField, ReadOnly]
    private float height;
    [SerializeField, ReadOnly]
    private float dot;

    public bool linkedDown;
    public Ladder down;
    public bool linkedUp;
    public Ladder up;
    // Use this for initialization
    void Awake()
    {
        ladder = this.GetComponent<Rigidbody>();
        collider = ladder.GetComponent<Collider>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.SetLadder(this);
            inUse = true;
        }
        else if (other.transform.root.TryGetComponent<PlayerMovementController>(out PlayerMovementController playermov))
        {
            playermov.SetLadder(this);
            inUse = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerMovementController>(out PlayerMovementController playermov))
        {
            if (playermov.currentClimb == this)
            {
                playermov.UnsnapLedge();
            }
            inUse = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, this.transform.up);
    }
    public float GetHeight()
    {
        height = snap.lossyScale.y;
        return height;
    }

    public Vector3 GetSnapPoint(float climberHeight)
    {
        return snap.transform.position + (snap.transform.up * -snapPoint * (GetHeight() - climberHeight) * 0.5f);
    }

    public Vector3 GetSnapPointDot(float climberHeight, Vector3 climberPosition, PlayerMovementController player, int dir)
    {
        //if (linkedLeft) left.snapPoint = -0.9f;
        //if (linkedRight) right.snapPoint = 0.9f;
        dot = Vector3.Dot(snap.transform.up, snap.transform.position - climberPosition);

        snapPoint = dot * 2f / (GetHeight() - climberHeight);
        //Debug.Log("" + (snap.transform.position - climberPosition) + " dot " + snap.transform.up + " = " + dot + " snap: " + snapPoint);
        float offset = climberHeight / GetHeight();
        if (snapPoint > 1 && linkedDown && dir > 0)
        {
            player.SetLadder(down);
            down.snapPoint = -0.9f;
            inUse = false;
            return down.GetSnapPoint(climberHeight);
        }
        if (snapPoint < -1 && linkedUp && dir < 0)
        {
            player.SetLadder(up);
            up.snapPoint = 0.9f;
            inUse = false;
            return up.GetSnapPoint(climberHeight);
        }
        snapPoint = Mathf.Clamp(snapPoint, -1f,1f);

        return GetSnapPoint(climberHeight);
    }
}
