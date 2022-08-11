using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StanceHandler
{
    public MixerTransition2DAsset blendStance;
    public AvatarMask blendMask;
    public float blendWeight;
    public bool additive;
}
