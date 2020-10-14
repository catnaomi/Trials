using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LockonShoulderCam : MonoBehaviour
{

    public GameObject player;
    PlayerActor playerController;

    InputHandler inputHandler;

    [Space(10)]
    public float rotateSpeed = 180f;
    [Space(10)]
    public float unlockedHeight = 3f;
    public float unlockedDistance = 8f;
    [Space(5)]
    public float playerEyeHeight = 1f;
    [Space(5)]
    public float lockedHeight = 3f;
    public float lockedDistance = 4f;
    [Space(5)]
    public float knockedDownHeight = 6f;
    public float knockedDownDistance = 8f;
    [Space(5)]
    public float minDistanceToPlayer = 4f;
    [Space(5)]
    public float crosshairHeight = 0f;
    public float crosshairDistance = 2f;
    public float crosshairBack = 1f;
    public float crosshairRight = 1f;
    public float aimVerticalLimit = 5f;
    public float aimVerticalSpeed = 20f;
    public float lookVerticalLimit = 1f;
    public float lookVerticalSpeed = 1f;
    public float lookVerticalAngleLimit = 70f;

    //public float lockOnLookHeight;

    [Space(10)]
    public float TargetChangeSpeed = 0.5f;
    public float HorizontalStringLength = 2f;

    [Space(10)]
    public float frustumDistance = 0.5f;
    public float transitionTime;
    public LayerMask layerMask;
    Camera cam;

    Vector3 InputRotation;
    float InputAimHeight;
    float InputAimAngle;

    Vector3 cameraPositionGoal;
    Vector3 collisionAdjustedPosition;
    [HideInInspector] public Vector3 focusPosition;
    [HideInInspector] public Vector3 playerIKPosition;

    Vector3 stringAdjustedPlayerPosition;
    Vector3 lockTargetPoint;
    Vector3 dampedLockTargetPoint;
    Vector3 collisionPoint;

    Vector3 camVelocity;


    public List<GameObject> targetables;
    public List<GameObject> validTargets;
    public bool lockedOn;
    public GameObject currentTarget;
    public GameObject targetClosestToCenter;

    
    public float TargetRefreshDelay = 1f;
    public float lockDistance = 20f;
    public float centerRadius = 0.2f;
    public float targetClock;

    public float ForceUnlockDelay = 3f;
    public float forceUnlockClock;
    Dictionary<AxisUtilities.AxisDirection, Transform> directionToTarget;

    public bool crosshairMode;

    public GameObject statDisplay;
    public GameObject crosshairDisplay;
    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerActor>();

        transform.position.Set(transform.position.x, player.transform.position.y + unlockedHeight, transform.position.z);

        cam = GetComponent<Camera>();
        targetClock = TargetRefreshDelay;
        inputHandler = InputHandler.main;

        inputHandler.SecondaryStickFlick.AddListener(SwitchTargets);
        lockedOn = false;

        InputRotation = player.transform.forward;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var frustumHeight = 2.0f * frustumDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * cam.aspect;
        Vector3 cameraSize = new Vector3(frustumWidth, frustumHeight, frustumDistance);


        Vector3 playerPos = player.transform.position + player.transform.up * playerEyeHeight;

        Vector3 angledAimVector = (Quaternion.AngleAxis(InputAimAngle, cam.transform.right) * NumberUtilities.FlattenVector(InputRotation)).normalized;
        Vector3 angledIKVector = (Quaternion.AngleAxis(InputAimAngle, cam.transform.forward) * NumberUtilities.FlattenVector(transform.right)).normalized;

        if (playerController.humanoidState == HumanoidActor.HumanoidState.Ragdolled)
        {
            focusPosition = playerController.positionReference.Hips.position;
            Vector3 kbSourceDirection = -playerController.lastForce;
            kbSourceDirection.Scale(new Vector3(1, 0, 1));
            kbSourceDirection.Normalize();
            cameraPositionGoal = focusPosition + kbSourceDirection * knockedDownDistance + Vector3.up * knockedDownHeight;
            InputRotation = -kbSourceDirection;
        }
        else if (lockedOn)
        {
            //Debug.Log("movement" + Vector3.Distance(focusPosition, player.transform.position + player.transform.up * lookHeight));
            stringAdjustedPlayerPosition = HorizontalStringPull(stringAdjustedPlayerPosition, playerPos, HorizontalStringLength);
            //CamAxisDamp(focusPosition, playerPos, minLockDistance, maxLockDistance);
            //focusPosition = player.transform.position + player.transform.up * lookHeight;
            lockTargetPoint = currentTarget.GetComponentInChildren<Renderer>().bounds.center;
            dampedLockTargetPoint = Vector3.SmoothDamp(dampedLockTargetPoint, lockTargetPoint, ref camVelocity, TargetChangeSpeed);//Vector3.SmoothDamp(dampedLockTargetPoint, (targetCenter + playerPos) / 2f, ref camVelocity, TargetChangeSpeed);//Vector3.Lerp(dampedLockTargetPoint, (targetCenter + playerPos) / 2f, TargetBuffer);

            focusPosition = (playerPos + dampedLockTargetPoint) / 2f;

            Vector3 overShoulderDirection = NumberUtilities.FlattenVector(stringAdjustedPlayerPosition - focusPosition);
            float heightDifference = Mathf.Max(focusPosition.y - playerPos.y, 0f);
            float adjustedLockDistance = Mathf.Max(Vector3.Distance(playerPos, lockTargetPoint) / 2f, lockedDistance);
            //Debug.Log(String.Format("cam dist: {0:0.00}, player dist {1:0.00}", Vector3.Distance(cam.transform.position, lockTargetPoint), Vector3.Distance(playerPos, lockTargetPoint)));
            cameraPositionGoal = focusPosition + (overShoulderDirection.normalized * (overShoulderDirection.magnitude + adjustedLockDistance)) + Vector3.up * (lockedHeight + heightDifference);

            //InputRotation = -focusToTarget;
        }
        else if (crosshairMode)
        {
            //focusPosition = player.transform.position + player.transform.up * (playerEyeHeight + InputAimHeight) + InputRotation * crosshairDistance;
            //playerIKPosition = player.transform.position + player.transform.up * (playerEyeHeight + InputAimHeight) + transform.right * crosshairDistance;



            focusPosition = playerPos + angledAimVector * crosshairDistance;
            playerIKPosition = playerPos + angledIKVector * crosshairDistance;


            Vector3 dir = (focusPosition - playerPos).normalized;
            Vector3 backPos = playerPos + dir * -crosshairBack;
            cameraPositionGoal = backPos + player.transform.up * crosshairHeight + player.transform.right * crosshairRight;
            dampedLockTargetPoint = focusPosition;
            //InputRotation = -player.transform.forward;
        }
        else
        {
            Vector3 playerForward = InputRotation;
            focusPosition = playerPos;//player.transform.position + player.transform.up * playerEyeHeight;
            //cameraPositionGoal = playerPos + (playerForward * unlockedDistance) + (player.transform.up * (unlockedHeight + -InputAimHeight));

            cameraPositionGoal = playerPos + (playerForward * (unlockedDistance / 2f)) + (player.transform.up * (unlockedHeight)) + (angledAimVector * (unlockedDistance / 2f));
            dampedLockTargetPoint = focusPosition;
        }

        Vector3 currentPosition = this.transform.position;


        /*
        Vector3 goalHorizontal = new Vector3(cameraPositionGoal.x, 0, cameraPositionGoal.z);
        Vector3 currentHorizontal = new Vector3(currentPosition.x, 0, currentPosition.z);
        Vector3 targetHorizontal;
        targetHorizontal = Vector3.Lerp(currentHorizontal, goalHorizontal, HorizontalBuffer);

        float goalVertical = cameraPositionGoal.y;
        float currentVertical = currentPosition.y;
        float targetVertical;
        targetVertical = Mathf.Lerp(currentVertical, goalVertical, VerticalBuffer);

        targetPosition = targetHorizontal + Vector3.up * targetVertical;*/
        Vector3 direction = (cameraPositionGoal - playerPos);





        if (Physics.BoxCast(
                playerPos,
                cameraSize / 2,
                direction.normalized,
                out RaycastHit hit,
                Quaternion.LookRotation(focusPosition - cameraPositionGoal),
                direction.magnitude,
                layerMask.value))
        {
            collisionPoint = hit.point;
            collisionAdjustedPosition = collisionPoint + (hit.normal * 0.1f);//focusPosition + (direction * hit.distance);
        }
        else
        {
            collisionAdjustedPosition = cameraPositionGoal;
        }

        float minDist = (crosshairMode ? 0f : minDistanceToPlayer);
        while (Vector3.Distance(playerPos, collisionAdjustedPosition) < minDist)
        {
            collisionAdjustedPosition += Vector3.up * 0.1f;
        }

        transform.position = collisionAdjustedPosition;

        Vector3 look = focusPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(look);

        targetClock += Time.deltaTime;
        if (targetClock > TargetRefreshDelay)
        {
            targetClock = 0f;
            UpdateLockOn();
        }
    }

    private void Update()
    {
        if (inputHandler.GetTargetDown())
        {
            if (!lockedOn && validTargets.Count > 0)
            {
                if (targetClosestToCenter == null)
                {
                    UpdateLockOn();
                }
                lockedOn = true;
                SetTarget(targetClosestToCenter);
            }
            /*
            else if (lockedOn)
            {
                lockedOn = false;
            }
            else if (!lockedOn)
            {
                InputRotation = playerController.transform.forward;
            }
            */
        }

        if (lockedOn && inputHandler.GetTargetHeld() && inputHandler.targetClock > 1f)
        {
            InputRotation = NumberUtilities.FlattenVector(-cam.transform.forward);
            lockedOn = false;
        }

        if (lockedOn && !IsTargetValid(currentTarget))
        {
            UpdateLockOn();
            forceUnlockClock += Time.deltaTime;
            if (targetClosestToCenter == null || forceUnlockClock > ForceUnlockDelay)
            {
                lockedOn = false;
            }
            else
            {
                SetTarget(targetClosestToCenter);
            }
        }
        else
        {
            forceUnlockClock = 0f;
        }

        if (!lockedOn)
        {
            SetTarget(null);
            targetClosestToCenter = null;
        }

        // cam rotation

        float limit = (crosshairMode ? aimVerticalLimit : lookVerticalLimit);
        float vspeed = (crosshairMode ? aimVerticalSpeed : lookVerticalSpeed);

        if (crosshairMode || !lockedOn)
        {
            InputAimHeight += Input.GetAxisRaw("SecondaryVertical") * vspeed * Time.deltaTime;
            InputAimHeight = Mathf.Clamp(InputAimHeight, -limit, limit);
            InputAimAngle += -Input.GetAxisRaw("SecondaryVertical") * rotateSpeed * Time.deltaTime;
            InputAimAngle = Mathf.Clamp(InputAimAngle, -lookVerticalAngleLimit, lookVerticalAngleLimit);
        }
        if (!lockedOn)
        {
            InputRotation = Quaternion.AngleAxis(Input.GetAxisRaw("SecondaryHorizontal") * rotateSpeed * Time.deltaTime, transform.up) * InputRotation;
        }
        
    }

    private void OnGUI()
    {
        crosshairDisplay.SetActive(crosshairMode && !lockedOn);
        if (lockedOn && currentTarget.TryGetComponent<Actor>(out Actor targetActor))
        {
            Renderer targetRenderer = currentTarget.GetComponentInChildren<Renderer>();
            Vector3 renderPosition =
            (
                targetRenderer.bounds.center +
                Vector3.up * targetRenderer.bounds.extents.y +
                transform.right * 0f + // horizontal offset
                Vector3.up * 0.5f // vertical offset
            );
            statDisplay.transform.position = Camera.main.WorldToScreenPoint(renderPosition);
            statDisplay.SetActive(true);

            StatDisplay displayData = statDisplay.GetComponentInChildren<StatDisplay>();
            if (displayData.actor != targetActor)
            {
                displayData.actor = targetActor;
                //displayData.Reset();
            }
        }
        else
        {
            statDisplay.SetActive(false);
        }
    }

    private Vector3 HorizontalStringPull(Vector3 oldPos, Vector3 newPos, float stringLength)
    {
        Vector3 dir = newPos - oldPos;

        Vector3 forwardProject = Vector3.Project(dir, cam.transform.forward);
        Vector3 upProject = Vector3.Project(dir, cam.transform.up);
        Vector3 rightProject = Vector3.Project(dir, cam.transform.right); // how far the target focus position is from the current one

        Vector3 rightMovement = Vector3.zero;

        if (rightProject.magnitude > stringLength)
        {
            rightMovement = rightProject.normalized * (rightProject.magnitude - stringLength);
        }

        return oldPos + (forwardProject + upProject + rightMovement);
    }

    private Vector3 CamAxisDamp(Vector3 oldVector, Vector3 newVector, float minDistance, float maxDistance)
    {
        Vector3 dir = newVector - oldVector;

        float min = minDistance;
        float max = maxDistance;


        Vector3 forwardProject = Vector3.Project(dir, cam.transform.forward);
        Vector3 upProject = Vector3.Project(dir, cam.transform.up);
        Vector3 rightProject = Vector3.Project(dir, cam.transform.right);

        float forwardPercent = Mathf.Clamp((forwardProject.magnitude - min) / (max - min), 0f, 1f);
        float upPercent = Mathf.Clamp((upProject.magnitude - min) / (max - min), 0f, 1f);
        float rightPercent = Mathf.Clamp((rightProject.magnitude - min) / (max - min), 0f, 1f);


        float forwardBuffer = 1f;// forwardPercent;
        float upBuffer = 1f;// upPercent;
        float rightBuffer = rightPercent;

        Vector3 forwardMovement = Vector3.Lerp(Vector3.zero, forwardProject, forwardBuffer);
        Vector3 upMovement = Vector3.Lerp(Vector3.zero, upProject, upBuffer);
        Vector3 rightMovement = Vector3.Lerp(Vector3.zero, rightProject, rightBuffer);

        if (rightPercent <= 0f)
        {
            //rightMovement += cam.transform.right * (Mathf.Sin(Time.time /2f) - 0.5f) * 0.1f * Time.deltaTime;
            float rotatePercent = (rightProject.magnitude - min) / min;
            rightMovement = rightProject.normalized * rotatePercent * 0.05f * Time.deltaTime;
        }
        if (Input.GetButtonDown("Interact"))
        {
            // do nothing
        }
        //Debug.Log("Current: " + rightMovement + " Damped: " + (Vector3.Lerp(Vector3.zero, rightProject, rightBuffer)));
        return oldVector + (forwardMovement + upMovement + rightMovement);
    }

    private void SwitchTargets()
    {
        try
        {
            if (lockedOn)
            {
                AxisUtilities.AxisDirection direction = AxisUtilities.InvertAxis(inputHandler.SecondaryFlickDirection, false, false, false);
                SetTarget(directionToTarget[direction].gameObject);
                UpdateDirections();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    private void UpdateLockOn()
    {

        targetables.Clear();
        targetables.AddRange(GameObject.FindGameObjectsWithTag("Actor"));
        targetables.AddRange(GameObject.FindGameObjectsWithTag("LockTarget"));
        validTargets = new List<GameObject>();
        foreach (GameObject targetable in targetables)
        {
            if (IsTargetValid(targetable))
            {
                validTargets.Add(targetable);
            }
        }
        if (currentTarget != null && !validTargets.Contains(currentTarget))
        {
            validTargets.Add(currentTarget);
        }
        GameObject leadingTarget = null;
        Vector3 center = new Vector3(0.5f, 0.5f);

        foreach (GameObject target in validTargets)
        {
            if (leadingTarget == null)
            {
                leadingTarget = target;
                continue;
            }
            else
            {
                Collider leadingCollider = leadingTarget.GetComponentInChildren<Collider>();
                Vector3 leadingCenter = leadingCollider.bounds.center;
                Vector3 leadingPoint = cam.WorldToViewportPoint(leadingCenter);
                float leadingPointDistance = Vector3.Distance(new Vector3(leadingPoint.x, leadingPoint.y), center);
                float leadingWorldDistance = leadingPoint.z;

                Collider targetCollider = target.GetComponentInChildren<Collider>();
                Vector3 targetCenter = targetCollider.bounds.center;
                Vector3 targetPoint = cam.WorldToViewportPoint(targetCenter);
                float targetPointDistance = Vector3.Distance(new Vector3(targetPoint.x, targetPoint.y), center);
                float targetWorldDistance = targetPoint.z;

                if (leadingPointDistance < centerRadius && targetPointDistance < centerRadius)
                {
                    if (targetWorldDistance < leadingWorldDistance)
                    {
                        leadingTarget = target;
                    }
                }
                else if (targetPointDistance < leadingPointDistance)
                {
                    leadingTarget = target;
                }
            }
        }
        targetClosestToCenter = leadingTarget;
        UpdateDirections();
    }

    private void UpdateDirections()
    {
        if (!lockedOn || currentTarget == null) return;
        List<Transform> otherTransforms = new List<Transform>();
        foreach (GameObject target in validTargets)
        {
            if (target != currentTarget)
            {
                otherTransforms.Add(target.transform);
            }
        }

        directionToTarget = AxisUtilities.MapTransformsToAxisDirections(cam.transform, currentTarget.GetComponentInChildren<Collider>().bounds.center, otherTransforms);
    }
    private bool IsTargetValid(GameObject target)
    {
        if (target == null || !target.activeInHierarchy)
        {
            // target is inactive or doesn't exist.
            return false;
        }

        if (target == player.gameObject)
        {
            // target is the player
            return false;
        }

        Vector3 viewPoint = cam.WorldToViewportPoint(target.GetComponent<Collider>().bounds.center);
        if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1 || viewPoint.z < 0)
        {
            // target is off-screen
            // return false;
        }

        float dist = Vector3.Distance(cam.transform.position, target.transform.position);

        if (dist > lockDistance)
        {
            // target is too far away
            return false;
        }

        bool obstructed = Physics.Raycast(player.transform.position, target.transform.position - player.transform.position, out RaycastHit hit, lockDistance, LayerMask.GetMask("Terrain"));

        if (obstructed && hit.distance < dist)
        {
            // terrain is between player and target
            return false;
        }
        return true;

    }

    public Vector3 GetPlayerFaceForward()
    {
        if (crosshairMode)
        {
            return NumberUtilities.FlattenVector(focusPosition - (player.transform.position + player.transform.up * playerEyeHeight)).normalized;
        }
        else if (!lockedOn)
        {
            return NumberUtilities.FlattenVector(cam.transform.forward).normalized;
        }
        else
        {
            return NumberUtilities.FlattenVector(lockTargetPoint - player.transform.position).normalized;
        }
    }

    public void SetCrosshairMode(bool mode)
    {
        if (mode && !crosshairMode)
        {
            InputRotation = -InputRotation;
        }
        else if (!mode && crosshairMode)
        {
            InputRotation = -InputRotation;
        }
        
        crosshairMode = mode;
    }

    private void SetTarget(GameObject target)
    {
        currentTarget = target;
        playerController.SetCombatTarget(target);
    }
    private void OnDrawGizmosSelected()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        var frustumHeight = 2.0f * frustumDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * cam.aspect;
        Vector3 cameraSize = new Vector3(frustumWidth, frustumHeight, frustumDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(cameraPositionGoal, cameraSize);
        Gizmos.DrawSphere(playerIKPosition, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(collisionAdjustedPosition, cameraSize);
        Gizmos.DrawSphere(collisionPoint, 0.05f);
        Gizmos.DrawRay(focusPosition, cam.transform.right * HorizontalStringLength);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(dampedLockTargetPoint, 0.05f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(player.transform.position + player.transform.up * playerEyeHeight, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(focusPosition, 0.5f);

        foreach (GameObject target in targetables)
        {
            Collider collider = target.GetComponentInChildren<Collider>();
            if (lockedOn && target == currentTarget)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                bool dir = false;
                try
                {
                    if (lockedOn)
                    {
                        float offset = -0.5f;
                        GUIStyle style = new GUIStyle();
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = Color.white;
                        foreach (AxisUtilities.AxisDirection axis in directionToTarget.Keys)
                        {
                            if (directionToTarget[axis] == target.transform)
                            {
                                Handles.color = Color.white;
                                Handles.Label(collider.bounds.center + cam.transform.up * offset, AxisUtilities.AxisDirectionToString(AxisUtilities.InvertAxis(axis, false, false, false)), style);
                                dir = true;
                                offset += 0.3f;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                }

                if (target == targetClosestToCenter)
                {
                    Gizmos.color = Color.magenta;
                }
                else if (dir)
                {
                    Gizmos.color = Color.cyan;
                }
                else if (validTargets.Contains(target))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.grey;
                }
            }
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lockDistance);
    }

    public Vector3 GetCrosshairPosition()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, crosshairDistance, LayerMask.GetMask("Actors", "Terrain")))
        {
            return hit.point;
        }
        else
        {
            return focusPosition;
        }
    }
}
