using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthIndicator : MonoBehaviour
{
    public static BossHealthIndicator instance;
    public CanvasGroup group;
    public Image health;
    public Image damaged;
    public Image background;
    public Image nonactor;
    public float hp;
    public float damageHP;
    public float hpAdjustSpeed = 1f;
    public bool showing;
    public bool showHP;
    bool changed;
    public GameObject target;
    public Actor targetingActor;
    public UIImpulseReceiver impulseReceiver;
    Actor actor;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        showing = false;
        showHP = false;
        changed = false;
        if (target != null)
        {
            
            if (actor is NavigatingHumanoidActor navActor)
            {
                showing = navActor.actionsEnabled;
            }
            else
            {
                showing = true;
            }
            
            if (actor == null || actor.attributes.health.max <= 0f)
            {
                hp = 1f;
                damageHP = 1f;
                showHP = false;
            }
            else
            {
                float max = actor.attributes.health.max;
                hp = Mathf.Clamp01(actor.attributes.health.current / max);
                damageHP = Mathf.Clamp01(actor.attributes.smoothedHealth / max);
                showHP = true;
            }
        }
        if (target == null)
        {
            showing = false;
        }

        if (showing && group.alpha == 0)
        {
            group.alpha = 1f;
        }
        else if (!showing && group.alpha == 1)
        {
            group.alpha = 0f;
        }
    }
    // Update is called once per frame
    void OnGUI()
    {
        health.enabled = showHP;
        background.enabled = showing;
        damaged.enabled = showHP;
        nonactor.enabled = !showHP;
        health.fillAmount = (!changed) ? Mathf.MoveTowards(health.fillAmount, hp, hpAdjustSpeed * Time.deltaTime) : hp;
        damaged.fillAmount = damageHP;

        
        if (showing && target != null)
        {
            // do nothing
        }
        else if (target == null)
        {
            showing = false;
        }

        changed = false;
    }

    public void SetTargetLocal(GameObject target)
    {
        if (target == null) return;
        this.target = target;
        actor = this.target.GetComponent<Actor>();
        if (actor == null)
            actor = this.target.GetComponentInParent<Actor>();
        if (actor != null)
        {
            impulseReceiver.SetActor(actor);
        }
    }
    public static void SetTarget(GameObject target)
    {
        instance.SetTargetLocal(target);

    }

    public void HideLocal()
    {
        this.target = null;
        actor = null;
        impulseReceiver.SetActor(null);
    }
    public static void Hide()
    {
        instance.HideLocal();
    }

    public static GameObject GetTarget()
    {
        if (instance != null && instance.target != null)
        {
            return instance.target;
        }
        return null;
    }
}
