using UnityEngine;
using System.Collections;
using CustomUtilities;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Gun", menuName = "ScriptableObjects/Weapons/Create Gun", order = 1), SerializeField]
public class RangedGun : RangedWeapon, IHitboxHandler
{
    public DamageKnockback damageKnockback;
    public IKHandler ikHandler;
    public int ammoCapacity;
    [ReadOnly] public int ammoCurrent;
    public float maxDistance;
    [Tooltip("Accuracy Measured as radius of circle that is maximum variation of launch angle.")]
    public float accuracy;

    bool loaded;
    bool canFire;
    bool canReceiveAnimEvents;
    Transform muzzle;

    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);
        canFire = true;
        LoadGun();
        if (actor.TryGetComponent<AnimationFXHandler>(out AnimationFXHandler animationFXHandler))
        {
            canReceiveAnimEvents = true;
            animationFXHandler.OnGunLoad.AddListener(LoadGun);
        }
        else
        {
            canReceiveAnimEvents = false;
        }

        if (!actor.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);
        //DespawnArrows();
        if (actor.TryGetComponent<AnimationFXHandler>(out AnimationFXHandler animationFXHandler))
        {
            animationFXHandler.OnArrowDraw.RemoveListener(LoadGun);
        }
    }

    public override void UpdateWeapon(Actor actor)
    {
        base.UpdateWeapon(actor);
    }

    public void LoadGun()
    {
        int remaining = GetAmmunitionRemaining();
        if (remaining > 0)
        {
            ammoCurrent = holder.GetComponent<Inventory>().RemoveNumber(ammunitionReference, ammoCapacity);
        }
    }

    public bool ShouldReload()
    {
        return ammoCurrent == 0 && GetAmmunitionRemaining() > 0;
    }
    public void Fire()
    {
        if (!GetHeldActor().TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;


        
        if (ammoCurrent <= 0)
        {
            if (GetAmmunitionRemaining() <= 0)
            {
                return;
            }
            else
            {
                LoadGun();
                GetHeldActor().SendMessage("GunReload");
                return;
            }
        }
        else
        {
            ammoCurrent--;
        }
        GetHeldActor().SendMessage("GunFire");

        Vector3 launchVector = GetHeldActor().GetLaunchVector(positionReference.MainHand.transform.position);
        Vector2 accuracyVector = Random.insideUnitCircle * accuracy;

        launchVector += Vector3.up * accuracyVector.y;
        launchVector += holder.transform.right * accuracyVector.x;

        RaycastHit[] hits = Physics.RaycastAll(muzzle.transform.position, launchVector, maxDistance, LayerMask.GetMask("Default", "Terrain", "Actors") | MaskReference.Terrain);
        RaycastHit hit = new RaycastHit();
        float leadingDist = Mathf.Infinity;
        Vector3 endPoint = muzzle.transform.position + launchVector.normalized * maxDistance;
        foreach (RaycastHit rhit in hits)
        {
            if (rhit.distance < leadingDist && rhit.collider.transform.root != holder.transform.root)
            {
                hit = rhit;
                leadingDist = rhit.distance;
            }
        }
        if (hit.collider != null)
        {
            endPoint = hit.point;
            FXController.instance.CreateFX(FXController.FX.FX_Sparks, hit.point, Quaternion.LookRotation(launchVector), 1f);
            if (hit.collider.transform.root.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                DamageKnockback dk = new DamageKnockback(this.damageKnockback);
                dk.source = holder.gameObject;
                dk.originPoint = muzzle.transform.position;
                damageable.TakeDamage(dk);
            }
            else
            {
                //FXController.instance.CreateFX(FXController.FX.FX_Sparks, hit.point, Quaternion.LookRotation(launchVector), 1f);
            }
        }
        FXController.instance.CreateGunTrail(muzzle.transform.position, endPoint, muzzle.transform.forward, 5f, null);
    }
    public override bool CanFire()
    {
        return canFire;
    }

    public override void SetCanFire(bool fire)
    {
        canFire = fire;
    }
    public void HitboxActive(bool active)
    {
        if (active && canFire)
        {
            Debug.Log("gun fire!!!");
            Fire();
            canFire = false;
        }

        if (!active && !canReceiveAnimEvents)
        {
            canFire = true;
        }

    }


    public override GameObject GenerateModel()
    {
        GameObject obj = base.GenerateModel();
        muzzle = CustomUtilities.InterfaceUtilities.FindRecursively(obj.transform, "_muzzle");
        return obj;
    }

    public override void DestroyModel()
    {
        base.DestroyModel();
        muzzle = null;
    }
}
