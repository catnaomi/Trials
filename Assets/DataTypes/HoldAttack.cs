using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "holdatk0000_name", menuName = "ScriptableObjects/Attacks/Hold Attack", order = 1)]
public class HoldAttack : InputAttack
{
    [Header("Hold Attack Settings")]
    [SerializeField] private ClipTransition start;
    [SerializeField] private ClipTransition loop;
    public float minDuration = 1f;
    public float chargeTime = 5f;
    public InputAttack unchargedAttack;
    public InputAttack chargedAttack;
    public ClipTransition GetStartClip()
    {
        return start;
    }

    public ClipTransition GetLoopClip()
    {
        return loop;
    }
}
