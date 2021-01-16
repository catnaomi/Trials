using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Basic Attack", order = 1)]
public class InputAttack : InputAction
{
    /*
     * 0 is deliberately blank
     * 001-399   standard attacks
     * 001-099   1h right
     * 101-199   1h left
     * 201-299   2h
     * 301-399   2x
     * 
     * 
     */
    public int attackId;
    public bool isBlockOK; // can attack be initiated from block
    public bool isSprintOK; // can attack be initiated from sprint
    public bool isFallingOK; // can attack be initiated while falling

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
}
