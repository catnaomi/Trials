using UnityEngine;
using System.Collections;

public interface OffHandWeapon
{

    bool HandleInput(out InputAction action);

    bool CanOffhandEquip();
}
