using Animancer;
using UnityEngine;

public class AnimancerPlayEvent : MonoBehaviour
{
    public AnimancerComponent animancer;
    public ClipTransition clip;

    public void PlayClip()
    {
        animancer.Play(clip);
    }
}
