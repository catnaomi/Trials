using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AllDestroyedTrigger : MonoBehaviour
{
    public GameObject[] watchList;
    [ReadOnly, SerializeField] int remaining;
    public float watchRefreshTime = 1f;
    public UnityEvent OnOneDestroy;
    public UnityEvent OnAllDestroy;
    // Start is called before the first frame update
    void Start()
    {
        remaining = watchList.Length;
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
                }
            }
            if (currentRemaining < remaining)
            {
                OnOneDestroy.Invoke();
                if (currentRemaining == 0)
                {
                    OnAllDestroy.Invoke();
                }
            }
            remaining = currentRemaining;
            yield return new WaitForSeconds(watchRefreshTime);
        }
    }
}
