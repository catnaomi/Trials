using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "BladeWeapon", menuName = "ScriptableObjects/CreateBladeWeapon", order = 1)]
public class BladeWeapon : EquippableWeapon, HitboxHandler
{
    public float width = 1f;
    public float length = 1.5f;
    //public float weight = 1f;
    bool wall;
    bool active;
    //[HideInInspector] WeaponController weaponController;
    [HideInInspector] HitboxGroup hitboxes;

    TrailRenderer trailThrust;
    TrailRenderer trailSlash;

    public Damage elementRatios;


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
                    staminaCost = (10 + 1 * GetWeight() + 15 * Mathf.Abs(GetBalance())) * 1;
                    sound = FXController.clipDictionary["sword_swing_light"];
                    break;
                case AttackType.ThrustingLight:
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
                    staminaCost = (10 + 1 * GetWeight() + 15 * Mathf.Abs(GetBalance())) * 1;
                    sound = FXController.clipDictionary["sword_swing_light"];
                    break;
                case AttackType.SlashingMedium:
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2;
                    staminaCost = (5 + 1 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1.5f;
                    sound = FXController.clipDictionary["sword_swing_medium"];
                    break;
                case AttackType.ThrustingMedium:
                    sound = FXController.clipDictionary["sword_swing_medium"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2;
                    staminaCost = (5 + 1 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1.5f;
                    break;
                case AttackType.SlashingHeavy:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 3;
                    staminaCost = (5 + 1 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2f;
                    break;
                case AttackType.ThrustingHeavy:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (10 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 3;
                    staminaCost = (5 + 1 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 2f;
                    break;
                case AttackType.Bash:
                    sound = FXController.clipDictionary["sword_swing_heavy"];
                    //poiseCost = (1 + 3 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
                    staminaCost = (5 + 1 * GetWeight() + 20 * Mathf.Abs(GetBalance())) * 1;
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
                return 50f;
            case AttackType.ThrustingMedium:
                return 50f;
            case AttackType.SlashingHeavy:
                return 100f;
            case AttackType.ThrustingHeavy:
                return 100f;
        }
    }


    // handle offhand attacks, should only happen if equipped to off hand as player
    public bool HandleInput(out InputAction action)
    {
        action = null;
        bool down = Input.GetButtonDown("Attack2");

        if (down)
        {
            //action = ActionsLibrary.GetInputAction(OffhandAttack);
            return true;
        }

        return false;
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

    public virtual float GetWeight()
    {
        return weight;
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

    public virtual Damage GetBaseDamage(bool slashing)
    {
        Damage baseDamage;
        if (slashing)
        {
            baseDamage = new Damage(2f + 1f * GetWeight() + 5 * Mathf.Pow(GetSlashingModifier(), 2) + 3 * GetBalance(), DamageType.Slashing);
        }
        else
        {
            baseDamage = new Damage(2f + 1f * GetWeight() + 5 * Mathf.Pow(GetPiercingModifier(), 2) + 3 * GetBalance(), DamageType.Piercing);
        }
        baseDamage.Add(elementRatios);
        return baseDamage;
    }
    
    public virtual float GetBasePoiseDamage()
    {
        return 25f + 5f * GetWeight();
    }

    public override Damage GetBlockResistance()
    {
        return new Damage().Add(elementRatios).SetRatio(DamageType.Slashing, 0.5f).SetRatio(DamageType.Piercing, 0.5f);
    }

    private void SetTrails(bool thrust, bool slash)
    {
        foreach (TrailRenderer trail in GetModel().GetComponentsInChildren<TrailRenderer>())
        {
            if (trail.gameObject.name == "trail_thrust")
            {
                trailThrust = trail;
            }
            else if (trail.gameObject.name == "trail_slash")
            {
                trailSlash = trail;
            }
        }
        if (trailThrust != null)
        {
            trailThrust.emitting = thrust;
        }
        if (trailSlash != null)
        {
            trailSlash.emitting = slash;
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
                    damage = GetBaseDamage(true).MultPotential(1),
                    staminaDamage = (10f + 3f * GetWeight()) * 1f,
                    poiseDamage = GetBasePoiseDamage() * 1f,//(10f + 6f * GetWeight()) * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.Stagger,
                };           
            case AttackType.SlashingMedium:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(true).MultPotential(2),
                    staminaDamage = (10f + 3f * GetWeight()) * 2f,
                    poiseDamage = GetBasePoiseDamage() * 2f,//(10f + 6f * GetWeight()) * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.HeavyStagger,
                };
            case AttackType.SlashingHeavy:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(true).MultPotential(3),
                    staminaDamage = (10f + 3f * GetWeight()) * 3f,
                    poiseDamage = GetBasePoiseDamage() * 3f,//(10f + 6f * GetWeight()) * 3f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.Knockdown,
                    breaksArmor = true,
                };
            case AttackType.ThrustingLight:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(false).MultPotential(1),
                    staminaDamage = (10f + 3f * GetWeight()) * 1f,
                    poiseDamage = GetBasePoiseDamage() * 1f,//(10f + 6f * GetWeight()) * 1f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            lightKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.Stagger,
                };
            case AttackType.ThrustingMedium:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(false).MultPotential(2),
                    staminaDamage = (10f + 3f * GetWeight()) * 2f,
                    poiseDamage = GetBasePoiseDamage() * 2f,//(10f + 6f * GetWeight()) * 2f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.HeavyStagger,
                };
            case AttackType.ThrustingHeavy:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(false).MultPotential(3),
                    staminaDamage = (10f + 3f * GetWeight()) * 3f,
                    poiseDamage = GetBasePoiseDamage() * 3f,//(10f + 6f * GetWeight()) * 3f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    minStaggerType = DamageKnockback.StaggerType.Flinch,
                    staggerType = DamageKnockback.StaggerType.Knockdown,
                    breaksArmor = true,
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
                    staggerType = DamageKnockback.StaggerType.Stun,
                };
            case AttackType.SlashingCritical:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(true).MultPotential(4),
                    staminaDamage = (10f + 3f * GetWeight()) * 3f,
                    poiseDamage = 999f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggerType = DamageKnockback.StaggerType.Knockdown,
                    breaksArmor = true,
                    unblockable = true,
                };
            case AttackType.ThrustingCritical:
                return new DamageKnockback()
                {
                    damage = GetBaseDamage(false).MultPotential(4),
                    staminaDamage = (10f + 3f * GetWeight()) * 3f,
                    poiseDamage = 999f,
                    kbForce = DamageKnockback.GetKnockbackRelativeToTransform
                        (
                            heavyKB,
                            actor.transform
                        ),
                    staggerType = DamageKnockback.StaggerType.Knockdown,
                    breaksArmor = true,
                    unblockable = true,
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
        return 10f * GetWeight();
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

    public bool CanOffhandEquip()
    {
        return EquippableOff;
    }
}
