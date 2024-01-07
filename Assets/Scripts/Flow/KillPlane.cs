using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class KillPlane : MonoBehaviour
{
    public float interval = 0.1f;
    public float maximumHeightDifference = 100;
    public bool ignorePlayer = false;
    public bool ignoreObjects = true;
    public UnityEvent OnKillPlayer;
    Plane plane;
    Bounds bounds;
    // Start is called before the first frame update
    void Start()
    {
        plane = new Plane(this.transform.up, this.transform.position);
        bounds = this.GetComponent<Renderer>().bounds;
        StartCoroutine("CheckKillPlane");
    }

    IEnumerator CheckKillPlane()
    {
        while (true)
        {


            yield return new WaitForSecondsRealtime(interval);
            if (this.gameObject.scene != SceneManager.GetActiveScene()) continue;
            List<Actor> actors = ActorManager.GetActors();
            foreach (Actor actor in actors)
            {
                if (!actor.IsAlive()) continue;
                if (actor is PlayerActor && ignorePlayer) continue;
                if (actor is not PlayerActor && actor.gameObject.scene != this.gameObject.scene) continue;
                if (!plane.GetSide(actor.transform.position) && IsWithinPlane(actor.transform.position))
                {
                    actor.OnFallOffMap();
                    if (actor is PlayerActor)
                    {
                        OnKillPlayer.Invoke();
                    }
                    Debug.Log("killplaned: " + actor);
                }
                //yield return null;
            }
            if (!ignoreObjects)
            {
                BreakableObject[] breakableObjects = FindObjectsOfType<BreakableObject>();
                foreach (BreakableObject breakable in breakableObjects)
                {
                    if (breakable.gameObject.scene != this.gameObject.scene) continue;
                    if (!plane.GetSide(breakable.transform.position) && IsWithinPlane(breakable.transform.position))
                    {
                        breakable.BreakObject();
                        Debug.Log("killplaned: " + breakable);
                    }
                }
            }
        }
    }

    bool IsWithinPlane(Vector3 position)
    {
        Vector3 adjustedPosition = position - this.transform.position;
        if (Mathf.Abs(adjustedPosition.x) > bounds.extents.x) return false;
        if (Mathf.Abs(adjustedPosition.z) > bounds.extents.z) return false;
        if (Mathf.Abs(adjustedPosition.y) > maximumHeightDifference) return false;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Bounds gizBounds = (!Application.isPlaying) ? this.GetComponent<Renderer>().bounds : bounds;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector3 center = gizBounds.center;
        center.y -= maximumHeightDifference * 0.5f;
        Gizmos.DrawCube(center, new Vector3(gizBounds.extents.x * 2, maximumHeightDifference, gizBounds.extents.z * 2));
    }
}
