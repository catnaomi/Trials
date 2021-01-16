using UnityEngine;
using System.Collections;

public class Ladder : ClimbDetector
{
    public float TOP_HEIGHT_MARGIN = 1.5f;
    public float BOTTOM_HEIGHT_MARGIN = 0.25f;
    Rigidbody ladder;
    public float height = -1;
    public float currentHeight; // center = 0
    public bool canDescend = true;
    public bool canAscend = true;
    // Use this for initialization
    void Awake()
    {
        ladder = this.GetComponent<Rigidbody>();
        collider = ladder.GetComponent<Collider>();

        if (height <= 0)
        {
            height = collider.bounds.size.y;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.SetLadder(this);
            inUse = true;
        }
    }
    public bool SetCurrentHeight(Vector3 position)
    {
        bool shouldDismount = false;

        currentHeight = position.y - this.collider.transform.position.y;

        canDescend = (currentHeight > (0 - (height / 2)) + BOTTOM_HEIGHT_MARGIN);
        canAscend = (currentHeight < ((height / 2)) - TOP_HEIGHT_MARGIN);

        shouldDismount = canAscend == false;
        return shouldDismount;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, this.transform.forward);

        if (height >= 0)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Vector3 climbablePreviewSize = this.collider.bounds.size;
            climbablePreviewSize.y = this.height - TOP_HEIGHT_MARGIN - BOTTOM_HEIGHT_MARGIN;
            Gizmos.DrawCube(this.transform.position + Vector3.up * (BOTTOM_HEIGHT_MARGIN - TOP_HEIGHT_MARGIN), climbablePreviewSize);
        }
    }
}
