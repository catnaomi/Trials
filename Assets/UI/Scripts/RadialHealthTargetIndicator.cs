using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialHealthTargetIndicator : MonoBehaviour
{
    public Canvas canvas;
    public Image health;
    public Image damaged;
    public Image background;
    public float hp;
    public float damageHP;
    public float hpRadialSpeed = 1f;
    public float haloDistance = 1f;
    public float camInitialDistance = 1f;
    public bool showing;
    public bool showHP;
    bool changed;
    Vector3 initialScale;
    public GameObject target;
    public Actor targetingActor;
    Actor actor;
    // Start is called before the first frame update
    void Start()
    {
        if (targetingActor == null)
        {
            targetingActor = PlayerActor.player;
        }
        initialScale = canvas.transform.localScale;
    }

    private void Update()
    {
        showing = false;
        showHP = false;
        changed = false;
        if (target != targetingActor.GetCombatTarget())
        {
            target = targetingActor.GetCombatTarget();
            changed = true;
        }
        if (target != null)
        {
            showing = true;
            
            actor = target.GetComponentInParent<Actor>();
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
            Vector3 dir = target.transform.position - Camera.main.transform.position;
            //dir.y = 0f;
            canvas.transform.position = target.transform.position + dir.normalized * haloDistance;
            canvas.transform.rotation = Quaternion.LookRotation(dir.normalized);
            float dist = Vector3.Distance(canvas.transform.position, Camera.main.transform.position);
            canvas.transform.localScale = initialScale * dist / camInitialDistance;
        }
        

    }
    // Update is called once per frame
    void OnGUI()
    {
        health.enabled = showHP;
        background.enabled = showing;
        damaged.enabled = showHP;
        health.fillAmount = (!changed) ? Mathf.MoveTowards(health.fillAmount, hp, hpRadialSpeed * Time.deltaTime) : hp;
        damaged.fillAmount = damageHP;

        
        if (showing)
        {
            RectTransform parent = GetComponentInParent<RectTransform>();
            Vector2 viewportPos = Camera.main.WorldToScreenPoint(target.transform.position);
            this.transform.position = viewportPos;//new Vector2(viewportPos.x * parent.rect.width, viewportPos.y * parent.rect.height);
        }

        changed = false;
    }


}
