using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Status Effects/Create Instant Heal", order = 1)]
public class HealInstant : Effect
{
    public float amount;
    public override bool ApplyEffect(ActorAttributes attributes)
    {
        attributes.RecoverHealth(amount);
        return false;
    }
}