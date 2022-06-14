using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Qi's Transforming Weapon", order = 1)]
public class TransformingWeapon : BladeWeapon
{

    TransformingWeaponModelHandler handler;
    DojoBossCombatantActor.WeaponState weaponState;


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

        thrustFX = FXController.CreateSwordThrust().GetComponent<LineSwordThrust>();
        thrustFX.pseudoParent = actor.transform;

        handler = GetModel().GetComponent<TransformingWeaponModelHandler>();

        UpdateTransformWeapon();
        GenerateHitboxes();
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);


        //slashMesh.transform.rotation = Quaternion.identity;
    }

    public void UpdateTransformWeapon()
    {
        DojoBossCombatantActor.WeaponState oldState = weaponState;
        weaponState = GetWeaponState();

        handler.state = weaponState;

        if (oldState != weaponState)
        {
            GenerateHitboxes();

            Transform currentModel = handler.GetCurrentModel().transform;
            top = InterfaceUtilities.FindRecursively(currentModel, "_top");
            bottom = InterfaceUtilities.FindRecursively(currentModel, "_bottom");

            slashFX.SetTopPoint(top);
            slashFX.SetBottomPoint(bottom);
            thrustFX.SetTopPoint(top);
            thrustFX.SetBottomPoint(bottom);
        }
    }

    public override float GetLength()
    {
        return stats[weaponState].length;
    }

    public override float GetWidth()
    {
        return stats[weaponState].width;
    }
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
        {DojoBossCombatantActor.WeaponState.Daox2, new WeaponStats(0.75f, 0.1f) },
        {DojoBossCombatantActor.WeaponState.Spear, new WeaponStats(1.5f, 0.2f) },

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
