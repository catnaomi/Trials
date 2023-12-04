using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAttributeData
{
    public AttributeValue health;
    public AttributeValue timeCharges;

    public PlayerAttributeData()
    {
        health = new AttributeValue();
        timeCharges = new AttributeValue();
    }
    public void GetAttributeData(TimeTravelController time, ActorAttributes playerAttributes)
    {
        health.Copy(playerAttributes.health);
        timeCharges.Copy(time.charges);
    }
}
