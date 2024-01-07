using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IceShieldGolemMecanimActor : Actor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    CapsuleCollider collider;
    HumanoidPositionReference positionReference;
    ActorTimeTravelHandler timeTravelHandler;

    [Header("Block & Block Switch")]
    public bool isBlocking;
    public string blockSequence;
    [ReadOnly, SerializeField] Queue<int> blockQueue;
    [Header("Strafe Settings")]
    [SerializeField, ReadOnly] Vector3 initialPosition;
    [SerializeField, ReadOnly] Vector3 strafeVector;
    [SerializeField, ReadOnly] float strafeDot;
    [SerializeField, ReadOnly] float playerDot;
    public float maximumStrafeDistance;
    public float strafeSpeed;
    public bool isHitboxActive;
    public UnityEvent OnHitboxActive;
    [Header("Attack Settings")]
    public float hitboxSize = 1f;
    public DamageKnockback riposteDamage;
    [Header("References")]
    public Transform hitboxMountR;
    [SerializeField, ReadOnly] Hitbox hitboxR;
    public Transform hitboxMountL;
    [SerializeField, ReadOnly] Hitbox hitboxL;
    public GameObject shieldR;
    public GameObject shieldL;
    public GameObject blockCollider;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] float StrafeMove;
    [ReadOnly, SerializeField] int BlockType;
    [ReadOnly, SerializeField] bool OnHit; //trigger
    [ReadOnly, SerializeField] bool OnFail; //trigger
    [ReadOnly, SerializeField] bool GuardBreak; //trigger

    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        inventory = GetComponent<HumanoidNPCInventory>();
        collider = GetComponent<CapsuleCollider>();
        positionReference = GetComponent<HumanoidPositionReference>();
        timeTravelHandler = GetComponent<ActorTimeTravelHandler>();

        BlockType = 1;
        InitializeStrafe();
        InitializeBlockSequence();
        NextBlock();
        GenerateHitboxes();
    }

    public override void ActorPostUpdate()
    {
        UpdateStrafe(this.transform.position);
        UpdateMecanimValues();
    }
    void UpdateMecanimValues()
    {
        animator.SetFloat("StrafeMove", StrafeMove);
        animator.UpdateTrigger("OnHit", ref OnHit);
        animator.UpdateTrigger("OnFail", ref OnFail);
        animator.UpdateTrigger("GuardBreak", ref GuardBreak);
        animator.SetInteger("BlockType", BlockType);
    }

    #region Strafing
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
        if (IsBlocking())
        {
            this.transform.position = position;
        }
        
    }

    void OnDrawGizmosSelected()
    { 
        Vector3 position = initialPosition != Vector3.zero ? initialPosition : this.transform.position;
        Vector3 direction = strafeVector != Vector3.zero ? strafeVector : this.transform.right;
        direction *= maximumStrafeDistance;

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(position + Vector3.up * 1f, direction);
        Gizmos.DrawRay(position + Vector3.up * 1f, -direction);

        Gizmos.DrawRay(position + direction, Vector3.up * 2f);
        Gizmos.DrawRay(position - direction, Vector3.up * 2f);

        Gizmos.color = Color.red;

        Gizmos.DrawRay(position + Vector3.Project(this.transform.position - position, direction), Vector3.up * 2f);

        Gizmos.color = Color.green;

        Gizmos.DrawRay(position + playerDot * direction, Vector3.up * 2f);
    }
    #endregion

    #region Blocking

    void InitializeBlockSequence()
    {
        blockQueue = new Queue<int>();

        string[] sequence = blockSequence.ToUpper().Split(' ');

        foreach (string s in sequence)
        {
            if (s == "X")
            {
                blockQueue.Enqueue(1);
            }
            else if (s == "O")
            {
                blockQueue.Enqueue(-1);
            }
        }

        this.attributes.health.max = this.attributes.health.current = blockQueue.Count * 4 + 1;
    }

    int GetNextInSequence()
    {
        if (blockQueue.TryDequeue(out int result))
        {
            return result;
        }
        else
        {
            return 0;
        }
    }

    public void NextBlock()
    {
        BlockType = GetNextInSequence();
    }

    public void TakeDamage(DamageKnockback damage)
    {
        if (!IsAlive()) return;
        if (DamageKnockback.IsFriendlyFire(this.attributes.friendlyGroup, damage.friendlyGroup)) return;

        if (this.IsTimeStopped())
        {
            TimeTravelController.time.TimeStopDamage(damage, this, 1);
            return;
        }

        bool hitFromBehind = !(Vector3.Dot(-this.transform.forward, (damage.source.transform.position - this.transform.position).normalized) <= 0f);

        lastDamageTaken = damage;

        if (IsBlocking() && !hitFromBehind)
        {
            bool success = (BlockType > 0 && damage.isSlash) || (BlockType < 0 && damage.isThrust);
            bool fail = !success && !damage.isRanged;
            if (success)
            {
                
                NextBlock();

                if (BlockType != 0)
                {
                    OnHit = true;
                    damage.OnBlock.Invoke();
                    OnBlock.Invoke();
                }
                else 
                // guardbreak
                {
                    Break();
                    this.OnHurt.Invoke();
                    damage.OnCrit.Invoke();
                    damage.didCrit = true;
                    
                }
                attributes.health.current-=4;
            }
            else if (fail && !damage.timeDelayed)
            {
                OnFail = true;
                damage.OnBlock.Invoke();
                OnBlock.Invoke();
                if (!damage.cannotRecoil && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.Recoil();
                }
            }
            else
            {
                OnHit = true;
                damage.OnBlock.Invoke();
                OnBlock.Invoke();
            }
        }
        else
        {
            this.OnHurt.Invoke();
            damage.OnHit.Invoke();
            attributes.health.current = 0;
            Die();
        }
    }

    public void Break()
    {
        GuardBreak = true;
        isBlocking = false;
        blockCollider.SetActive(false);
        shieldL.SetActive(false);
        shieldR.SetActive(false);
    }

    #endregion Blocking

    #region Attacking
    void GenerateHitboxes()
    {
        hitboxL = Hitbox.CreateHitbox(hitboxMountL.position, hitboxSize, hitboxMountL, riposteDamage, this.gameObject);
        hitboxR = Hitbox.CreateHitbox(hitboxMountR.position, hitboxSize, hitboxMountR, riposteDamage, this.gameObject);
    }

    public void HitboxActive(int active)
    {
        if (active == 0)
        {
            hitboxL.SetActive(false);
            hitboxR.SetActive(false);
        }
        else if (active == 1)
        {
            hitboxR.SetActive(true);
            OnHitboxActive.Invoke();
        }
        else if (active == 2)
        {
            hitboxL.SetActive(true);
            OnHitboxActive.Invoke();
        }
    }
    #endregion
    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void StartCritVulnerability(float time)
    {
        
    }
    public void StopCritVulnerability()
    {

    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void GetParried()
    {
        
    }
    public override bool IsBlocking()
    {
        return isBlocking;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public void StartInvulnerability(float duration)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInvulnerable()
    {
        return false; //TODO: implement invulnerability?
    }
}
