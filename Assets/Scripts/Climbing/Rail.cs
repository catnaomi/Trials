using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections;

public class Rail : ClimbDetector
{
    readonly float MAX_AUTO_LEDGE_DISTANCE = 1f;
    Rigidbody rail;

    public Transform snap;
    [Range(-1f, 1f)]
    public float snapPoint;
    [SerializeField, ReadOnly]
    private float length;
    [SerializeField, ReadOnly]
    private float dot;

    public bool linkedLeft;
    public Rail left;
    public bool linkedRight;
    public Rail right;
    bool isLeftLinkValid;
    bool isRightLinkValid;
    // Use this for initialization
    void Awake()
    {
        rail = this.GetComponent<Rigidbody>();
        collider = this.GetComponent<Collider>();
        //ValidateLinks();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDisabled) return;
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.SetRail(this);
            inUse = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            if (!player.IsClimbing())
            {
                player.UnsetClimb(this);
                inUse = false;
            }
            //inUse = false;
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

    public override Vector3 GetClimbHeading()
    {
        return this.transform.right;
    }
    public Vector3 GetSnapPoint(float climberWidth)
    {
        return GetSnapPointForValue(climberWidth, this.snapPoint);
    }

    public Vector3 GetSnapPointForValue(float climberWidth, float snapPoint)
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
            player.SetRail(left);
            left.snapPoint = -0.9f;
            inUse = false;
            return left.GetSnapPoint(climberWidth);
        }
        if (snapPoint < -1 && linkedRight && dir < 0)
        {
            player.SetRail(right);
            right.snapPoint = 0.9f;
            inUse = false;
            return right.GetSnapPoint(climberWidth);
        }
        snapPoint = Mathf.Clamp(snapPoint, -1f, 1f);

        return GetSnapPoint(climberWidth);
    }

    private readonly float debug_climberwidth = 0.5f;
    public Vector3 GetLeftEnd()
    {
        return GetSnapPointForValue(debug_climberwidth, 1f);
    }

    public Vector3 GetRightEnd()
    {
        return GetSnapPointForValue(debug_climberwidth, -1f);
    }
    private void OnValidate()
    {
        //ValidateLinks();
    }

    public void ValidateLinks()
    {
        isLeftLinkValid = true;
        if (linkedLeft)
        {
            if (left == null)
            {
                Debug.LogWarning(string.Format("{0} is linked on left side with no connecting ledge", this));
                isLeftLinkValid = false;
            }
            else if (!left.linkedRight || left.right != this)
            {
                Debug.LogWarning(string.Format("{0} not properly linked to {1}", this, left));
                isLeftLinkValid = false;
            }
            else if (left == this)
            {
                Debug.LogWarning(string.Format("{0} is linked to self", this));
                isLeftLinkValid = false;
            }
        }
        isRightLinkValid = true;
        if (linkedRight)
        {
            if (right == null)
            {
                Debug.LogWarning(string.Format("{0} is linked on right side with no connecting ledge", this));
                isRightLinkValid = false;
            }
            else if (!right.linkedLeft || right.left != this)
            {
                Debug.LogWarning(string.Format("{0} not properly linked to {1}", this, right));
                isRightLinkValid = false;
            }
            else if (right == this)
            {
                Debug.LogWarning(string.Format("{0} is linked to self", this));
                isRightLinkValid = false;
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (linkedLeft)
        {
            Gizmos.color = isLeftLinkValid ? Color.yellow : Color.magenta;
            if (left != null)
            {
                Gizmos.DrawLine(this.GetLeftEnd() + Vector3.up * 0.01f, left.GetRightEnd());
            }
            Gizmos.DrawWireSphere(GetLeftEnd(), 0.1f);
        }
        if (linkedRight)
        {
            Gizmos.color = isRightLinkValid ? Color.cyan : Color.magenta;
            if (right != null)
            {
                Gizmos.DrawLine(this.GetRightEnd() + Vector3.up * -0.01f, right.GetLeftEnd());
            }
            Gizmos.DrawWireSphere(GetRightEnd(), 0.1f);
        }
    }

    public static void AutoConnectLedges()
    {

        Ledge[] ledges = FindObjectsOfType<Ledge>();


        Debug.Log("validating ledge links");
        // check validity of all ledge links before editing
        foreach (Ledge validatingLedge in ledges)
        {

            validatingLedge.ValidateLinks();
        }

        Debug.Log("auto linking ledges");

        foreach (Ledge workingLedge in ledges)
        {
            workingLedge.AutoConnectLedge();
        }
    }

    public static void ValidateAllLedges()
    {
        Ledge[] ledges = FindObjectsOfType<Ledge>();


        Debug.Log("validating ledge links");
        // check validity of all ledge links before editing
        foreach (Ledge validatingLedge in ledges)
        {
            validatingLedge.ValidateLinks();
        }
    }

    public void AutoConnectLedge()
    {

        bool shouldCheckLeft = !this.isLeftLinkValid || !this.linkedLeft;
        bool shouldCheckRight = !this.isRightLinkValid || !this.linkedRight;

        if (shouldCheckLeft || shouldCheckRight)
        {
            Rail[] ledges = FindObjectsOfType<Rail>();
            bool foundLeftLink = false;
            bool foundRightLink = false;
            // left side
            foreach (Rail connectingLedge in ledges)
            {
                if (connectingLedge == this) continue;
                if (shouldCheckLeft && !foundLeftLink)
                {
                    if (Vector3.Distance(this.GetLeftEnd(), connectingLedge.GetRightEnd()) <= MAX_AUTO_LEDGE_DISTANCE)
                    {
                        this.left = connectingLedge;
                        connectingLedge.right = this;
                        this.isLeftLinkValid = true;
                        connectingLedge.isRightLinkValid = true;
                        foundLeftLink = true;
                        this.linkedLeft = true;
                        connectingLedge.linkedRight = true;
                    }
                }
                else if (shouldCheckRight && !foundRightLink)
                {
                    if (Vector3.Distance(this.GetRightEnd(), connectingLedge.GetLeftEnd()) <= MAX_AUTO_LEDGE_DISTANCE)
                    {
                        this.right = connectingLedge;
                        connectingLedge.left = this;
                        this.isRightLinkValid = true;
                        connectingLedge.isLeftLinkValid = true;
                        foundRightLink = true;
                        this.linkedRight = true;
                        connectingLedge.linkedLeft = true;
                    }
                }
            }
            if (shouldCheckLeft && !foundLeftLink)
            {
                this.linkedLeft = false;
            }
            if (shouldCheckRight && !foundRightLink)
            {
                this.linkedRight = false;
            }
        }

    }

    public void ValidateThenAutoConnectLedge()
    {
        //ValidateLinks();
        //AutoConnectLedge();
    }
}
