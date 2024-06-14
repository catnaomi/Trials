using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnObject : MonoBehaviour, IEventVisualizable
{

    public GameObject obj;
    [SerializeField] float delay;
    [SerializeField, ReadOnly] bool timerStarted;
    Vector3 targetPosition;
    Quaternion targetRotation;
    GameObject workingInstance;
    GameObject copy;
    
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = obj.transform.position;
        targetRotation = obj.transform.rotation;
        copy = Instantiate(obj, this.transform.position, this.transform.rotation);
        copy.SetActive(false);
        workingInstance = obj;
    }

    // Update is called once per frame
    void Update()
    {
        if (workingInstance == null && !timerStarted)
        {
            StartRespawnTimer();
        }
    }

    void StartRespawnTimer()
    {
        timerStarted = true;
        this.StartTimer(delay, Respawn);
    }

    public void Respawn()
    {
        workingInstance = Instantiate(copy, targetPosition, targetRotation);
        workingInstance.SetActive(true);
        timerStarted = false;
    }

    public GameObject[] GetEventTargets()
    {
        return new GameObject[] { obj };
    }

}
