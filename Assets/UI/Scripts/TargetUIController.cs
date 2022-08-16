using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetUIController : MonoBehaviour
{
    public PlayerTargetManager targetManager;
    public GameObject targetInfoPrefab;

    public int maxTargetInfos = 10;

    public List<TargetUIInfo> infos;
    List<GameObject> assignedTargets;
    List<GameObject> newTargets;
    List<GameObject> removedTargets;
    private void Start()
    {
        infos = new List<TargetUIInfo>();

        for (int i = 0; i < maxTargetInfos; i++)
        {
            GameObject obj = Instantiate(targetInfoPrefab, this.transform);
            TargetUIInfo info = obj.GetComponent<TargetUIInfo>();
            info.controller = this;
            infos.Add(info);
        }
        assignedTargets = new List<GameObject>();
        newTargets = new List<GameObject>();
        removedTargets = new List<GameObject>();
        targetManager.OnTargetUpdate.AddListener(UpdateTargets);
    }

    private void UpdateTargets()
    {
        newTargets.Clear();
        removedTargets.Clear();

        foreach (GameObject target in targetManager.targets)
        {
            if (!assignedTargets.Contains(target))
            {
                newTargets.Add(target);
            }
        }
        foreach (GameObject oldTarget in assignedTargets)
        {
            if (!targetManager.targets.Contains(oldTarget))
            {
                removedTargets.Add(oldTarget);
            }
        }
        for (int i = 0; i < maxTargetInfos; i++)
        {
            TargetUIInfo info = infos[i];

            if (info.target == null && newTargets.Count > 0)
            {
                info.SetTarget(newTargets[0]);
                newTargets.RemoveAt(0);
            }
            else if (removedTargets.Contains(info.target))
            {
                info.SetTarget(null);
            }
        }
        assignedTargets.Clear();
        assignedTargets.AddRange(targetManager.targets);
    }

    public bool IsTargeting()
    {
        return targetManager.lockedOn;
    }
    
    public bool IsActiveTarget(GameObject target)
    {
        return targetManager.currentTarget == target;
    }

    TargetUIInfo GetInfoFromTarget(GameObject target)
    {
        foreach (TargetUIInfo info in infos)
        {
            if (info.target == target)
            {
                return info;
            }
        }
        return null;
    }
}
