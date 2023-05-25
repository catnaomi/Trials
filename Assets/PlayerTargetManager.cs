using Cinemachine;
using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerTargetManager : MonoBehaviour
{
    public PlayerActor player;
    public Transform playerHead;
    public Camera cam;
    public float maxPlayerDistance = 20f;
    public float maxCamDistance = 20f;
    public List<GameObject> targets;
    public LayerMask blocksTargetingMask;
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
    public float targetDelay;
    float invalidTime;
    public float invalidExpiryTime = 1f;
    bool targetHeldLastFrame;
    bool wasTargetBlock;
    [Header("Press To Change Target Settings")]
    public float maxChangeTargetDelay = 2f;
    public int changeTargetIndexOffset = 0;
    public float changeTargetResetDelay = 3f;
    float targetPressedClock;
    float targetReleasedClock;
    bool lockOnRelease;
    bool lockOnPress;
    public float targetChangeSpeed = 10f;
    public float targetChangeMaxDistance = 25f;
    public Transform targetAim;
    [Header("Free Look Control Settings")]
    public bool handleCamera;
    CinemachineFreeLook freeLook;
    public float freeLookMinDistance = 1f;
    public float freeLookDistanceOffset = 0f;
    public float freeLookDistanceMultiplier = 1f;
    public float freeLookMaxDistance = 25f;
    [Space(10)]
    public float botRigHeightMult = .3f;
    public float botRigHeightFixedHead = 0f;
    [Space(5)]
    public float midRigHeightMult = .5f;
    public float midRigHeightFixedHead = 0f;
    [Space(5)]
    public float topRigHeightMult = .7f;
    public float topRigHeightFixedHead = 0f;
    [Space(10)]
    public float radiusHeightMult = 1f;
    public bool targetingCylinder = false;
    [Space(10)]
    [SerializeField, ReadOnly] float radius;
    [Space(5)]
    [SerializeField, ReadOnly] float topHeight;
    [SerializeField, ReadOnly] float topRadius;
    [Space(5)]
    [SerializeField, ReadOnly] float midHeight;
    [SerializeField, ReadOnly] float midRadius;
    [Space(5)]
    [SerializeField, ReadOnly] float botHeight;
    [SerializeField, ReadOnly] float botRadius;
    public UnityEvent OnRecenterFree;
    public UnityEvent OnRecenterTarget;
    public UnityEvent OnTargetUpdate;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerActor.player;
        targets = new List<GameObject>();
        cmtg = GetComponent<CinemachineTargetGroup>();
        //InputHandler.main.SecondaryStickFlick.AddListener(SwitchTargets);
        if (cam == null)
        {
            cam = Camera.main;
        }
        StartCoroutine(UpdateTargets());
        lockedOn = false;

        SetupInputListeners();

        //player.toggleTarget.AddListener(ToggleTarget);
        //player.changeTarget.AddListener(SwitchTargets);

        if (handleUI)
        {
            targetGraphic = GameObject.Instantiate(targetGraphic);
            upGraphic = GameObject.Instantiate(upGraphic);
            downGraphic = GameObject.Instantiate(downGraphic);
            leftGraphic = GameObject.Instantiate(leftGraphic);
            rightGraphic = GameObject.Instantiate(rightGraphic);
        }
        invalidTime = invalidExpiryTime;
        freeLook = player.vcam.target.GetComponent<CinemachineFreeLook>();
        targetAim.position = Vector3.zero;
    }

    void SetupInputListeners()
    {
        PlayerInput inputs = player.GetComponent<PlayerInput>();

        inputs.actions["Target"].started += OnLockOn;
    }


    private void OnLockOn(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            lockOnPress = true;
        }
        else if (context.interaction is HoldInteraction)
        {
            lockOnRelease = true;
        }
    }

    IEnumerator UpdateTargets()
    {
        while (true)
        {
            GameObject[] allLockTargets = GameObject.FindGameObjectsWithTag("LockTarget");

            //targets.Clear();

            List<GameObject> validTargets = new List<GameObject>();
            List<GameObject> invalidTargets = new List<GameObject>();

            rays = new List<Ray>();
            foreach (GameObject target in allLockTargets)
            {
                float playerDist = Vector3.Distance(target.transform.position, player.positionReference.Spine.position);
                float camDist = Vector3.Distance(target.transform.position, cam.transform.position);

                Ray pRay = new Ray(player.positionReference.Spine.position, (target.transform.position - player.positionReference.Spine.position));
                Ray cRay = new Ray(cam.transform.position, (target.transform.position - cam.transform.position));

                bool playerTerrainBlocked = Physics.Raycast(pRay, playerDist, blocksTargetingMask);
                bool camTerrainBlocked = Physics.Raycast(cRay, camDist, blocksTargetingMask);

                bool playerInRange = playerDist < maxPlayerDistance;
                bool camInRange = camDist < maxCamDistance;

                Vector3 vpp = Camera.main.WorldToViewportPoint(target.transform.position);
                bool onScreen = (Mathf.Abs(vpp.x) <= 1f) && (Mathf.Abs(vpp.y) <= 1f) && (vpp.z >= 0);
                bool invalid = false;
                if (target != PlayerActor.player.GetCombatTarget())
                {
                    invalid = !onScreen || ((!playerInRange || playerTerrainBlocked) && (!camInRange || camTerrainBlocked));
                }

                if (target.transform.root.TryGetComponent<Actor>(out Actor actor))
                {
                    if (!actor.IsAlive())
                    {
                        invalid = true;
                    }
                }

                if (!invalid)
                {
                    rays.Add(pRay);
                    rays.Add(cRay);
                }

                //Debug.Log(string.Format("object:{0}, pterr:{1}, prang:{2}, cterr:{3}, crang:{4}, invalid:{5}", target.name, playerTerrainBlocked, (int)playerDist, camTerrainBlocked, (int)camDist, invalid));
                if (invalid)
                {
                    invalidTargets.Add(target);
                }
                else
                {
                    validTargets.Add(target);
                }
            }

            List<GameObject> targetsToDelete = new List<GameObject>();
            foreach (GameObject workingTarget in targets)
            {
                if (!validTargets.Contains(workingTarget))
                {
                    targetsToDelete.Add(workingTarget);
                }
            }
            targets = targets.Except(targetsToDelete).ToList();

            targets = targets.Union(validTargets).ToList();
            if (targets.Count > 0 && !(lockedOn))// || targetReleasedClock < changeTargetResetDelay))
            {
                targets.Sort((a,b) => {
                    /*
                    if (a == PlayerActor.player.GetCombatTarget())
                    {
                        return -1;
                    }
                    else if (b == PlayerActor.player.GetCombatTarget())
                    {
                        return 1;
                    }
                    else if (a == currentTarget && (lockedOn || targetReleasedClock < changeTargetResetDelay))
                    {
                        return -1;
                    }
                    else if (b == currentTarget && (lockedOn || targetReleasedClock < changeTargetResetDelay))
                    {
                        return 1;
                    }
                    */
                    return (int)Mathf.Sign(Vector3.Distance(player.transform.position, a.transform.position) - Vector3.Distance(player.transform.position, b.transform.position));
                    /*
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
                        
                    }*/
                });
                changeTargetIndexOffset = 0;
            }
            OnTargetUpdate.Invoke();
            yield return new WaitForSecondsRealtime(targetDelay);
        }
    }


    void Update()
    {
        /*
       
        bool targetDown = false;
        bool targetUp = false;
        bool targetHeld = PlayerActor.player.IsTargetHeld();
        bool targetIsBlock = PlayerActor.player.IsTargetHeld() && PlayerActor.player.IsBlockHeld();
        bool didSwitchTargetButtons = false;
        if (targetHeld)
        {
            if (!lockedOn && targets.Count > 0)
            {
                if (!targetHeldLastFrame)
                {
                    lockedOn = true;
                }
                //SetTarget(targets[0]);
                //UpdateDirections();
            }
            else if (targets.Count <= 0)
            {
                lockedOn = false;
                if (!targetHeldLastFrame)
                {
                    Recenter();
                }
                
            }

            if (!targetHeldLastFrame)
            {
                targetDown = true;
            }
            didSwitchTargetButtons = targetIsBlock != wasTargetBlock;
            wasTargetBlock = targetIsBlock;

        }
        else
        {
            if (targetHeldLastFrame)
            {
                targetUp = true;
            }
            if (player.IsInDialogue())
            {
                lockedOn = true;
            }
            else
            {
                lockedOn = false;
            }
            
        }


        if (!lockedOn || targetDown)
        {
            
            if (targetDown && targetReleasedClock < maxChangeTargetDelay && !didSwitchTargetButtons)
            {
                changeTargetIndexOffset++; // change targets
               
            }
            else if (!lockedOn && targetReleasedClock > changeTargetResetDelay)
            {
                changeTargetIndexOffset = 0;
            }
            if (targets.Count > 0)
            {
                SetTarget(targets[changeTargetIndexOffset % targets.Count]);
            }
        }
        
        
        if (lockedOn)
        {
            if (targetDown && !player.IsInDialogue())
            {
                player.SetCombatTarget(currentTarget);
            }
            else if (player.GetCombatTarget() != currentTarget)
            {
                SetTarget(player.GetCombatTarget());
            }
        } 
        else
        {
            player.SetCombatTarget(null);
          
        }
        *
        *
        */
        if (lockOnPress)
        {
            if (lockedOn)
            {
                if (targets.Count > 1)
                {
                    CycleTargets();
                }
                else
                {
                    RecenterTarget();
                }
            }
            else
            {
                if (!lockedOn && targets.Count > 0)
                {
                    lockedOn = true;
                }
                else if (targets.Count <= 0)
                {
                    lockedOn = false;
                    RecenterFree();
                }
            }
        }
        else if (lockOnRelease)
        {
            if (lockedOn)
            {
                lockedOn = false;
            }     
        }
        

        if (lockedOn)
        {
            if (lockOnPress && !player.IsInDialogue())
            {
                player.SetCombatTarget(currentTarget);
            }
            else if (player.GetCombatTarget() != currentTarget)
            {
                SetTarget(player.GetCombatTarget());
            }
        }
        else
        {
            player.SetCombatTarget(null);

        }
        lockOnRelease = false;
        lockOnPress = false;

        bool shouldExpire = false;
        if (currentTarget == player.GetCombatTarget() && !IsValidTarget(currentTarget))
        {    
            shouldExpire = true;
        }

        if (shouldExpire)
        {
            if (invalidTime > 0f)
            {
                invalidTime -= Time.deltaTime;
            }
            else
            {
                if (targets.Count > 0)
                {
                    changeTargetIndexOffset = 0;
                    while (changeTargetIndexOffset < targets.Count)
                    {
                        if (IsValidTarget(targets[changeTargetIndexOffset]))
                        {
                            break;
                        }
                        changeTargetIndexOffset++;
                    }
                    if (changeTargetIndexOffset < targets.Count)
                    {
                        SetTarget(targets[changeTargetIndexOffset % targets.Count]);
                        player.SetCombatTarget(currentTarget);
                    }
                    else
                    {
                        player.SetCombatTarget(null);
                        currentTarget = null;
                        lockedOn = false;
                    }
                }
                else
                {
                    player.SetCombatTarget(null);
                    currentTarget = null;
                    lockedOn = false;
                }       
            }
        }
        else
        {
            invalidTime = invalidExpiryTime;
        }

        /*
        if (cmtg.m_Targets.Length > 1)
        {
            cmtg.m_Targets[1].target = currentTarget != null ? currentTarget.transform : null;
        }
        else
        {
            cmtg.AddMember(currentTarget != null ? currentTarget.transform : null, 1f, 2f);
        }
        */

        if (currentTarget != null)
        {
            if (targetAim.position.magnitude < 0.01f || Vector3.Distance(targetAim.position, currentTarget.transform.position) > targetChangeMaxDistance)
            {
                targetAim.position = currentTarget.transform.position;
            }
            else
            {
                targetAim.position = Vector3.MoveTowards(targetAim.position, currentTarget.transform.position, targetChangeSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (!lockedOn && targetReleasedClock >= changeTargetResetDelay)
            {
                targetAim.position = Vector3.zero;
            }
        }
        if (handleCamera && lockedOn && cmtg.m_Targets.Length > 1)
        {
            this.transform.rotation = Quaternion.LookRotation(PlayerActor.player.transform.forward);
            UpdateFreeLook();
            
        }

        /*
        if (targetHeld)
        {
            if (targetDown)
            {
                targetPressedClock = 0f;
            }
            else if (targetPressedClock < 60f)
            {
                targetPressedClock += Time.deltaTime;
            }
            targetReleasedClock = 0f;
        }
        else
        {
            if (targetUp)
            {
                targetReleasedClock = 0f;
            }
            else if (targetReleasedClock < 60f)
            {
                targetReleasedClock += Time.deltaTime;
            }
            targetPressedClock = 0f;
        }
        targetHeldLastFrame = targetHeld;
        */
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

    public void RecenterFree()
    {
        OnRecenterFree.Invoke();
    }

    public void RecenterTarget()
    {
        OnRecenterTarget.Invoke();
    }

    void CycleTargets()
    {
        if (targets.Count > 0)
        {
            changeTargetIndexOffset++;
            SetTarget(targets[changeTargetIndexOffset % targets.Count]);
        }   
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

    public bool IsValidTarget(GameObject target)
    {
        if (target == null) return false;
        if (target.transform.root.TryGetComponent<Actor>(out Actor actor))
        {
            if (!actor.IsAlive())
            {
                return false;
            }
        }
        float playerDist = Vector3.Distance(target.transform.position, player.positionReference.Spine.position);
        float camDist = Vector3.Distance(target.transform.position, cam.transform.position);

        Ray pRay = new Ray(player.positionReference.Spine.position, (target.transform.position - player.positionReference.Spine.position));
        Ray cRay = new Ray(cam.transform.position, (target.transform.position - cam.transform.position));

        bool playerTerrainBlocked = Physics.Raycast(pRay, playerDist, MaskReference.Terrain);
        bool camTerrainBlocked = Physics.Raycast(cRay, camDist, MaskReference.Terrain);

        bool playerInRange = playerDist < maxPlayerDistance;
        bool camInRange = camDist < maxCamDistance;

        Vector3 vpp = Camera.main.WorldToViewportPoint(target.transform.position);
        bool onScreen = (Mathf.Abs(vpp.x) <= 1f) && (Mathf.Abs(vpp.y) <= 1f) && (vpp.z >= 0);
        bool invalid = !onScreen || ((!playerInRange || playerTerrainBlocked) && (!camInRange || camTerrainBlocked));



        return !invalid;
    }

    void UpdateFreeLook()
    {
        radius = Mathf.Clamp((cmtg.Sphere.radius * freeLookDistanceMultiplier) + freeLookDistanceOffset, freeLookMinDistance, freeLookMaxDistance);

        topHeight = -(cmtg.transform.position.y - playerHead.position.y - topRigHeightFixedHead) + radius * topRigHeightMult;
        topRadius = RadiusWithY(radius + (radius * (radiusHeightMult * topRigHeightMult)), topHeight);

        midHeight = -(cmtg.transform.position.y - playerHead.position.y - midRigHeightFixedHead) + radius * midRigHeightMult;
        midRadius = RadiusWithY(radius + (radius * (radiusHeightMult * midRigHeightMult)), midHeight);

        botHeight = -(cmtg.transform.position.y - playerHead.position.y - botRigHeightFixedHead) + radius * botRigHeightMult;
        botRadius = RadiusWithY(radius + (radius * (radiusHeightMult * botRigHeightMult)), botHeight);

        freeLook.m_Orbits[0].m_Height = topHeight;
        freeLook.m_Orbits[0].m_Radius = topRadius;

        freeLook.m_Orbits[1].m_Height = midHeight;
        freeLook.m_Orbits[1].m_Radius = midRadius;

        freeLook.m_Orbits[2].m_Height = botHeight;
        freeLook.m_Orbits[2].m_Radius = botRadius;
    }

    float RadiusWithY(float r, float y)
    {
        if (targetingCylinder) return r;
        if (Mathf.Abs(y) > Mathf.Abs(r)) return r; // making sure we don't accidentally get NaNs, that should only be happening when editing values in inspector anyway
        return Mathf.Sqrt(Mathf.Pow(r,2) - Mathf.Pow(y,2));
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
        if (target == null || player.GetCombatTarget() == null)
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
        if (target == null || player.GetCombatTarget() == null)
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

        if (cmtg == null) return;
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(cmtg.transform.position, radius);
#if UNITY_EDITOR
        Handles.color = Color.red;
        Handles.DrawWireDisc(cmtg.transform.position + Vector3.up * topHeight, Vector3.up, topRadius);
        Handles.DrawWireDisc(cmtg.transform.position + Vector3.up * midHeight, Vector3.up, midRadius);
        Handles.DrawWireDisc(cmtg.transform.position + Vector3.up * botHeight, Vector3.up, botRadius);
#endif
    }
}
