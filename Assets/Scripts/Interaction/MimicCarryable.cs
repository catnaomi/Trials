using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicCarryable : Carryable
{
    public MimicPotActor mimic;
    public Vector3 carryPosition;

    public override void SetCarryPosition(Vector3 position)
    {
        carryPosition = position;
    }
}
