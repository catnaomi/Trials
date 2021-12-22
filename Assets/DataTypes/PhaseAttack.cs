using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Phase Attack", order = 1)]
public class PhaseAttack : InputAttack
{
    [SerializeField] private ClipTransition loop;
    [SerializeField] private ClipTransition end;

    public ClipTransition GetLoopPhaseClip()
    {
        return loop;
    }
    public ClipTransition GetEndPhaseClip()
    {
        return end;
    }

}
