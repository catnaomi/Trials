using Cinemachine;
using CustomUtilities;
using System;
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

    public int index = 0;
    public bool lockedOn;
    public GameObject currentTarget;

    CinemachineTargetGroup cmtg;

    Dictionary<AxisUtilities.AxisDirection, Transform> directionToTarget;

    public Transform upTarget;
    public Transform downTarget;
    public Transform leftTarget;
    public Transform rightTarget;

    List<Ray> rays;


    public bool handleUI = true;
    public GameObject targetGraphic;
    public GameObject upGraphic;
    public GameObject downGraphic;
    public GameObject leftGraphic;
    public GameObject rightGraphic;
    public Vector3 uiOffset;
    public bool billboard = true;

    // Start is called before the first frame update
    void Start()
    {
        targets = new List<GameObject>();
        cmtg = GetComponent<CinemachineTargetGroup>();
        //InputHandler.main.SecondaryStickFlick.AddListener(SwitchTargets);
        StartCoroutine(UpdateTargets());
        lockedOn = false;

        player.toggleTarget.AddListener(ToggleTarget);
        player.secondaryStickFlick.AddListener(SwitchTargets);

        if (handleUI)
        {
            targetGraphic = GameObject.Instantiate(targetGraphic);
            upGraphic = GameObject.Instantiate(upGraphic);
            downGraphic = GameObject.Instantiate(downGraphic);
            leftGraphic = GameObject.Instantiate(leftGraphic);
            rightGraphic = GameObject.Instantiate(rightGraphic);
        }
    }

    IEnumerator UpdateTargets()
    {
        while (true)
        {
            GameObject[] allLockTargets = GameObject.FindGameObjectsWithTag("LockTarget");

            targets.Clear();

            List<GameObject> invalidTargets = new List<GameObject>();

            rays = new List<Ray>();
            foreach (GameObject target in allLockTargets)
            {
                float playerDist = Vector3.Distance(target.transform.position, player.centerTransform.position);
                float camDist = Vector3.Distance(target.transform.position, cam.transform.position);

                Ray pRay = new Ray(player.centerTransform.position, (target.transform.position - player.centerTransform.position));
                Ray cRay = new Ray(cam.transform.position, (target.transform.position - cam.transform.position));

                bool playerTerrainBlocked = Physics.Raycast(pRay, playerDist, LayerMask.GetMask("Terrain"));
                bool camTerrainBlocked = Physics.Raycast(cRay, camDist, LayerMask.GetMask("Terrain"));

                bool playerInRange = playerDist < maxPlayerDistance;
                bool camInRange = camDist < maxCamDistance;

                Vector3 vpp = Camera.main.WorldToViewportPoint(target.transform.position);
                bool onScreen = (Mathf.Abs(vpp.x) <= 1f) && (Mathf.Abs(vpp.y) <= 1f) && (vpp.z >= 0);
                bool invalid = !onScreen || ((!playerInRange || playerTerrainBlocked) && (!camInRange || camTerrainBlocked));


                if (!invalid)
                {
                    rays.Add(pRay);
                    rays.Add(cRay);
                }

                //Debug.Log(string.Format("object:{0}, pterr:{1}, prang:{2}, cterr:{3}, crang:{4}, invalid:{5}", target.name, playerTerrainBlocked, (int)playerDist, camTerrainBlocked, (int)camDist, invalid));
                if (invalid)
                {
                    invalidTargets.Add(currentTarget);
                }
                else
                {
                    targets.Add(target);
                }
            }

            if (targets.Count > 0)
            {
                targets.Sort((a,b) => {
                    Vector3 aDist = Camera.main.WorldToViewportPoint(a.transform.position);
                    Vector3 bDist = Camera.main.WorldToViewportPoint(b.transform.position);
                    return Math.Sign(bDist.magnitude - aDist.magnitude);
                    if (aDist.z < 0 && bDist.z < 0)
                    {
                        return 0;
                    }
                    else if (aDist.z < 0)
                    {
                        return -1;
                    }
                    else if (bDist.z < 0)
                    {
                        return 1;
                    }
                    else
                    {
                        
                    }
                });
            }
            yield return new WaitForSecondsRealtime(2f);
        }
    }

    // Update is called once per frame
    void Update()
    {      

        if (false)//Input.GetButtonDown("Target"))
        {
            Debug.Log("target?");
            if (!lockedOn && targets.Count > 0)
            {
                lockedOn = true;
                SetTarget(targets[0]);
                UpdateDirections();
            }
            else
            {
                lockedOn = false;
                SetTarget(null);
            }
        }


        if (currentTarget != null && player.GetCombatTarget() != currentTarget)
        {
            if (cmtg.m_Targets.Length > 1)
            {
                cmtg.m_Targets[1].target = currentTarget.transform;
            }
            else
            {
                cmtg.AddMember(currentTarget.transform, 1f, 2f);
            }

            //cmtg.AddMember(currentTarget.transform, 1f, 2f);
            player.SetCombatTarget(currentTarget);
        }
        else if (currentTarget == null && player.GetCombatTarget() != null)
        {
            player.SetCombatTarget(null);
            lockedOn = false;
        }
    }

    private void UpdateDirections()
    {
        if (!lockedOn || currentTarget == null) return;
        List<Transform> otherTransforms = new List<Transform>();
        foreach (GameObject target in targets)
        {
            if (target != currentTarget && target != null)
            {
                otherTransforms.Add(target.transform);
            }
        }

        directionToTarget = AxisUtilities.MapTransformsToAxisDirections(cam.transform, currentTarget.transform.position, otherTransforms);

        upTarget = directionToTarget[AxisUtilities.AxisDirection.Up];
        downTarget = directionToTarget[AxisUtilities.AxisDirection.Down];
        leftTarget = directionToTarget[AxisUtilities.AxisDirection.Left];
        rightTarget = directionToTarget[AxisUtilities.AxisDirection.Right];
    }

    void ToggleTarget()
    {
        //Debug.Log("target?");
        if (!lockedOn && targets.Count > 0)
        {
            lockedOn = true;
            SetTarget(targets[0]);
            UpdateDirections();
        }
        else
        {
            lockedOn = false;
            SetTarget(null);
        }
    }
    void SwitchTargets()
    {
        if (lockedOn)
        {
            AxisUtilities.AxisDirection direction = AxisUtilities.DirectionToAxisDirection(player.look, "HORIZONTAL", "VERTICAL");
            //AxisUtilities.AxisDirection direction = AxisUtilities.InvertAxis(InputHandler.main.SecondaryFlickDirection, false, false, false);
            //Debug.Log("switch:" + direction + "--" + player.look);
            if (directionToTarget.TryGetValue(direction, out Transform target) && target != null)
            {
                SetTarget(target.gameObject);
                UpdateDirections();
            }            
        }
    }

    void SetTarget(GameObject t)
    {
        currentTarget = t;
        //player.SetCombatTarget(currentTarget);
    }

    private void OnGUI()
    {
        if (handleUI)
        {
            HandleTargetGraphic(targetGraphic, currentTarget);
            HandleTargetGraphic(upGraphic, upTarget);
            HandleTargetGraphic(downGraphic, downTarget);
            HandleTargetGraphic(leftGraphic, leftTarget);
            HandleTargetGraphic(rightGraphic, rightTarget);
            
        }
    }

    private bool HandleTargetGraphic(GameObject graphic, Transform target)
    {
        if (target == null || PlayerActor.player.GetCombatTarget() == null)
        {
            graphic.SetActive(false);
            return false;
        }
        graphic.SetActive(true);
        graphic.transform.position = target.position + uiOffset;
        return true;
    }

    private bool HandleTargetGraphic(GameObject graphic, GameObject target)
    {
        if (target == null || PlayerActor.player.GetCombatTarget() == null)
        {
            graphic.SetActive(false);
            return false;
        }
        graphic.SetActive(true);
        graphic.transform.position = target.transform.position + uiOffset;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        /*
        Gizmos.color = Color.red;
        foreach (Ray ray in rays)
        {
            Gizmos.DrawRay(ray);
        }
        */
    }
}
