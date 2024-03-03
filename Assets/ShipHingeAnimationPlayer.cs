using Animancer;
using UnityEngine;

public class ShipHingeAnimationPlayer : MonoBehaviour
{
    public AnimancerComponent animancer;

    public ClipTransition fall;
    public ClipTransition hasFallen;

    public void PlayFall()
    {
        animancer.Play(fall);
    }
    public void PlayHasFallen()
    {
        animancer.Play(hasFallen);
    }
}
