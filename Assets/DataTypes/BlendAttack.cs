using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Blend Attack", order = 1)]
public class BlendAttack : InputAttack
{
    [SerializeField] private ClipTransition blend;
    [SerializeField] private AvatarMask mask;
    [SerializeField] private float blendWeight;
    [SerializeField] private float fadeOutTime = 0f;
    public ClipTransition GetBlendClip()
    {
        return blend;
    }


    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {

        AnimancerState state = actor.animancer.Play(this.GetClip());
        actor.SetCurrentDamage(this.GetDamage());
        actor.animancer.Layers[HumanoidAnimLayers.BilayerBlend].SetMask(mask);
        actor.animancer.Layers[HumanoidAnimLayers.BilayerBlend].Weight = blendWeight;
        actor.animancer.Layers[HumanoidAnimLayers.BilayerBlend].Play(blend);
        if (fadeOutTime == 0)
        {
            state.Events.OnEnd = () =>
            {
                actor.animancer.Layers[HumanoidAnimLayers.BilayerBlend].Stop();
                endEvent();
            };
        }
        else
        {
            state.Events.OnEnd = () => {
                endEvent();
                actor.StartCoroutine(FadeOutBlend(actor.animancer, endEvent));
            };
            
        }
        return state;
    }

    IEnumerator FadeOutBlend(AnimancerComponent animancer, Action endEvent)
    {
        float t;
        float clock = 0f;
        while (clock < fadeOutTime)
        {
            yield return null;
            clock += Time.deltaTime;
            t = Mathf.Clamp01(clock / fadeOutTime);
            animancer.Layers[HumanoidAnimLayers.BilayerBlend].Weight = 1f - t;
        }
        animancer.Layers[HumanoidAnimLayers.BilayerBlend].Stop();
        
    }
}
