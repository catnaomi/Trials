using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetManager : MonoBehaviour
{
    public PlayerActor player;
    public Camera cam;
    public float maxPlayerDistance = 20f;
    public float maxCamDistance = 20f;
    public List<GameObject> targets;
    public Dictionary<GameObject, Transform> targetMidpoints;

    int index = 0;
    public bool shiftOnDebug = true;
    public GameObject currentTarget;

    CinemachineTargetGroup cmtg;

    // Start is called before the first frame update
    void Start()
    {
        targets = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] allLockTargets = GameObject.FindGameObjectsWithTag("LockTarget");

        foreach (GameObject lockTarget in allLockTargets)
        {
            if (!targets.Contains(lockTarget))
            {
                targets.Add(lockTarget);
            }
        }

        if (Input.GetButtonDown("Debug"))
        {
            index++;
        }
        if (index > targets.Count)
        {
            index = 0;
        }
        if (targets.Count > 0)
        {
            currentTarget = targets[index];
        }
        else
        {
            currentTarget = null;
        }
    }
}
