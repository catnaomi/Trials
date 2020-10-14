﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(ActorAttributes))]
public class Actor : MonoBehaviour
{
    [Header("Actor Details")]
    public string actorName;
    public string actorTitle;
    public int actorId;
    [Space(5), HideInInspector]
    public ActorAttributes attributes;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public AudioSource audioSource;

    public InputAction CurrentAction;   

    [HideInInspector]
    public UnityEvent OnActionCommand;
    [HideInInspector]
    public UnityEvent OnActionStart;
    [HideInInspector]
    public UnityEvent OnActionEnd;
    [HideInInspector]
    public UnityEvent OnHurt;
    [HideInInspector]
    public UnityEvent OnHit;
    private  int mercyId; //hitbox

    // targets

    public GameObject CombatTarget;
    public GameObject FollowTarget;
    private void Awake()
    {
        OnHurt = new UnityEvent();
        OnHit = new UnityEvent();
        OnActionCommand = new UnityEvent();
        OnActionStart = new UnityEvent();
        OnActionEnd = new UnityEvent();

        ActorAwake();
    }

    protected virtual void ActorAwake()
    {
        // run after base class Awake()
    }
    public void Start()
    {
        CurrentAction = ActionsLibrary.GetInputAction(0);

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attributes = GetComponent<ActorAttributes>();
        ActorStart();
    }

    public virtual void ActorStart()
    {
        // run after base class Start()
    }

    public void Update()
    {
        ActorPreUpdate();

        ActorPostUpdate();
    }

    public virtual void ActorPreUpdate()
    {
        // run before base class 
    }

    public virtual void ActorPostUpdate()
    {
        // run after base class 
    }

    private void OnTriggerEnter(Collider other)
    {
        HitboxController hitbox;
        if (!other.TryGetComponent<HitboxController>(out hitbox))
        {
            return;
        }

        if (hitbox.GetSource() == this.transform.root)
        {
            return;
        }

        if (hitbox.id == mercyId)
        {
            return;
        }

        mercyId = hitbox.id;
        OnHurt.Invoke();
        hitbox.lastHitActor = this;
        hitbox.OnHit.Invoke();
        OnHitboxEnter(hitbox);
    }

    protected virtual void OnHitboxEnter(HitboxController hitbox)
    {
        // do nothing by default
        ProcessDamageKnockback(hitbox.damageKnockback);
    }

    public virtual void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        // do nothing by default
    }

    public virtual void TakeAction(InputAction action)
    {
        CurrentAction = action;
        animator.SetInteger("ActionType", action.animId);
        animator.SetBool("HasNewAction", true);
        //animator.SetTrigger("TakeAction");
        OnActionCommand.Invoke();
    }

    public void ClearAction()
    {
        CurrentAction = null;
        animator.SetBool("HasNewAction", false);
    }

    public InputAction GetLastAction()
    {
        return CurrentAction;
    }

    public GameObject GetCombatTarget()
    {
        return CombatTarget;
    }

    public void SetCombatTarget(GameObject target)
    {
        CombatTarget = target;
    }
    public GameObject GetFollowTarget()
    {
        return FollowTarget;
    }

    public void SetFollowTarget(GameObject target)
    {
        FollowTarget = target;
    }

    public virtual bool IsAlive()
    {
        return attributes.health.current > 0;
    }

    public virtual bool IsInjured()
    {
        return attributes.health.current == 0;
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
