using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class KillPlane : MonoBehaviour
{
    public float interval = 0.1f;
    public bool ignorePlayer = false;
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
            Actor[] actors = FindObjectsOfType<Actor>();
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
            BreakableObject[] breakableObjects = FindObjectsOfType<BreakableObject>();
            foreach (BreakableObject breakable in breakableObjects)
            {
                if (!plane.GetSide(breakable.transform.position) && IsWithinPlane(breakable.transform.position))
                {
                    breakable.BreakObject();
                    Debug.Log("killplaned: " + breakable);
                }
            }
        }
    }

    bool IsWithinPlane(Vector3 position)
    {
        Vector3 adjustedPosition = position - this.transform.position;
        if (Mathf.Abs(adjustedPosition.x) > bounds.extents.x) return false;
        if (Mathf.Abs(adjustedPosition.z) > bounds.extents.z) return false;
        return true;
    }
}
