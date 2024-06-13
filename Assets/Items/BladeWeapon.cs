using UnityEngine;
using System.Collections;
using CustomUtilities;

[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Create Blade Weapon", order = 1)]
public class BladeWeapon : EquippableWeapon, IHitboxHandler
{
    public float baseDamage;
    public float width = 1f;
    public float length = 1.5f;
    public bool doubleSided;

    [Space(10)]
    [SerializeField]public float slashModifier = 1f;
    [SerializeField]public float thrustModifier = 1f;
    bool wall;
    bool active;
    [HideInInspector] protected HitboxGroup hitboxes;

    ParticleSystem trailSystem;
    protected MeshSwordSlash slashFX;
    protected SpiralSwordThrust thrustFX;

    protected Transform top;
    protected Transform bottom;
    public DamageType elements;

    DamageKnockback lastDK;
    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);

        GenerateHitboxes();

        SetTrails(false, false);
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);
        hitboxes.OnHitHitbox.AddListener(ClashContact);

        top = InterfaceUtilities.FindRecursively(GetModel().transform, "_top");
        bottom = InterfaceUtilities.FindRecursively(GetModel().transform, "_bottom");

        slashFX = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashFX.topPoint = top;
        slashFX.bottomPoint = bottom;
        slashFX.pseudoParent = actor.transform;
        if (slashFX.gameObject.scene != actor.gameObject.scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(slashFX.gameObject, actor.gameObject.scene);
        }

        thrustFX = FXController.CreateSwordThrust().GetComponent<SpiralSwordThrust>();
        thrustFX.topPoint = top;
        thrustFX.bottomPoint = bottom;
        thrustFX.pseudoParent = actor.transform;
        if (thrustFX.gameObject.scene != actor.gameObject.scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(thrustFX.gameObject, actor.gameObject.scene);
        }
    }

    public void UpdateFXPoints()
    {
        Actor actor = GetHeldActor();
        top = InterfaceUtilities.FindRecursively(GetModel().transform, "_top");
        bottom = InterfaceUtilities.FindRecursively(GetModel().transform, "_bottom");

        if (slashFX == null)
        {
            slashFX = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        }
        slashFX.topPoint = top;
        slashFX.bottomPoint = bottom;
        slashFX.pseudoParent = actor.transform;
        if (slashFX.gameObject.scene != actor.gameObject.scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(slashFX.gameObject, actor.gameObject.scene);
        }

        if (thrustFX == null)
        {
            thrustFX = FXController.CreateSwordThrust().GetComponent<SpiralSwordThrust>();
        }
        thrustFX.topPoint = top;
        thrustFX.bottomPoint = bottom;
        thrustFX.pseudoParent = actor.transform;
        if (thrustFX.gameObject.scene != actor.gameObject.scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(thrustFX.gameObject, actor.gameObject.scene);
        }
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);

        GameObject.Destroy(slashFX);
        DestroyHitboxes();
    }

    public HitboxGroup GetHitboxes()
    {
        return hitboxes;
    }

    public virtual void GenerateHitboxes()
    {
        if (hitboxes != null)
        {
            hitboxes.DestroyAll();
        }
        if (!doubleSided)
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
            GetHand().transform.position + (-1f * GetLength() * GetHand().transform.forward),
            GetHand().transform.forward,
            GetLength() * 2f,
            GetWidth(),
            GetHand().transform,
            new DamageKnockback(),
            holder.gameObject);
        }
    }

    protected void DestroyHitboxes()
    {
        if (hitboxes == null)
        {
            return;
        }
        hitboxes.DestroyAll();
    }

    public virtual void HitboxActive(bool active)
    {
        if (hitboxes == null || hitboxes.IsDestroyed())
        {
            GenerateHitboxes();
        }
        
        if (active)
        {
            DamageKnockback dk = this.GetDamageFromAttack(holder);
            dk.kbForce = DamageKnockback.GetKnockbackRelativeToTransform(dk.kbForce * baseDamage, holder.transform);
            dk.originPoint = GetModel().transform.position;
            slashFX.transform.position = holder.transform.position;
            wall = false;
            slashFX.SetTopPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_top"));
            slashFX.SetBottomPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_bottom"));
            thrustFX.SetTopPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_top"));
            thrustFX.SetBottomPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_bottom"));
            float staminaCost = this.GetStamCost() * 1;
            var animationFXHandler = holder.gameObject.GetComponent<AnimationFXHandler>();
            animationFXHandler.Swing(dk.isSlash, dk.fxData.isHeavyAttack);            
            if (dk.isSlash)
            {
                slashFX.BeginSlash();
            }
            else if (dk.isThrust)
            {
                thrustFX.BeginThrust();
            }
            hitboxes.SetDamage(dk);
        }
        else
        {
            slashFX.EndSlash();
            thrustFX.EndThrust();
        }
        
        hitboxes.SetActive(active);
        this.active = active;
    }

    public virtual void TrailsActive(bool active)
    {
        if (active)
        {
            DamageKnockback dk = this.GetDamageFromAttack(holder);

            slashFX.transform.position = holder.transform.position;
            wall = false;
            slashFX.SetTopPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_top"));
            slashFX.SetBottomPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_bottom"));
            thrustFX.SetTopPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_top"));
            thrustFX.SetBottomPoint(InterfaceUtilities.FindRecursivelyActiveOnly(GetModel().transform, "_bottom"));
            if (dk.isSlash)
            {
                holder.gameObject.SendMessage(dk.fxData.isHeavyAttack ? "SlashHeavy" : "SlashLight");
                slashFX.BeginSlash();
            }
            else if (dk.isThrust)
            {
                holder.gameObject.SendMessage(dk.fxData.isHeavyAttack ? "ThrustHeavy" : "ThrustLight");
                thrustFX.BeginThrust();
            }
        }
        else
        {
            slashFX.EndSlash();
            thrustFX.EndThrust();
        }
    }
    
    public override void FlashWarning()
    {
        GameObject fx = FXController.CreateBladeWarning();
        fx.transform.SetParent(bottom);
        fx.transform.localScale = Vector3.one;
        holder.StartCoroutine(FlashWarningRoutine(fx));
        Destroy(fx, 5f);
    }

    IEnumerator FlashWarningRoutine(GameObject fx)
    {
        float clock = 0f;
        float t;
        while (clock < 0.5f)
        {
            yield return null;
            if (fx == null || bottom == null || top == null) yield break;
            clock += Time.deltaTime;
            t = Mathf.Clamp01(clock / 0.5f);
            fx.transform.position = Vector3.Lerp(bottom.position, top.position, t);
        }
    }

    public float GetStamCost()
    {
        return 10 + 1 * GetWeight() + 15 * Mathf.Abs(GetBalance());
    }

    public float GetStaminaDamage()
    {
        return 10f + 3f * GetWeight();
    }

    public float GetPoiseFromAttack(AttackType type)
    {
        switch (type)
        {
            default:
            case AttackType.SlashingLight:
                return 0f;
            case AttackType.ThrustingLight:
                return 0f;
            case AttackType.SlashingMedium:
                return 10f;
            case AttackType.ThrustingMedium:
                return 10f;
            case AttackType.SlashingHeavy:
                return 25f;
            case AttackType.ThrustingHeavy:
                return 25f; 
        }
    }

    public GameObject GetHand()
    {
        switch (((IHumanoidInventory)GetInventory()).GetItemHand(this))
        {
            case Inventory.OffType:
                return GetPositionReference().OffHand;
            default:
                return GetPositionReference().MainHand;

        }
    }

    public virtual GameObject GetModel()
    {
        return model;
    }

    public virtual float GetWidth()
    {
        return width;
    }

    public virtual float GetLength()
    {
        return length;
    }

    public virtual float GetHiltLength()
    {
        return 0f;
    }

    public virtual float GetBladeLength()
    {
        return length;
    }

    public virtual float GetBalance()
    {
        return 0f;
    }

    public virtual float GetSlashingModifier()
    {
        return slashModifier;
    }

    public virtual float GetPiercingModifier()
    {
        return thrustModifier;
    }

    public virtual float GetBaseDamage()
    {
        return baseDamage;
    }

    public virtual float GetBasePoiseDamage()
    {
        return 25f + 5f * GetWeight();
    }

    public virtual DamageType GetElements()
    {
        return elements;
    }

    private void SetTrails(bool thrust, bool slash)
    {
        return;
        if (GetModel() == null) return;
        trailSystem = GetModel().GetComponentInChildren<ParticleSystem>();

        if (trailSystem == null)
        {
            GameObject trailPrefab = Resources.Load<GameObject>("Prefabs/trail_particle");
            GameObject trailObject = GameObject.Instantiate(trailPrefab, GetModel().transform);
            trailObject.transform.position = GetHand().transform.position + GetModel().transform.up * GetLength();
            trailSystem = trailObject.GetComponent<ParticleSystem>();
        }
        if (trailSystem != null)
        {
            if (thrust || slash)
            {
                var t = trailSystem.trails;
                trailSystem.GetComponent<FadeTrails>().Reset();
                trailSystem.Play();
            }
            else
            {
                var t = trailSystem.trails;
                trailSystem.GetComponent<FadeTrails>().StartFade();
            }
        }
    }

    public enum AttackType
    {
        None,               // 0
        SlashingLight,      // 1
        SlashingMedium,     // 2
        SlashingHeavy,      // 3
        ThrustingLight,     // 4
        ThrustingMedium,    // 5
        ThrustingHeavy,     // 6
        Bash,               // 7
        SlashingCritical,
        ThrustingCritical,
        Disarming,
    }

    public virtual DamageKnockback GetDamageFromAttack(Actor actor)
    {
        if (actor is IAttacker attacker)
        {
            DamageKnockback damage = attacker.GetLastDamage();
            if (damage.isSlash)
            {
                damage.healthDamage *= this.baseDamage * slashModifier;
            }
            else if (damage.isThrust)
            {
                damage.healthDamage *= this.baseDamage * thrustModifier;
            }
            else
            {
                damage.healthDamage *= this.baseDamage;
            }
            damage.AddTypes(this.elements);
            return damage;
        }
        return new DamageKnockback();
    }

    public float GetHeft()
    {
        return 1f / weight; 
    }

    public override float GetBlockPoiseDamage()
    {
        return 100f;
    }

    public float GetPoiseRecoveryRate()
    {
        // at base weight: (1 + (1/1)) / 2 = 1
        // at 2 weight: (1 + (1/2)) / 2 = 0.75
        // at 0.5 weight: (1 + (1/0.5)) / 2 = 1.5

        return (1f + GetHeft()) / 2f;
    }

    public float GetPoiseCost(AttackType attackType)
    {
        /*
         * at base weight: 5 + (5 * 1) = 10
         * at 2 weight: 5 + (5 * 2) = 15
         * at 0.5 weight: 5 + (5 * 0.5) = 7.5
         * 
         * light: +0
         * medium: +5
         * heavy: +10
         */

        float chargeCost = 0f;

        switch (attackType)
        {
            case AttackType.SlashingLight:
            case AttackType.ThrustingLight:
                chargeCost = 0f;
                break;
            case AttackType.SlashingMedium:
            case AttackType.ThrustingMedium:
                chargeCost = 5f;
                break;
            case AttackType.SlashingHeavy:
            case AttackType.ThrustingHeavy:
                chargeCost = 10f;
                break;
        }

        return 5f + (5f * weight) + chargeCost;
    }

    protected void ClashContact()
    {
        Hitbox contactBox = hitboxes.terrainContactBox;
        Hitbox otherBox = contactBox.clashedHitbox;

        DamageKnockback thisDamage = contactBox.damageKnockback;
        DamageKnockback otherDamage = otherBox.damageKnockback;


        if (thisDamage.isSlash && (otherDamage.isSlash || otherDamage.isThrust) && !otherDamage.cannotRecoil)
        {
            Vector3 contactPoint = ((contactBox.transform.position) + (otherBox.transform.position)) / 2f;
            FXController.CreateFX(FXController.FX.FX_Sparks,
                    contactPoint,
                    Quaternion.identity,
                    1f);
            if (otherDamage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Recoil();
            }
        }
    }
    protected void WallContact()
    {
        wall = true;
    }
    protected void TerrainContact()
    {
        Hitbox contactBox = hitboxes.terrainContactBox;

        if (active && holder.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference))
        {
            Vector3 contactPoint = contactBox.hitTerrain.ClosestPoint((positionReference.MainHand.transform.position + positionReference.MainHand.transform.forward * (length / 2f)));

            FXController.CreateFX(FXController.FX.FX_Sparks,
                    contactPoint,
                    Quaternion.identity,
                    1f);
            wall = false;
        }
    }

    public virtual int GetDurability()
    {
        return -999;
    }

    public void GetStatDifferencesWeaponComparison(BladeWeapon proposedWeapon, ref WeaponStatBlock statBlock)
    {
        statBlock.stat_Weight.comparisonValue = proposedWeapon.GetWeight();
        statBlock.stat_Length.comparisonValue = proposedWeapon.GetLength();
        statBlock.stat_Width.comparisonValue = proposedWeapon.GetWidth();

        statBlock.stat_Balance.comparisonValue = proposedWeapon.GetBalance();

        statBlock.stat_AttackSpeed.comparisonValue = proposedWeapon.GetAttackSpeed(false);
        if (proposedWeapon is CraftableWeapon craftWeapon)
        {
            statBlock.stat_Durability.comparisonValue = craftWeapon.GetDurability();
            statBlock.stat_Durability.compare = true;
        }

        statBlock.stat_Weight.compare = true;
        statBlock.stat_Length.compare = true;
        statBlock.stat_Width.compare = true;
        statBlock.stat_Balance.compare = true;
        statBlock.stat_AttackSpeed.compare = true;
    }

    public bool CanOffhandEquip()
    {
        return EquippableOff;
    }

 
}
