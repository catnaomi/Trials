using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BilayerMixer2DAsset", menuName = "Animancer/Mixer Transition/2D Bilayer", order = 3)]
public class BilayerMixer2DAsset : MixerTransition2DAsset
{
    public ClipTransition transition2;
    public AvatarMask mask;
    public float weight = 1f;
}
