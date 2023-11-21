using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasHealthAttribute
{
    public AttributeValue GetHealth();
    public float GetSmoothedHealth();
}
