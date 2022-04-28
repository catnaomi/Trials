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
    public bool baseClipLoops = true;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animation>();
        animancer = GetComponent<AnimancerComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animancer != null && (!animancer.IsPlayableInitialized || animancer.IsPlaying() != play))
        {
            if (play)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }
    }

    private void Stop()
    {
        animancer.Stop();
        animancer.transform.localPosition = Vector3.zero;
        animancer.transform.localRotation = Quaternion.identity;
    }

    private void Play()
    {
        animancer.transform.localPosition = Vector3.zero;
        animancer.transform.localRotation = Quaternion.identity;
        AnimancerState state = animancer.Play(clip);
        state.NormalizedTime = 0f;
        if (!baseClipLoops)
        {
            state.Events.OnEnd = () => { animancer.Stop(); };
        }
        
    }

    private void OnDestroy()
    {
        if (animancer.IsPlayableInitialized) animancer.Stop();
    }
}
