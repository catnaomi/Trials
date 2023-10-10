using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AllDestroyedTrigger : MonoBehaviour
{
    public GameObject[] watchList;
    [ReadOnly, SerializeField] int remaining;
    public float watchRefreshTime = 1f;
    public UnityEvent OnOneDestroy;
    public UnityEvent OnAllDestroy;
    public List<GameObject> removedObjects;
    // Start is called before the first frame update
    void Start()
    {
        remaining = watchList.Length;
        removedObjects = new List<GameObject>();
        StartCoroutine(WatchCoroutine());
    }

    IEnumerator WatchCoroutine()
    {
        while (remaining > 0)
        {
            int currentRemaining = watchList.Length;
            foreach (GameObject obj in watchList)
            {
                if (obj == null)
                {
                    currentRemaining--;
                    removedObjects.Add(null);
                }
                else if (!obj.activeSelf)
                {
                    //currentRemaining--;
                    //removedObjects.Add(obj);
                }
            }
            if (removedObjects.Count > 0)
            {
                watchList = watchList.Except(removedObjects).ToArray();
                removedObjects.Clear();
            }
            if (currentRemaining < remaining)
            {
                OnOneDestroy.Invoke();
                if (currentRemaining <= 0)
                {
                    OnAllDestroy.Invoke();
                }
            }
            remaining = currentRemaining;
            yield return new WaitForSeconds(watchRefreshTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach(GameObject obj in watchList)
        {
            if (obj != null)
            {
                Gizmos.color = Color.red;
                DrawArrow.ForGizmoTwoPoints(this.transform.position, obj.transform.position);
            }
        }
    }
}
