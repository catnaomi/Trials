using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Special/Qi's Transforming Weapon", order = 1)]
public class TransformingWeapon : BladeWeapon
{
    TransformingWeaponModelHandler handler;
    [HideInInspector]public DojoBossCombatantActor.WeaponState weaponState;

    public TransformingSubWeapon subWeapon;
    TransformingWeaponModelHandler subHandler;
    public override void EquipWeapon(Actor actor)
    {
        OnEquip.Invoke();
        holder = actor;
        
        if (holder is DojoBossCombatantActor dojoboss)
        {
            dojoboss.OnWeaponTransform.AddListener(UpdateTransformWeapon);
        }

        slashFX = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashFX.pseudoParent = actor.transform;

        thrustFX = FXController.CreateSwordThrust().GetComponent<SpiralSwordThrust>();
        thrustFX.pseudoParent = actor.transform;


        handler = GetModel().GetComponent<TransformingWeaponModelHandler>();

        GenerateHitboxes();
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);

        subWeapon = ScriptableObject.Instantiate(subWeapon);
        
        subWeapon.prefab = this.prefab;
        subWeapon.baseDamage = this.baseDamage;
        subWeapon.width = 0.25f;
        subWeapon.length = 1f;
        subWeapon.slashModifier = 1f;
        subWeapon.thrustModifier = 1f;
        subWeapon.elements = this.elements;
        subWeapon.EquippableOff = true;
        subWeapon.OneHanded = true;
        subWeapon.OffHandEquipSlot = Inventory.EquipSlot.lHip;
        
        subWeapon.primaryWeapon = this;
        holder.GetComponent<HumanoidNPCInventory>().Add(subWeapon);
        holder.GetComponent<HumanoidNPCInventory>().EquipOffHandWeapon(subWeapon);
        holder.GetComponent<HumanoidNPCInventory>().EquipRangedWeapon(subWeapon);

        subHandler = subWeapon.GetModel().GetComponent<TransformingWeaponModelHandler>();
        UpdateTransformWeapon();

        
        //slashMesh.transform.rotation = Quaternion.identity;
    }

    public void UpdateTransformWeapon()
    {
        DojoBossCombatantActor.WeaponState oldState = weaponState;
        weaponState = GetWeaponState();
        handler = GetModel().GetComponent<TransformingWeaponModelHandler>();
        subHandler = subWeapon.GetModel().GetComponent<TransformingWeaponModelHandler>();

        if (weaponState == DojoBossCombatantActor.WeaponState.Bow)
        {
            handler.state = DojoBossCombatantActor.WeaponState.None;
        }
        else if (handler.state != weaponState)
        {
            handler.state = weaponState;
            GenerateHitboxes();


            GameObject currentModel = handler.GetCurrentModel();
            if (currentModel != null)
            {
                top = InterfaceUtilities.FindRecursively(currentModel.transform, "_top");
                bottom = InterfaceUtilities.FindRecursively(currentModel.transform, "_bottom");

                slashFX.SetTopPoint(top);
                slashFX.SetBottomPoint(bottom);
                thrustFX.SetTopPoint(top);
                thrustFX.SetBottomPoint(bottom);
            }
            

            
        }
        if (weaponState == DojoBossCombatantActor.WeaponState.Daox2)
        {
            subHandler.state = DojoBossCombatantActor.WeaponState.Daox2;
        }
        else if (weaponState == DojoBossCombatantActor.WeaponState.Bow)
        {
            subHandler.state = DojoBossCombatantActor.WeaponState.Bow;
        }
        else
        {
            subHandler.state = DojoBossCombatantActor.WeaponState.None;
        }
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

    /*
    public override float GetLength()
    {
        return stats[weaponState].length;
    }

    public override float GetWidth()
    {
        return stats[weaponState].width;
    }
    */

    public DojoBossCombatantActor.WeaponState GetWeaponState()
    {
        if (holder is DojoBossCombatantActor boss)
        {
            return boss.weaponState;
        }
        else
        {
            return DojoBossCombatantActor.WeaponState.Quarterstaff;
        }
    }

    protected override void GenerateHitboxes()
    {
        if (hitboxes != null)
        {
            hitboxes.DestroyAll();
        }

        if (weaponState != DojoBossCombatantActor.WeaponState.Quarterstaff)
        {
            hitboxes = Hitbox.CreateHitboxLine(
            GetHand().transform.position,
            GetHand().transform.forward,
            GetLength(),
            GetWidth(),
            GetHand().transform,
            new DamageKnockback(),
            holder.gameObject);
        }
        else
        {
            hitboxes = Hitbox.CreateHitboxLine(
            GetHand().transform.position + GetHand().transform.forward * GetLength() * -0.5f,
            GetHand().transform.forward,
            GetLength(),
            GetWidth(),
            GetHand().transform,
            new DamageKnockback(),
            holder.gameObject);
        }

    }

    Dictionary<DojoBossCombatantActor.WeaponState, WeaponStats> stats = new Dictionary<DojoBossCombatantActor.WeaponState, WeaponStats>()
    {
        {DojoBossCombatantActor.WeaponState.Quarterstaff, new WeaponStats(2f, 0.25f) },
        {DojoBossCombatantActor.WeaponState.Scimitar, new WeaponStats(1f, 0.25f) },
        {DojoBossCombatantActor.WeaponState.Greatsword, new WeaponStats(1.5f, 0.125f) },
        {DojoBossCombatantActor.WeaponState.Rapier, new WeaponStats(1f, 0.25f) },
        {DojoBossCombatantActor.WeaponState.Hammer, new WeaponStats(1f, 0.25f) },
        {DojoBossCombatantActor.WeaponState.Daox2, new WeaponStats(0.75f, 0.25f) },
        {DojoBossCombatantActor.WeaponState.Spear, new WeaponStats(1.5f, 0.2f) },
        {DojoBossCombatantActor.WeaponState.Bow, new WeaponStats(1f, 0.25f) },
         {DojoBossCombatantActor.WeaponState.None, new WeaponStats(1.5f, 0.2f) }
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