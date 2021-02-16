using UnityEngine;
using System.Collections;
using CustomUtilities;

public class InspectorDebugActor : MonoBehaviour
{
    HumanoidActor actor;

    public bool applyDamage;
    public bool applyOnDebug;
    public DamageKnockback damageKnockback;

    [Space(10)]
    public bool DrawMainWeapon;
    public bool SheathMainWeapon;
    [Space(10)]
    public bool attackOnDebug;
    public int AIAttackID;
    public bool AIAttack;

    [Space(10)]
    public bool resetAttributes;
    [Space(10)]
    public bool ledgeOnDebug;
    public bool ladderOnDebug;
    [Space(10)]
    public bool rotateMainWepOnDebug;
    public bool rotateOffWepOnDebug;
    public float weaponAngle;
    public bool spinWeapons;


    [Space(10)]
    [ReadOnly] public bool canMove;
    [ReadOnly] public bool blocking;
    [ReadOnly] public bool attacking;
    [ReadOnly] public bool armored;
    [ReadOnly] public bool isHitboxActive;
    [ReadOnly] public bool invulnerable;
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool isSprinting;
    [ReadOnly] public bool isAerial;

    public bool ForceBlock;
    private void Start()
    {
        actor = GetComponent<HumanoidActor>();
    }
    // Update is called once per frame
    void Update()
    {
        if (applyDamage || (applyOnDebug && Input.GetButtonDown("Debug")))
        {
            applyDamage = false;
            actor.ProcessDamageKnockback(damageKnockback);
        }

        if (DrawMainWeapon)
        {
            DrawMainWeapon = false;
            if (!GetComponent<Animator>().GetBool("Armed"))
            {
                GetComponent<Animator>().SetTrigger("Unsheath-Main");
            }
        }

        if (SheathMainWeapon)
        {
            SheathMainWeapon = false;
            if (GetComponent<Animator>().GetBool("Armed"))
            {
                GetComponent<Animator>().SetTrigger("Sheath-Main");
            }
        }
        if (AIAttack || (attackOnDebug && Input.GetButtonDown("Debug")))
        {
            AIAttack = false;

            actor.GetComponent<Animator>().SetInteger("AI-Attack-ID", AIAttackID);
            actor.GetComponent<Animator>().SetTrigger("AI-Attack");

            //actor.TakeAction(action);

            //Debug.Log(string.Format("actor '{0}' took action '{1}' with anim id '{2}'", actor.actorName, action.desc, action.id));
        }

        if (resetAttributes)
        {
            resetAttributes = false;
            GetComponent<ActorAttributes>().Reset();
        }

        canMove = actor.CanMove();

        if (ForceBlock)
        {
            actor.GetComponent<Animator>().SetBool("Blocking", true);
        }

        if (Input.GetButtonDown("Debug") && ledgeOnDebug)
        {
            bool snap = actor.GetComponent<Animator>().GetBool("LedgeSnap");
            actor.GetComponent<Animator>().SetBool("LedgeSnap", !snap);
        }
        else if (Input.GetButtonDown("Debug") && ladderOnDebug)
        {
            bool snap = actor.GetComponent<Animator>().GetBool("LadderSnap");
            actor.GetComponent<Animator>().SetBool("LadderSnap", !snap);

        }
        
        if (Input.GetButtonDown("Debug") && rotateMainWepOnDebug)
        {
            actor.RotateMainWeapon(weaponAngle);
        }
        if (Input.GetButtonDown("Debug") && rotateOffWepOnDebug)
        {
            actor.RotateOffWeapon(weaponAngle);
        }
        if (spinWeapons)
        {
            weaponAngle = (Time.time * 100) % 360f;
            actor.RotateMainWeapon(weaponAngle);
            actor.RotateOffWeapon(weaponAngle);
        }

        blocking = actor.IsBlocking();
        attacking = actor.IsAttacking();
        armored = actor.IsArmored();
        isHitboxActive = actor.IsHitboxActive();
        invulnerable = actor.isInvulnerable;
        isSprinting = actor.IsSprinting();
        isJumping = actor.IsJumping();
        isAerial = actor.IsAerial();
    }
}
