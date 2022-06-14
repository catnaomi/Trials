using System.Collections;
using UnityEngine;

public interface IRangedWeapon
{
    public bool CanFire();
    public void SetCanFire(bool fire);
    public void SetStrength(float s);
    public RangedWeapon.FireMode GetFireMode();

}