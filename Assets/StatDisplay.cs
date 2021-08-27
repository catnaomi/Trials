using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    public Actor actor;

    public float healthOffset;
    public RectTransform healthDisplay;
    public RectTransform healthSpentDisplay;
    public RectTransform healthMaxDisplay;
    public Text healthText;

    public float staminaOffset;
    public RectTransform staminaDisplay;
    public RectTransform staminaSpentDisplay;
    public RectTransform staminaMaxDisplay;
    public Text staminaText;

    public Text heartsText;

    private void Start()
    {
        actor = PlayerActor.player;
    }
    private void OnGUI()
    {
        if (actor == null)
        {
            return;
        }

        healthDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.health.current * .5f);
        healthSpentDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.smoothedHealth * .5f);
        healthMaxDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.health.max * .5f);
        healthText.text = Mathf.Floor(actor.attributes.health.current).ToString();

        staminaDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.stamina.current * 5f);
        staminaSpentDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.smoothedStamina * 5f);
        staminaMaxDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actor.attributes.stamina.max * 5f);
        staminaText.text = Mathf.Floor(actor.attributes.stamina.current).ToString();

        heartsText.text = Mathf.Floor(actor.attributes.hearts.current).ToString();
    }
}
