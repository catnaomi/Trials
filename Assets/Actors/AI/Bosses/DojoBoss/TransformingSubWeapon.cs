using System.Collections;
using UnityEngine;

public class TransformingSubWeapon : BladeWeapon
{
    public override void EquipWeapon(Actor actor)
    {
        OnEquip.Invoke();
        holder = actor;

        slashFX = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashFX.pseudoParent = actor.transform;

        thrustFX = FXController.CreateSwordThrust().GetComponent<LineSwordThrust>();
        thrustFX.pseudoParent = actor.transform;


        GenerateHitboxes();
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);
    }
}