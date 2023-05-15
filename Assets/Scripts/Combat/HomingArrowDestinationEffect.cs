using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingArrowDestinationEffect : MonoBehaviour
{
    public Projectile projectileController;

    private void LateUpdate()
    {
        if (projectileController is HomingGroundProjectileController homingArrow)
        {
            this.transform.position = homingArrow.targetPoint;
        }
        else if (projectileController is BezierProjectileController bezierArrow && bezierArrow.controlPoints.Length > 0)
        {
            this.transform.position = bezierArrow.controlPoints[bezierArrow.controlPoints.Length - 1];
        }
    }
}
