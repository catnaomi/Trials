using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class DizzyHumanoid : MonoBehaviour
{
    Actor actor;
    IDamageable damageable;
    public GameObject damageableObject;
    public Transform pseudoParent;
    public bool critVuln;
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, ReadOnly] private float timeRemaining;
    float maxTime;
    ParentConstraint parentConstraint;
    SpriteRenderer sprite;


    private void Start()
    {
        this.damageable = this.GetComponentInParent<IDamageable>();
        if (this.damageable == null)
        {
            if (this.transform.parent.TryGetComponent<Actor>(out Actor actor) && actor is IDamageable damageable)
            {
                this.damageable = damageable;
                this.damageableObject = actor.gameObject;
            }

        }
        else
        {
            damageableObject = this.transform.parent.gameObject;
        }
        sprite = this.GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        /*
        float t = Mathf.Clamp01(timeRemaining / maxTime);
        float a = curve.Evaluate(t);
        if (sprite != null)
        {
            Color color = sprite.color;
            color.a = a;
            sprite.color = color;
        }
        

        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
        */
        if (damageable != null)
        {
            critVuln = damageable.IsCritVulnerable();
            sprite.enabled = damageable.IsCritVulnerable();
        }
       
    }

    public void Init()
    {
        /*
        parentConstraint = this.GetComponent<ParentConstraint>();
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        SetPseudoParent(actor.GetComponent<HumanoidPositionReference>().Head);
        actor.OnCritVulnerable.AddListener(OnCritVulnerable);
        */
    }

    void OnCritVulnerable()
    {
        float time = 0f;//damageHandler.GetCritTime();
        SetTime(time);
    }
    public void SetPseudoParent(Transform parent)
    {
        pseudoParent = parent;
        ConstraintSource constraintSource = new() { sourceTransform = pseudoParent, weight = 1f };
        if (parentConstraint.sourceCount < 1)
        {
            parentConstraint.AddSource(constraintSource);
        }
        else
        {
            parentConstraint.SetSource(0, constraintSource);
        }
        parentConstraint.translationAtRest = Vector3.zero;
        parentConstraint.constraintActive = true;
    }

    public void SetTime(float time)
    {
        this.gameObject.SetActive(true);
        if (time > 0f)
        { 
            if (!actor.IsTimeStopped())
            {
                timeRemaining = maxTime = time;
            }
           
        }
        else
        {
            timeRemaining = 0f;
        }
        
    }

    public void SetActor(Actor actor, IDamageHandler handler)
    {
        this.actor = actor;
        //this.damageHandler = handler;
        Init();
    }

}
