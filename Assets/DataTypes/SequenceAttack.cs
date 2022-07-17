using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Sequence Attack", order = 1)]
public class SequenceAttack : InputAttack
{
    [SerializeField] private InputAttack[] sequence;
    public override ClipTransition GetClip()
    {
        return sequence[0].GetClip();
    }


    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {
        if (sequence.Length <= 0) return null;

        AnimancerState PlayIndex(int index)
        {
            if (index < sequence.Length - 1)
            {
                return sequence[index].ProcessHumanoidAction(actor, () => PlayIndex(index + 1));
            }
            else
            {
                return sequence[index].ProcessHumanoidAction(actor, endEvent);
            }
        }

        return PlayIndex(0);
    }

    
}
