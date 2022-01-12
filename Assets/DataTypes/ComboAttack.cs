using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "comboatk0000_name", menuName = "ScriptableObjects/Attacks/Combo Attack", order = 1)]
public class ComboAttack : InputAttack
{
    [SerializeField] private ClipTransition[] sequence;
    [SerializeField] private float[] exitTimes;
    public DamageKnockback[] damages;
    public override ClipTransition GetClip()
    {
        return sequence[0];
    }

    public ClipTransition GetClip(int i)
    {
        if (i < 0 || i >= sequence.Length) { return sequence[0]; }
        return sequence[i];
    }

    public bool HasNext(int index)
    {
        if (index < sequence.Length - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetExitTime(int index)
    {
        if (index < exitTimes.Length)
        {
            return exitTimes[index];
        }
        return -1f;
    }

    public DamageKnockback GetDamage(int index)
    {
        if ((index - 1) <= 0) return damages[0];
        if ((index - 1) >= damages.Length) return damages[damages.Length - 1];
        return damages[index-1];
    }

    public override DamageKnockback GetDamage()
    {
        return GetDamage(0);
    }
}
