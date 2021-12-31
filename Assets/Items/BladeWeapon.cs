using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CustomUtilities;

[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/Weapons/Create Blade Weapon", order = 1)]
public class BladeWeapon : EquippableWeapon, HitboxHandler
{
    public float baseDamage;
    public float heartsDamage;
    public float width = 1f;
    public float length = 1.5f;
    //public float weight = 1f;
    bool wall;
    bool active;
    //[HideInInspector] WeaponController weaponController;
    [HideInInspector] HitboxGroup hitboxes;

    TrailRenderer trailThrust;
    TrailRenderer trailSlash;
    ParticleSystem trailSystem;
    MeshSwordSlash slashMesh;


    public List<DamageType> elements;


    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);

        //weaponController = new WeaponController(this, ((HumanoidActor)holder).positionReference.MainHand, holder);

        GenerateHitboxes();

        SetTrails(false, false);
        hitboxes.OnHitTerrain.RemoveAllListeners();
        hitboxes.OnHitTerrain.AddListener(TerrainContact);
        hitboxes.OnHitWall.AddListener(WallContact);


        slashMesh = FXController.CreateSwordSlash().GetComponent<MeshSwordSlash>();
        slashMesh.topPoint = InterfaceUtilities.FindRecursively(GetModel().transform, "_top");
        slashMesh.bottomPoint = InterfaceUtilities.FindRecursively(GetModel().transform, "_bottom");
        slashMesh.pseudoParent = actor.transform;
        //slashMesh.transform.rotation = Quaternion.identity;
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);

        GameObject.Destroy(slashMesh);
        DestroyHitboxes();
    }

    public override void FixedUpdateWeapon(Actor actor)
    {
        //weaponController.WeaponUpdate();
    }

    public HitboxGroup GetHitboxes()
    {
        return hitboxes;
    }

    protected void GenerateHitboxes()
    {
        if (hitboxes != null)
        {
            hitboxes.DestroyAll();
        }
        hitboxes = Hitbox.CreateHitboxLine(
            GetHand().transform.position,
            GetHand().transform.forward,
            GetLength(),
            GetWidth(),
            GetHand().transform,
            new DamageKnockback(),
            holder.gameObject);
    }

    protected void DestroyHitboxes()
    {
        if (hitboxes == null)
        {
            return;
        }
        hitboxes.DestroyAll();
    }
    public void HitboxActive(bool active)
    {
        if (hitboxes == null || hitboxes.IsDestroyed())
        {
            GenerateHitboxes();
        }
        DamageKnockback dk = this.GetDamageFromAttack(holder);
        hitboxes.SetDamage(dk);
        hitboxes.SetActive(active);
        if (active)
        {
            slashMesh.transform.position = holder.transform.position;
            wall = false;
            //holder.attributes.ReduceAttribute(holder.attributes.stamina, this.GetPoiseCost(((HumanoidActor)holder).nextAttackType));
            slashMesh.SetTopPoint(InterfaceUtilities.FindRecursively(GetModel().transform, "_top"));
            slashMesh.SetBottomPoint(InterfaceUtilities.FindRecursively(GetModel().transform, "_bottom"));
            float staminaCost = this.GetStamCost() * 1;
            if (dk.isSlash)
            {
                holder.gameObject.SendMessage("SlashLight");
                slashMesh.BeginSlash();
            }
            else if (dk.isThrust)
            {
                holder.gameObject.SendMessage("ThrustLight");
            }
            
            /*
             * FXController.CreateFX(sound,
                ((HumanoidActor)holder).positionReference.MainHand.transform.position + (((HumanoidActor)holder).positionReference.MainHand.transform.forward * length),
                Quaternion.identity,
                1f);
                */

        }
        else
        {
            slashMesh.EndSlash();
        }
        //SetTrails(AttackIsThrusting(nextAttackType) && active, AttackIsSlashing(nextAttackType) && active);
        //SetTrailColor(dk.healthDamage.GetHighestType(DamageType.Slashing, DamageType.Piercing));
        this.active = active;
    }

    public float GetStamCost()
    {
        return (10 + 1 * GetWeight() + 15 * Mathf.Abs(GetBalance()));
    }

    public float GetStaminaDamage()
    {
        return (10f + 3f * GetWeight());
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
            case -1:
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
        return 1f;
    }

    public virtual float GetPiercingModifier()
    {
        return 1f;
    }

    public virtual float GetBaseDamage()
    {
        return baseDamage;
    }
    public virtual float GetModifiedDamage(bool slashing)
    {
        if (slashing)
        {
            return this.baseDamage * this.GetSlashingModifier();
        }
        else // piercing
        {
            return this.baseDamage * this.GetPiercingModifier();
        }
    }

    public virtual float GetModifiedHeartsDamage(bool slashing)
    {
        if (slashing)
        {
            return this.heartsDamage * this.GetSlashingModifier();
        }
        else // piercing
        {
            return this.heartsDamage * this.GetPiercingModifier();
        }
    }
    public DamageType[] GetModifiedDamageTypes(bool slashing)
    {
        DamageType[] types = new DamageType[this.elements.Count + 1];
        for (int i = 0; i < this.elements.Count; i++)
        {
            types[i] = this.elements[i];
        }
        types[this.elements.Count] = (slashing) ? DamageType.Slashing : DamageType.Piercing;
        return types;
    }
    
    public virtual float GetBasePoiseDamage()
    {
        return 25f + 5f * GetWeight();
    }

    public override DamageResistance[] GetBlockResistance()
    {
        return null;
    }

    public virtual List<DamageType> GetElements()
    {
        return elements;
    }

    private void SetTrails(bool thrust, bool slash)
    {
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

                //t.lifetimeMultiplier = 0.2f;

                //t.lifetime = 0.2f;
                trailSystem.GetComponent<FadeTrails>().Reset();

                trailSystem.Play();

            }
            else
            {
                var t = trailSystem.trails;

 
                trailSystem.GetComponent<FadeTrails>().StartFade();
                //t.lifetimeMultiplier = 0f;
                //t.lifetime = 0f;
            }
        }
    }

    private void SetTrailColor(DamageType type)
    {
        Color c = FXController.GetColorForDamageType(type);
        Color c2 = FXController.GetSecondColorForDamageType(type);
        if (trailThrust != null)
        {
            trailThrust.startColor = c2;
            trailThrust.endColor = new Color(c.r, c.g, c.b, 0f);
            //trailThrust.material.SetColor("_EmissionColor", c2);
        }
        if (trailSlash != null)
        {
            trailSlash.startColor = c2;
            trailSlash.endColor = new Color(c.r, c.g, c.b, 0f);
            //trailSlash.material.SetColor("_EmissionColor", c2);
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

    public static bool AttackIsSlashing(AttackType type)
    {
        return true;
        if (type == AttackType.SlashingLight ||
            type == AttackType.SlashingMedium ||
            type == AttackType.SlashingHeavy ||
            type == AttackType.SlashingCritical)
        {
            return true;
        }
        return false;
    }

    public static bool AttackIsThrusting(AttackType type)
    {
        return true;
        if (type == AttackType.ThrustingLight ||
            type == AttackType.ThrustingMedium ||
            type == AttackType.ThrustingHeavy ||
            type == AttackType.ThrustingCritical)
        {
            return true;
        }
        return false;
    }
    /*
    public virtual DamageKnockback GetDamageFromAttack(AttackType attackType, Actor actor)
    {

        /*
         * ATTACKS
         * 
         * LIGHTS:
         *   8 base poise damage
         *   + 10 on full thust/slash
         *   + 2 at base weight
         *   * range: 8-20+
         *   
         * MEDIUMS:
         *   20 base poise damage
         *   + 15 on full thust/slash
         *   + 10 at base weight
         *   * range: 20-45+
         *   
         * HEAVIES:
         *  30 base poise damage
         *   + 20 on full thust/slash
         *   + 15 at base weight
         *   * range: 30-65+
         *   * can knockdown
         *   
         * knockback:
         *    *  direction: out
         *    *  50 base
         *    *   + 0 on full thrust/slash
         *    *   + 25 at base weight  
         * 
         * Blocking an attack takes
         * (1.5x)
         * the poise cost of the attack.
         * 
         * Damage:
         *  Lights: 1 (1/3 heart)
         *  Mediums: 3 (1 heart)
         *  Heavies: 6 (2 hearts)
         *    
         *

        Vector3 heavyKB = new Vector3(0, 20f, 50f + 25f * GetWeight());
        Vector3 lightKB = new Vector3(0, 0, 20f);

        switch (attackType)
        {
            case AttackType.SlashingLight:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(true) * 1f,
                    heartsDamage = GetModifiedHeartsDamage(true) * 1f,
                    staminaDamage = GetStaminaDamage() * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(true),
                    criticalMultiplier = 1.2f,
                };           
            case AttackType.SlashingMedium:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(true) * 2f,
                    heartsDamage = GetModifiedHeartsDamage(true) * 2f,
                    staminaDamage = GetStaminaDamage() * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(true),
                    criticalMultiplier = 1.2f,
                };
            case AttackType.SlashingHeavy:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(true) * 3f,
                    heartsDamage = GetModifiedHeartsDamage(true) * 3f,
                    staminaDamage = GetStaminaDamage() * 10f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers =  new DamageKnockback.StaggerData()
                    {
                        onHit = DamageKnockback.StaggerType.Stumble,
                        onArmorHit = DamageKnockback.StaggerType.Stagger,
                        onCritical = DamageKnockback.StaggerType.Knockdown,
                        onInjure = DamageKnockback.StaggerType.Knockdown,
                        onKill = DamageKnockback.StaggerType.Knockdown,
                        onHelpless = DamageKnockback.StaggerType.Knockdown,
                    },
                    breaksArmor = true,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(true),
                    criticalMultiplier = 1.2f,
                };
            case AttackType.ThrustingLight:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(false) * 1f,
                    heartsDamage = GetModifiedHeartsDamage(false) * 1f,
                    staminaDamage = GetStaminaDamage() * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(false),
                    criticalMultiplier = 1.7f,
                };
            case AttackType.ThrustingMedium:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(false) * 2f,
                    heartsDamage = GetModifiedHeartsDamage(false) * 2f,
                    staminaDamage = GetStaminaDamage() * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(false),
                    criticalMultiplier = 1.7f,
                };
            case AttackType.ThrustingHeavy:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(false) * 3f,
                    heartsDamage = GetModifiedHeartsDamage(false) * 3f,
                    staminaDamage = GetStaminaDamage() * 10f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = new DamageKnockback.StaggerData()
                    {
                        onHit = DamageKnockback.StaggerType.Stumble,
                        onArmorHit = DamageKnockback.StaggerType.Stagger,
                        onCritical = DamageKnockback.StaggerType.Crumple,
                        onInjure = DamageKnockback.StaggerType.Knockdown,
                        onKill = DamageKnockback.StaggerType.Knockdown,
                        onHelpless = DamageKnockback.StaggerType.Knockdown,
                    },
                    breaksArmor = true,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(false),
                    criticalMultiplier = 1.7f,
                };
            case AttackType.ThrustingCritical:
                return new DamageKnockback()
                {
                    healthDamage = GetModifiedDamage(false) * 3f,
                    heartsDamage = GetModifiedHeartsDamage(false) * 3f,
                    staminaDamage = GetStaminaDamage() * 10f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    breaksArmor = true,
                    source = GetHeldActor().gameObject,
                    types = GetModifiedDamageTypes(false),
                    criticalMultiplier = 1.7f,
                    forceCritical = true,
                };
            case AttackType.Disarming:
                return new DamageKnockback()
                {
                    healthDamage = 0f,
                    heartsDamage = 0f,
                    staminaDamage = 0f,
                    kbForce = heavyKB.magnitude * Vector3.up * 10f,
                    staggers = new DamageKnockback.StaggerData()
                    {
                        onHit = DamageKnockback.StaggerType.Stumble,
                        onArmorHit = DamageKnockback.StaggerType.Stumble,
                        onCritical = DamageKnockback.StaggerType.Stumble,
                        onInjure = DamageKnockback.StaggerType.Stumble,
                        onKill = DamageKnockback.StaggerType.Stumble,
                        onHelpless = DamageKnockback.StaggerType.Stumble,
                    },
                    breaksArmor = true,
                    source = GetHeldActor().gameObject,
                    disarm = true
                };
            default:
                return new DamageKnockback();
        }
    }
*/

    public virtual DamageKnockback GetDamageFromAttack(Actor actor)
    {
        if (actor is IAttacker attacker)
        {
            DamageKnockback damage = attacker.GetCurrentDamage();
            damage.healthDamage = 100 * (damage.healthDamage / 100f) * (this.GetBaseDamage() / 100f);
            damage.AddTypes(this.elements.ToArray());
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
        return 100f;//10f * GetWeight();
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

    private void WallContact()
    {
        wall = true;
    }
    private void TerrainContact()
    {
        Hitbox contactBox = hitboxes.terrainContactBox;

        //Vector3 contactPoint = contactBox.hitTerrain.ClosestPoint(contactBox.collider.bounds.center);

        /*
        if (contactBox.hitTerrain.Raycast(
            new Ray(((HumanoidActor)holder).positionReference.MainHand.transform.position,(((HumanoidActor)holder).positionReference.MainHand.transform.forward)),
            out RaycastHit hit,
            length))
            */

        if (active && holder.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference))
        {
            Vector3 contactPoint = contactBox.hitTerrain.ClosestPointOnBounds((positionReference.MainHand.transform.position + positionReference.MainHand.transform.forward * (length / 2f)));

            FXController.CreateFX(FXController.FX.FX_Sparks,
                    contactPoint,
                    Quaternion.identity,
                    1f);

            if (wall && holder is PlayerMovementController player)
            {
                player.HitWall();
            }

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
