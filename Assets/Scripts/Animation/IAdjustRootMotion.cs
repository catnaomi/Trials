using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdjustRootMotion
{
    bool ShouldAdjustRootMotion();

    Vector3 GetAdjustmentRelativePosition();
}
