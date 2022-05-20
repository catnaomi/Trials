using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlane : MonoBehaviour
{
    public float interval = 0.1f;
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
                if (actor is not PlayerActor && actor.gameObject.scene != this.gameObject.scene) continue;
                if (!plane.GetSide(actor.transform.position) && IsWithinPlane(actor.transform.position))
                {
                    actor.OnFallOffMap();
                    Debug.Log("killplaned: " + actor);
                }
                //yield return null;
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
