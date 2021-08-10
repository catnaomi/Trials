using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/CreateBladeWeapon", order = 1)]
public class BladeWeapon : EquippableWeapon, HitboxHandler
{
    public float baseDamage;
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
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);

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
        AttackType nextAttackType = ((HumanoidActor)holder).nextAttackType;
        DamageKnockback dk = this.GetDamageFromAttack(nextAttackType, holder);
        hitboxes.SetDamage(dk);
        hitboxes.SetActive(active);
        if (active)
        {
            wall = false;
            //holder.attributes.ReduceAttribute(holder.attributes.stamina, this.GetPoiseCost(((HumanoidActor)holder).nextAttackType));

            AudioClip sound;
            float staminaCost = 0f;
            switch (nextAttackType)
            {
                default:
                case AttackType.SlashingLight:
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
                    staminaCost = this.GetStamCost() * 1;
                    sound = FXController.clipDictionary["sword_swing_light"];
                    break;
                case AttackType.ThrustingLight:
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
                    staminaCost = this.GetStamCost() * 1;
                    sound = FXController.clipDictionary["sword_swing_light"];
                    break;
                case AttackType.SlashingMedium:
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2;
                    staminaCost = this.GetStamCost() * 1.5f;
                    sound = FXController.clipDictionary["sword_swing_medium"];
                    break;
                case AttackType.ThrustingMedium:
                    sound = FXController.clipDictionary["sword_swing_medium"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2;
                    staminaCost = this.GetStamCost() * 1.5f;
                    break;
                case AttackType.SlashingHeavy:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 3;
                    staminaCost = this.GetStamCost() * 2f;
                    break;
                case AttackType.ThrustingHeavy:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 3;
                    staminaCost = this.GetStamCost() * 2f;
                    break;
                case AttackType.Bash:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (1 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
                    staminaCost = this.GetStamCost() * 1;
                    break;
            }

            holder.attributes.ReduceAttribute(holder.attributes.stamina, staminaCost);
            //holder.attributes.ReduceAttributeToMin(holder.attributes.poise, poiseCost, 20f);
            holder.PlayAudioClip(sound);

            /*
             * FXController.CreateFX(sound,
                ((HumanoidActor)holder).positionReference.MainHand.transform.position + (((HumanoidActor)holder).positionReference.MainHand.transform.forward * length),
                Quaternion.identity,
                1f);
                */

        }
        SetTrails(AttackIsThrusting(nextAttackType) && active, AttackIsSlashing(nextAttackType) && active);
        SetTrailColor(dk.damage.GetHighestType(DamageType.Slashing, DamageType.Piercing));
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
        switch (GetHumanoidHolder().inventory.GetItemHand(this))
        {
            case -1:
                return GetHumanoidHolder().positionReference.OffHand;
            default:
                return GetHumanoidHolder().positionReference.MainHand;

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
    public virtual Damage GetModifiedDamage(bool slashing)
    {
        if (slashing)
        {
            Damage damage = new Damage(this.baseDamage * this.GetSlashingModifier(), elements);
            damage.types.Add(DamageType.Slashing);
            return damage;
        }
        else // piercing
        {
            Damage damage = new Damage(this.baseDamage * this.GetPiercingModifier(), elements);
            damage.types.Add(DamageType.Slashing);
            return damage;
        }
    }
    
    public virtual float GetBasePoiseDamage()
    {
        return 25f + 5f * GetWeight();
    }

    public override Damage GetBlockResistance()
    {
        return new Damage();
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
         */

        Vector3 heavyKB = new Vector3(0, 20f, 50f + 25f * GetWeight());
        Vector3 lightKB = new Vector3(0, 0, 20f);

        switch (attackType)
        {
            case AttackType.SlashingLight:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(true).MultPotential(1),
                    staminaDamage = GetStaminaDamage() * 1f,
                    poiseDamage = GetBasePoiseDamage() * 1f,//(10f + 6f * GetWeight()) * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHumanoidHolder().gameObject,
                };           
            case AttackType.SlashingMedium:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(true).MultPotential(2),
                    staminaDamage = GetStaminaDamage() * 2f,
                    poiseDamage = GetBasePoiseDamage() * 2f,//(10f + 6f * GetWeight()) * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.SlashingHeavy:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(true).MultPotential(3),
                    staminaDamage = GetStaminaDamage() * 3f,
                    poiseDamage = GetBasePoiseDamage() * 3f,//(10f + 6f * GetWeight()) * 3f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    breaksArmor = true,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.ThrustingLight:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(false).MultPotential(1),
                    staminaDamage = GetStaminaDamage() * 1f,
                    poiseDamage = GetBasePoiseDamage() * 1f,//(10f + 6f * GetWeight()) * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.ThrustingMedium:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(false).MultPotential(2),
                    staminaDamage = GetStaminaDamage() * 2f,
                    poiseDamage = GetBasePoiseDamage() * 2f,//(10f + 6f * GetWeight()) * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.ThrustingHeavy:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(false).MultPotential(3),
                    staminaDamage = GetStaminaDamage() * 3f,
                    poiseDamage = GetBasePoiseDamage() * 3f,//(10f + 6f * GetWeight()) * 3f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    breaksArmor = true,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.Bash:
                return new DamageKnockback()
                {
                    damage = new Damage(1f, DamageType.Earth),
                    staminaDamage = (10f + 3f * GetWeight()) * 1f,
                    poiseDamage = (10f + 6f * GetWeight()) * 3f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.SlashingCritical:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(true).MultPotential(4),
                    staminaDamage = GetStaminaDamage() * 3f,
                    poiseDamage = 999f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    breaksArmor = true,
                    unblockable = true,
                    source = GetHumanoidHolder().gameObject,
                };
            case AttackType.ThrustingCritical:
                return new DamageKnockback()
                {
                    damage = GetModifiedDamage(false).MultPotential(4),
                    staminaDamage = GetStaminaDamage() * 3f,
                    poiseDamage = 999f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggers = DamageKnockback.StandardStaggerData,
                    breaksArmor = true,
                    unblockable = true,
                    source = GetHumanoidHolder().gameObject,
                };
            default:
                return new DamageKnockback();
        }
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

        if (active)
        {
            Vector3 contactPoint = contactBox.hitTerrain.ClosestPointOnBounds(((HumanoidActor)holder).positionReference.MainHand.transform.position + (((HumanoidActor)holder).positionReference.MainHand.transform.forward * (length / 2f)));

            FXController.CreateFX(FXController.FX.FX_Sparks,
                    contactPoint,
                    Quaternion.identity,
                    1f);

            if (wall)
            {
                ((HumanoidActor)holder).HitWall();
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
