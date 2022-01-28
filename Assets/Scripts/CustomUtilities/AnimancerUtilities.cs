using Animancer;
using System.Collections;
using UnityEngine;

namespace CustomUtilities
{
    public class AnimancerUtilities
    {
        public static AnimationClip GetCurrentClip(AnimancerComponent animancer)
        {
            if (animancer.States.Current != null)
            {
                return GetHighestWeightStateRecursive(animancer.States.Current).Clip;
            }
            return null;
        }

        public static AnimancerState GetHighestWeightStateRecursive(AnimancerState animState)
        {
            if (animState is not MixerState mixerState)
            {
                return animState;
            }
            else
            {
                float highWeight = 0f;
                AnimancerState leadingState = null;
                foreach (AnimancerState childState in mixerState.ChildStates)
                {
                    if (childState != null && childState.Weight > highWeight)
                    {
                        highWeight = childState.Weight;
                        leadingState = childState;
                    }
                }
                if (leadingState != null)
                {
                    return GetHighestWeightStateRecursive(leadingState);
                }
                return null;
            }
        }
    }
}