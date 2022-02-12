using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "holdatk0000_name", menuName = "ScriptableObjects/Attacks/Hold Attack", order = 1)]
public class HoldAttack : InputAttack
{
    [SerializeField] private ClipTransition start;
    public DamageKnockback heldAttackData = DamageKnockback.GetDefaultDamage();
    public ClipTransition GetStartClip()
    {
        return start;
    }

    public DamageKnockback GetHeldDamage()
    {
        return heldAttackData;
    }
}
