using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class CombatTargetPlayerController : MonoBehaviour
{
    Actor actor;
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<Actor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (actor.CombatTarget == null && PlayerActor.player != null)
        {
            actor.SetCombatTarget(PlayerActor.player.gameObject);
        }
    }
}
