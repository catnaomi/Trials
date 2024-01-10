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
using UnityEditor.ShaderGraph.Internal;

public class PlayerTargetManager : MonoBehaviour
{
    public PlayerActor player;
    public Transform playerHead;
    public Camera cam;
    public float maxPlayerDistance = 20f;
    public float maxCamDistance = 20f;
    public float maxMidpointDistance = 20f;

    public LayerMask blocksTargetingMask;
    [Header("Current Target Status")]
    public GameObject currentTarget;
    public List<GameObject> targets;
    public bool lockedOn;

    

    CinemachineTargetGroup cmtg;

    public float targetDelay;
    [Header("Press To Change Target Settings")]
    public float targetChangeSpeed = 10f;
    public float targetChangeMaxDistance = 25f;
    public Transform targetAim;
    public Transform centerAim;
    bool targetAimShouldSnap = true;
    [SerializeField, ReadOnly] int changeTargetIndexOffset = 0;
    bool lockOnRelease;
    bool lockOnPress;
    PlayerInput inputs;
    [Header("Dialogue Settings")]
    public CinemachineVirtualCameraBase dialogueCamera;
    public float maxDialogueCameraDistance = 5f;
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

        freeLook = player.vcam.target.GetComponent<CinemachineFreeLook>();
        targetAim.position = Vector3.zero;

        Recenter();
    }

    void SetupInputListeners()
    {
        inputs = player.GetComponent<PlayerInput>();

        inputs.actions["Target"].performed += OnLockOn;
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

            HashSet<GameObject> validTargets = new HashSet<GameObject>();
            HashSet<GameObject> invalidTargets = new HashSet<GameObject>();

            // find targets already in the scene
            foreach (GameObject target in allLockTargets)
            {
                bool invalid = !IsValidTarget(target);

                if (invalid)
                {
                    invalidTargets.Add(target);
                }
                else
                {
                    validTargets.Add(target);
                }
            }

            // also add targets already in list.
            // disabled targets won't get caught above and thus won't be invalidated properly without this.
            foreach (GameObject cTarget in targets)
            {
                bool invalidC = !IsValidTarget(cTarget);

                if (invalidC)
                {
                    invalidTargets.Add(cTarget);
                }
                else
                {
                    validTargets.Add(cTarget);
                }
            }

            // remove all invalid targets and add all valid targets to target list
            targets.RemoveAll(t => t == null);
            targets = targets.Except(invalidTargets).Union(validTargets).ToList();

            if (targets.Count > 0 && !lockedOn)
            {
                // sort by distance: closest first. current target remains in first always
                targets.Sort((a,b) => {
                    return (int)Mathf.Sign(Vector3.Distance(player.transform.position, a.transform.position) - Vector3.Distance(player.transform.position, b.transform.position));
                });
                // don't re-sort while locked on to maintain target changing order
                changeTargetIndexOffset = 0;
                SetTarget(targets[changeTargetIndexOffset]);
            }
            else if (!lockedOn)
            {
                SetTarget(null);
            }
            OnTargetUpdate.Invoke();
            yield return new WaitForSecondsRealtime(targetDelay);
        }
    }


    void Update()
    {
        HandleInput();

        if (player.IsInDialogue())
        {
            if (player.GetCombatTarget() != currentTarget)
            {
                SetTarget(player.GetCombatTarget());
            }
        }
        // set the player's target to the current target
        else if (lockedOn)
        {
            player.SetCombatTarget(currentTarget);
        }
        else
        {
            // remove player's target if not locked on
            player.SetCombatTarget(null);

        }

        if (lockedOn && currentTarget == player.GetCombatTarget() && !IsValidTarget(currentTarget) && !player.IsInDialogue())
        {
            ExpireTarget();
        }

        HandleAimTargetPosition();
        HandleTargetGroup();
    }

    void HandleInput()
    {
        if (lockOnPress)
        {
            if (lockedOn)
            {
                if (targets.Count > 1)
                {
                    CycleTargets();
                }
                RecenterTarget();
            }
            else
            {
                if (targets.Count > 0)
                {
                    lockedOn = true;
                }
                else
                {
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
        lockOnRelease = false;
        lockOnPress = false;

    }


    // controls the object that hovers on targets to create a gradual transition between targets
    void HandleAimTargetPosition()
    {
        // center target
        if (!player.IsInDialogue())
        {
            centerAim.position = player.positionReference.centerTarget.position;
        }
        else
        {
            Vector3 centerPos = player.positionReference.centerTarget.position;
            if (currentTarget != null)
            {
                centerPos.y = currentTarget.transform.position.y;
            }
            centerAim.position = centerPos;
        }
        // aim target
        if (!lockedOn || currentTarget == null || Vector3.Distance(targetAim.position, currentTarget.transform.position) > targetChangeMaxDistance)
        {
            targetAimShouldSnap = true;
        }

        if (currentTarget != null)
        {
            Vector3 targetPosition = currentTarget.transform.position;
            Vector3 targetDir = currentTarget.transform.position - centerAim.position;

            targetPosition = Vector3.ClampMagnitude(targetDir, maxMidpointDistance) + centerAim.position; 

            if (targetAimShouldSnap)
            {
                targetAim.position = targetPosition;
                targetAimShouldSnap = false;
            }
            else
            {
                targetAim.position = Vector3.MoveTowards(targetAim.position, targetPosition, targetChangeSpeed * Time.deltaTime);
            }
        }

    }

    
    // controls the cinemachine target group
    void HandleTargetGroup()
    {
        if (handleCamera && lockedOn && cmtg.m_Targets.Length > 1)
        {
            //this.transform.rotation = Quaternion.LookRotation(PlayerActor.player.transform.forward);
            Vector3 dir = cmtg.m_Targets[1].target.position - cmtg.m_Targets[0].target.position;
            dir.y = 0;
            dir.Normalize();
            this.transform.rotation = Quaternion.LookRotation(dir);
            UpdateFreeLook();

        }
        if (player.IsInDialogue())
        {
            float dist = Vector3.Distance(centerAim.position, currentTarget.transform.position);
            dialogueCamera.LookAt = (dist < maxDialogueCameraDistance) ? targetAim : currentTarget.transform;
        }
    }
    
    // runs when the current target expires (dies or becomes invalid)
    void ExpireTarget()
    {
        if (targets.Count > 0)
        {
            SetTarget(null);
            // find a valid target from the current list and switch to that
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
            {  //if an alternate target is successfully found
                SetTarget(targets[changeTargetIndexOffset % targets.Count]);
                player.SetCombatTarget(currentTarget);
            }
            else
            {   // end targeting if no alternative found
                player.SetCombatTarget(null);
                currentTarget = null;
                lockedOn = false;
            }
        }
        else
        {
            // end targeting if no targets
            player.SetCombatTarget(null);
            currentTarget = null;
            lockedOn = false;
        }
    }

    public void Recenter(bool force = false)
    {
        if (lockedOn)
        {
            RecenterTarget(force);
        }
        else
        {
            RecenterFree(force);
        }
    }

    public void RecenterFree(bool force = false)
    {
        if (force || inputs.actions["look"].ReadValue<Vector2>().magnitude == 0)
            OnRecenterFree.Invoke();
    }

    public void RecenterTarget(bool force = false)
    {
        if (force || inputs.actions["look"].ReadValue<Vector2>().magnitude == 0)
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

    /*
     * Criteria for Valid Targets:
     * If not Player's current target:
     * * must be on screen
     * * must be within playerDist
     * * must be within camDist
     * * must not be blocked by terrain
     * 
     * If Player's current target:
     * * must be within camDist
     */
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

        if (!target.activeInHierarchy)
        {
            return false;
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
        // for non current target
        bool invalid = !onScreen || ((!playerInRange || playerTerrainBlocked) || (!camInRange || camTerrainBlocked));

        if (target == PlayerActor.player.GetCombatTarget())
        {
            invalid = !camInRange;
        }

        //Debug.Log(string.Format("object:{0}, pterr:{1}, prang:{2}, cterr:{3}, crang:{4}, invalid:{5}", target.name, playerTerrainBlocked, (int)playerDist, camTerrainBlocked, (int)camDist, invalid));

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
