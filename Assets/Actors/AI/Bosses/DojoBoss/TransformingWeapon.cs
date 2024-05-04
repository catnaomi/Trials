using CustomUtilities;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Special/Qi's Transforming Weapon", order = 1)]
public class TransformingWeapon : BladeWeapon
{
    TransformingWeaponModelHandler handler;
    [HideInInspector]public DojoBossMecanimActor.WeaponState weaponState;

    public TransformingSubWeapon subWeapon;
    TransformingWeaponModelHandler subHandler;
    public override void EquipWeapon(Actor actor)
    {
        OnEquip.Invoke();
        holder = actor;
        
        slashFX = FXController.instance.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashFX.pseudoParent = actor.transform;

        thrustFX = FXController.instance.CreateSwordThrust().GetComponent<SpiralSwordThrust>();
        thrustFX.pseudoParent = actor.transform;

        GenerateHitboxes();
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);
    }
    
    public override Bounds GetBlockBounds()
    {
        GameObject currentModel = handler.GetCurrentModel();
        GameObject offModel = subHandler.GetCurrentModel();
        if (currentModel != null)
        {
            Transform blockTransform = InterfaceUtilities.FindRecursively(currentModel.transform, "_blockCollider");
            if (blockTransform != null && blockTransform.TryGetComponent<Collider>(out Collider collider))
            {
                return collider.bounds;
            }
        }
        else if (offModel != null)
        {
            Transform blockTransform = InterfaceUtilities.FindRecursively(offModel.transform, "_blockCollider");
            if (blockTransform != null && blockTransform.TryGetComponent<Collider>(out Collider collider))
            {
                return collider.bounds;
            }
        }
        return new Bounds();
    }

    Dictionary<DojoBossMecanimActor.WeaponState, WeaponStats> stats = new Dictionary<DojoBossMecanimActor.WeaponState, WeaponStats>()
    {
        {DojoBossMecanimActor.WeaponState.Quarterstaff, new WeaponStats(2f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.Scimitar, new WeaponStats(1f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.Greatsword, new WeaponStats(1.5f, 0.125f) },
        {DojoBossMecanimActor.WeaponState.Rapier, new WeaponStats(1f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.Hammer, new WeaponStats(1f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.Daox2, new WeaponStats(0.75f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.Spear, new WeaponStats(1.5f, 0.2f) },
        {DojoBossMecanimActor.WeaponState.Bow, new WeaponStats(1f, 0.25f) },
        {DojoBossMecanimActor.WeaponState.None, new WeaponStats(1.5f, 0.2f) }
    };

    struct WeaponStats
    {
        public float length;
        public float width;
        public WeaponStats(float length,float width)
        {
            this.length = length;
            this.width = width;
        }
    }
}