using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "comboatk0000_name", menuName = "ScriptableObjects/Attacks/Combo Attack", order = 1)]
public class ComboAttack : InputAttack
{
    [SerializeField] private ClipTransition[] sequence;
    [SerializeField] private float[] exitTimes;

    public override ClipTransition GetClip()
    {
        return sequence[0];
    }

    public ClipTransition GetClip(int i)
    {
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

}
