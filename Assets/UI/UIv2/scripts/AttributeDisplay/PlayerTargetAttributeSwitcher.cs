using System.Collections;
using UnityEngine;

public class PlayerTargetAttributeSwitcher : MonoBehaviour
{
    public ActorAttributes attributes;
    public StatDisplay statDisplay;
    public GameObject target;
    public bool display;
    public GameObject container;

    private void Update()
    {
        PlayerActor player = PlayerActor.player;
        if (player == null) return;
        if (player.GetCombatTarget() != target)
        {
            target = player.GetCombatTarget();
            if (target != null && target.transform.root.TryGetComponent<Actor>(out Actor actor))
            {
                display = true;
                attributes = actor.attributes;
                statDisplay.SetActor(actor);
                ((AttributeBar)statDisplay.healthBar).scale = (500f) / attributes.health.max; 
            }
            else
            {
                display = false;
            }
        }
    }

    private void OnGUI()
    {
        container.SetActive(display);
    }
}