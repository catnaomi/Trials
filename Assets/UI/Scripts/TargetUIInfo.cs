using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUIInfo : MonoBehaviour
{
    public GameObject target;
    public IHasHealthAttribute actor;
    public bool isActiveTarget;
    public TargetUIController controller;
    public Canvas canvas;
    [Space(20)]
    public CanvasGroup group;
    public CanvasGroup healthBarGroup;
    public float healthBarFadeTime = 1f;
    public Image health;
    public Image damaged;
    public Image activeTargetIcon;
    public Image inactiveTargetIcon;

    public void SetTarget(GameObject target)
    {
        GameObject targetActual = target;
        if (this.target != target)
        {
            this.target = target;
            if (target != null)
            {
                if (target.TryGetComponent<TargetUIMeta>(out TargetUIMeta meta))
                {
                    targetActual = meta.target;
                    if (meta.target != null)
                    {
                        actor = meta.target.GetComponent<IHasHealthAttribute>();
                    }
                }
                else
                {
                    targetActual = target.transform.root.gameObject;
                    actor = target.transform.root.GetComponent<IHasHealthAttribute>();
                }
                
            }

            if (targetActual != null && BossHealthIndicator.GetTarget() != null && targetActual == BossHealthIndicator.GetTarget())
            {
                actor = null;
            }
        }

    }

    private void OnGUI()
    {
        if (target == null)
        {
            group.alpha = 0f;
            healthBarGroup.alpha = 0f;
        }
        else
        {
            group.alpha = 1f;

            if (actor != null)
            {
                AttributeValue actorHealth = actor.GetHealth();
                float smoothedHealth = actor.GetSmoothedHealth();
                if (actorHealth.current != actorHealth.max)
                {
                    healthBarGroup.alpha = Mathf.MoveTowards(healthBarGroup.alpha, 1f, healthBarFadeTime * Time.deltaTime);
                    health.fillAmount = actorHealth.current / actorHealth.max;
                    damaged.fillAmount = smoothedHealth / actorHealth.max;
                }
                else
                {
                    healthBarGroup.alpha = Mathf.MoveTowards(healthBarGroup.alpha, 0f, healthBarFadeTime * Time.deltaTime);
                }
            }
            else
            {
                healthBarGroup.alpha = Mathf.MoveTowards(healthBarGroup.alpha, 0f, healthBarFadeTime * Time.deltaTime);
            }
            

            activeTargetIcon.enabled = controller.IsActiveTarget(target) && controller.IsTargeting();
            inactiveTargetIcon.enabled = controller.IsActiveTarget(target) && !controller.IsTargeting();
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            this.transform.position = Camera.main.WorldToScreenPoint(target.transform.position);
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }
    }
}
