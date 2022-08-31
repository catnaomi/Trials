using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDamageDisplay : MonoBehaviour
{
    public Actor actor;
    public Text text;
    public float damage;
    public float fadeDelay;
    public float fadeDuration;
    public bool damageThisFrame = false;
    float timer;

    public void Start()
    {
        damage = 0f;
        timer = fadeDelay + fadeDuration;
    }
    public void SetActor(Actor actor)
    {
        if (this.actor != null)
        {
            this.actor.OnHurt.RemoveListener(OnDamage);
        }
        this.actor = actor;
        actor.OnHurt.AddListener(OnDamage);
    }

    private void OnDamage()
    {
        if (!damageThisFrame)
        {
            if (timer > fadeDelay + fadeDuration)
            {
                damage = actor.lastDamageAmountTaken;
            }
            else
            {
                damage += actor.lastDamageAmountTaken;
            }
            timer = 0f;
            damageThisFrame = true;
        }

    }

    public void OnGUI()
    {
        float alpha = Mathf.Lerp(1f, 0f, (timer - fadeDelay) / fadeDuration);
        text.color = new Color(1f, 1f, 1f, alpha);
        text.text = Mathf.Floor(damage).ToString();
        if (alpha <= 0)
        {
            damage = 0f;
        }
        if (timer <= fadeDelay + fadeDuration)
        {
            timer += Time.deltaTime;
        }
    }

    public void Update()
    {
        damageThisFrame = false;
    }

}