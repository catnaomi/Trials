using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    public Actor actor;

    public AttributeDisplay healthBar;
    public AttributeDisplay staminaBar;
    public AttributeDisplay hearts;
    public SimpleDamageDisplay damage;

    private void Start()
    {
        if (actor == null) actor = PlayerActor.player;
        healthBar.SetAttribute(actor.attributes.health);
        staminaBar.SetAttribute(actor.attributes.stamina);
        hearts.SetAttribute(actor.attributes.hearts);
        damage.SetActor(actor);
    }

    public void SetActor(Actor actor)
    {
        this.actor = actor;
        healthBar.SetAttribute(actor.attributes.health);
        staminaBar.SetAttribute(actor.attributes.stamina);
        hearts.SetAttribute(actor.attributes.hearts);
        damage.SetActor(actor);
    }
    private void OnGUI()
    {
        if (actor == null)
        {
            return;
        }


        healthBar.SetSmoothValue(actor.attributes.smoothedHealth);
        healthBar.UpdateGUI();

        staminaBar.SetSmoothValue(actor.attributes.smoothedStamina);
        staminaBar.UpdateGUI();

        hearts.UpdateGUI();
    }
}
