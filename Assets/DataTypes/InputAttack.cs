using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Basic Attack", order = 1)]
public class InputAttack : InputAction
{
    public int attackId;
    public bool isBlockOK; // can attack be initiated from block
    public bool isSprintOK; // can attack be initiated from sprint
    public bool isFallingOK; // can attack be initiated while falling
    public bool isParryOK; // is attack a riposte or disarm?
    [SerializeField] protected ClipTransition anim;
    [Header("Attack Data")]
    public DamageKnockback attackData;
    public int GetAttackID()
    {
        return attackId;
    }

    public bool IsBlockOkay()
    {
        return isBlockOK;
    }

    public bool IsSprintOkay()
    {
        return isSprintOK;
    }

    public bool IsFallingOkay()
    {
        return isFallingOK;
    }

    public bool IsParryOkay()
    {
        return isParryOK;
    }
    public virtual ClipTransition GetClip()
    {
        return anim;
    }
}
