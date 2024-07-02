using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalPlane : MonoBehaviour
{
    Plane plane;
    Bounds bounds;
    Renderer renderer;
    public bool toWorld2 = false;
    [ReadOnly, SerializeField] private bool withinBounds;
    [ReadOnly, SerializeField] private bool playerNegative;
    [ReadOnly, SerializeField] private bool cameraNegative;
    [ReadOnly, SerializeField] private bool nearPlaneClipping;
    public float camErrorDist = 0.75f;
    public float range = 10f;

    void Start()
    {
        plane = new Plane(transform.up, transform.position);
        renderer = GetComponent<Renderer>();
        bounds = renderer.bounds;
    }

    private void Update()
    {
        CheckPortalPlane();
    }

    void CheckPortalPlane()
    {
        if (gameObject.scene != SceneManager.GetActiveScene()) return;
        withinBounds = Vector3.Distance(PlayerActor.player.transform.position, transform.position) < range || Vector3.Distance(Camera.main.transform.position, transform.position) < range;//IsWithinPlane(PlayerActor.player.transform.position);// IsWithinPlane(Camera.main.transform.position + Camera.main.nearClipPlane * Camera.main.transform.forward);
        bool swap = false;
        bool render = true;
        if (withinBounds)
        {
            bool inWorldOfThisPortal = PortalManager.instance.inWorld2 != toWorld2;
            playerNegative = !plane.GetSide(PlayerActor.player.transform.position);
            cameraNegative = !plane.GetSide(Camera.main.transform.position+ Camera.main.nearClipPlane* Camera.main.transform.forward);
            nearPlaneClipping = IsNearPlaneClipping();
            if (nearPlaneClipping)
            {
                render = false;
                if (playerNegative && inWorldOfThisPortal)
                {
                    swap = true;
                }
                else if (!playerNegative && !inWorldOfThisPortal)
                {
                    swap = true;
                }
            }
            else if (playerNegative && cameraNegative && inWorldOfThisPortal)
            {
                swap = true;
            }
            else if (playerNegative && !cameraNegative)
            {
                bool visibleThroughPortal = StillVisibleThroughPortal(PlayerActor.player.transform.position, Camera.main.transform.position);
                if (visibleThroughPortal && !inWorldOfThisPortal)
                {
                    swap = true;
                }
                else if (!visibleThroughPortal && inWorldOfThisPortal)
                {
                    swap = true;
                }
            }
        }
        if (swap)
        {
            PortalManager.instance.Swap();
        }
        renderer.enabled = render;
    }

    bool IsWithinPlane(Vector3 position)
    {
        Ray posray = new Ray(position, transform.up);
        Ray negray = new Ray(position, -transform.up);
        bool intersect = bounds.IntersectRay(posray) || bounds.IntersectRay(negray);
        return intersect;
    }

    bool StillVisibleThroughPortal(Vector3 playerPos, Vector3 cameraPos)
    {
        Ray ray = new Ray(cameraPos, playerPos - cameraPos);
        Debug.DrawRay(cameraPos, playerPos - cameraPos, Color.magenta);
        bool intersectsBounds = bounds.IntersectRay(ray);
        bool intersectsPlane = plane.Raycast(ray, out float distance);
        bool withinDistance = distance <= (playerPos - cameraPos).magnitude;
        return intersectsBounds && intersectsPlane && withinDistance;
    }

    bool IsNearPlaneClipping()
    {
        float distToPlane = Mathf.Abs(plane.GetDistanceToPoint(Camera.main.transform.position));
        return (distToPlane < camErrorDist) && IsWithinPlane(Camera.main.transform.position);
    }
}
