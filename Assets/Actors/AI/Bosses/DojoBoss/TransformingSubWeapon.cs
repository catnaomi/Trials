using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Special/Qi's Transforming Sub-Weapon", order = 1)]
public class TransformingSubWeapon : BladeWeapon
{
    [HideInInspector] public TransformingWeapon primaryWeapon;
    public override void EquipWeapon(Actor actor)
    {
        OnEquip.Invoke();
        holder = actor;

        slashFX = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashFX.pseudoParent = actor.transform;

        thrustFX = FXController.CreateSwordThrust().GetComponent<SpiralSwordThrust>();
        thrustFX.pseudoParent = actor.transform;


        GenerateHitboxes();
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);

        if (actor.TryGetComponent<AnimationFXHandler>(out AnimationFXHandler animationFXHandler))
        {
            canReceiveAnimEvents = true;
            animationFXHandler.OnArrowDraw.AddListener(Draw);
            animationFXHandler.OnArrowNock.AddListener(Nock);
        }
        else
        {
            canReceiveAnimEvents = false;
        }

        if (!actor.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;

        Transform parent = positionReference.MainHand.transform;

        Vector3 dir = positionReference.MainHand.transform.parent.up;
        deadArrow.transform.position = parent.transform.position + dir * arrowLength;
        deadArrow.transform.rotation = Quaternion.LookRotation(dir);
        deadArrow.transform.SetParent(parent.transform, true);
    }

    public override void HitboxActive(bool active)
    {
        if (primaryWeapon.weaponState != DojoBossMecanimActor.WeaponState.Bow)
        {
            base.HitboxActive(active);
            return;
        }
        if (active && canFire)
        {
            Debug.Log("bow fire!!!");
            Fire();
            canFire = false;
        }

        if (!active && !canReceiveAnimEvents)
        {
            canFire = true;
        }
        nocked = false;
        if (deadArrow != null) deadArrow.SetActive(false);

    }

    #region RANGED WEAPON LOGIC

    public GameObject arrowPrefab;
    public GameObject deadArrowPrefab;
    public DamageKnockback bowDamageKnockback;
    public float fireStrengthMult = 100f;
    public float drawTime = 1f;
    float nockTime;
    bool canFire;
    ArrowController[] arrows;
    GameObject deadArrow;
    public int arrowCount = 4;
    bool canReceiveAnimEvents = false;
    public float arrowLength = 1f;
    bool nocked;
    LineRenderer line;


    public bool CanFire()
    {
        throw new System.NotImplementedException();
    }

    public void SetCanFire(bool fire)
    {
        throw new System.NotImplementedException();
    }

    public void SetStrength(float s)
    {
        throw new System.NotImplementedException();
    }

    public RangedWeapon.FireMode GetFireMode()
    {
        throw new System.NotImplementedException();
    }

    public override Bounds GetBlockBounds()
    {
        return primaryWeapon.GetBlockBounds();
    }
    public void Draw()
    {
        if (GetAmmunitionRemaining() > 0)
        {
            deadArrow.SetActive(true);
        }
    }

    public void Nock()
    {
        canFire = true;
        nockTime = Time.time;
        nocked = true;


    }
    public void Fire()
    {
        if (!GetHeldActor().TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;
        deadArrow.SetActive(false);
        GetHeldActor().SendMessage("ArrowFire");
        if (usesAmmunition && GetAmmunitionRemaining() <= 0)
        {
            return;
        }
        else if (usesAmmunition)
        {
            holder.GetComponent<Inventory>().RemoveOne(ammunitionReference);
        }

        Vector3 launchVector = GetHeldActor().GetLaunchVector(positionReference.MainHand.transform.position) + Vector3.up * 0.0f;

        if (holder.GetCombatTarget() != null)
        {
            // assist at dist 20 = 0.05
            // assist at dist 1 = 0

            float dist = Vector3.Distance(holder.GetCombatTarget().transform.position, holder.transform.position);

            Vector3 aimAssist = Vector3.zero;

            Debug.Log("aim assist: " + aimAssist.y * 100f);

            if (holder.GetCombatTarget().TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference hpr))
            {
                launchVector = (hpr.Spine.position - positionReference.OffHand.transform.position).normalized + aimAssist;
            }
            else
            {
                launchVector = (holder.GetCombatTarget().transform.position - positionReference.OffHand.transform.position).normalized + aimAssist;
            }

        }
        float launchStrength = fireStrengthMult;

        Vector3 origin = positionReference.MainHand.transform.position + positionReference.MainHand.transform.parent.up * arrowLength;
        ArrowController arrow = ArrowController.Launch(arrowPrefab, origin, Quaternion.LookRotation(launchVector), launchVector * launchStrength, holder.transform, this.bowDamageKnockback);
        arrow.Launch(origin, Quaternion.LookRotation(launchVector), launchVector * launchStrength, holder.transform, this.bowDamageKnockback);

        Collider[] arrowColliders = arrow.GetComponentsInChildren<Collider>();
        foreach (Collider actorCollider in holder.transform.GetComponentsInChildren<Collider>())
        {
            foreach (Collider arrowCollider in arrowColliders)
            {
                Physics.IgnoreCollision(actorCollider, arrowCollider);
            }
        }

    }

    public override GameObject GenerateModel()
    {
        deadArrow = GameObject.Instantiate(deadArrowPrefab);
        deadArrow.SetActive(false);
        GameObject obj = base.GenerateModel();
        line = model.GetComponentInChildren<LineRenderer>();
        return obj;
    }

    #endregion
}