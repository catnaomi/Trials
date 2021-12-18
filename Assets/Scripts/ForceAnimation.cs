using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Animator),typeof(AnimancerComponent))]
public class ForceAnimation : MonoBehaviour
{
    Animation anim;
    AnimancerComponent animancer;
    public AnimationClip clip;
    public bool play = true;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animation>();
        animancer = GetComponent<AnimancerComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animancer.IsPlaying() != play)
        {
            if (play)
            {
                animancer.Play(clip);
            }
            else
            {
                animancer.Stop();
            }
        }
    }

    private void OnDisable()
    {
        animancer.Stop();
    }
}
