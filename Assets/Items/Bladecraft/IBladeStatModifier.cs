using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBladeStatModifier
{

    public Dictionary<string, float> GetStatMods();
    public DamageType[] GetAddedElements();
    public float GetWeight();
}