using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IceShieldGolemMecanimActor : Actor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    CapsuleCollider collider;
    CharacterController cc;
    HumanoidPositionReference positionReference;
    ActorTimeTravelHandler timeTravelHandler;

    [Header("Strafe Settings")]
    [SerializeField, ReadOnly] Vector3 initialPosition;
    [SerializeField, ReadOnly] Vector3 strafeVector;
    [SerializeField, ReadOnly] float strafeDot;
    [SerializeField, ReadOnly] float playerDot;
    public float maximumStrafeDistance;
    public float strafeSpeed;
    public bool isHitboxActive;
    public UnityEvent OnHitboxActive;

    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] float StrafeMove;

    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        inventory = GetComponent<HumanoidNPCInventory>();
        collider = GetComponent<CapsuleCollider>();
        cc = GetComponent<CharacterController>();
        positionReference = GetComponent<HumanoidPositionReference>();
        timeTravelHandler = GetComponent<ActorTimeTravelHandler>();


        InitializeStrafe();
    }

    public void InitializeStrafe()
    {
        initialPosition = this.transform.position;
        strafeVector = this.transform.right;
    }

    public void UpdateStrafe(Vector3 currentPosition)
    {
        Vector3 position;
        float dot = Vector3.Dot(strafeVector, currentPosition - initialPosition);
        dot /= maximumStrafeDistance;

        strafeDot = Mathf.Clamp(dot, -1f, 1f);

        position = initialPosition + strafeVector * strafeDot * maximumStrafeDistance;

        if (CombatTarget != null)
        {
            playerDot = Vector3.Dot(strafeVector, CombatTarget.transform.position - initialPosition);
            playerDot /= maximumStrafeDistance;
            playerDot = Mathf.Clamp(playerDot, -1, 1);
            StrafeMove = strafeDot - playerDot;

            Vector3 targetPosition = initialPosition + strafeVector * playerDot * maximumStrafeDistance;

            position = Vector3.MoveTowards(position, targetPosition, strafeSpeed * Time.deltaTime);
        }

        this.transform.position = position;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 position = initialPosition != Vector3.zero ? initialPosition : this.transform.position;
        Vector3 direction = strafeVector != Vector3.zero ? strafeVector : this.transform.right;
        direction *= maximumStrafeDistance;

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(position + Vector3.up * 2f, direction);
        Gizmos.DrawRay(position + Vector3.up * 2f, -direction);

        Gizmos.DrawRay(position + direction, Vector3.up * 4f);
        Gizmos.DrawRay(position - direction, Vector3.up * 4f);

        Gizmos.color = Color.red;

        Gizmos.DrawRay(position + Vector3.Project(this.transform.position - position, direction), Vector3.up * 4f);

        Gizmos.color = Color.green;

        Gizmos.DrawRay(position + playerDot * direction, Vector3.up * 4f);
    }
    // Update is called once per frame
    public override void ActorPostUpdate()
    {
        UpdateStrafe(this.transform.position);
        UpdateMecanimValues();
    }
    void UpdateMecanimValues()
    {
        animator.SetFloat("StrafeMove", StrafeMove);
    }

    public void TakeDamage(DamageKnockback damage)
    {
        throw new System.NotImplementedException();
    }

    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void StartCritVulnerability(float time)
    {
        throw new System.NotImplementedException();
    }

    public bool IsCritVulnerable()
    {
        throw new System.NotImplementedException();
    }

    public void GetParried()
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        throw new System.NotImplementedException();
    }

    public GameObject GetGameObject()
    {
        throw new System.NotImplementedException();
    }
}
