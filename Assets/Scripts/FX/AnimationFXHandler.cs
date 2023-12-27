using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CustomUtilities;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

public class AnimationFXHandler : MonoBehaviour
{
    public AnimationFXSounds animSounds;
    public FXController.FXMaterial fxMaterial;
    [Header("Footsteps")]
    public AudioSource footSourceLight;
    public AudioSource footSourceHeavy;
    public float footstepDelay = 0.25f;
    float stepLTime;
    float stepRTime;
    [Space(10)]
    public Transform footL;
    public Transform footR;
    [Header("Swim")]
    public AudioSource waterSource;
    [Header("Combat")]
    public AudioSource combatWhiffSource;
    public AudioSource combatHitSource;
    public Vector3 parryPosition = Vector3.forward;
    [Header("Events")]
    public UnityEvent OnDust;
    public UnityEvent OnDashDust;
    public UnityEvent OnStepL;
    public UnityEvent OnStepR;
    [Header("Spiral")]
    public UnityEvent StartSpiral;
    public UnityEvent EndSpiral;
    [Header("Anim Events")]
    public UnityEvent OnArrowDraw;
    public UnityEvent OnArrowNock;
    [Space(10)]
    public UnityEvent OnGunLoad;
    Actor actor;
    bool didTypedBlock;
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<Actor>();
        actor.OnHurt.AddListener(ShowHitParticle);
        actor.OnBlock.AddListener(ShowBlockParticle);
        if (actor is PlayerActor player)
        {
            player.OnParrySlashStart.AddListener(ParrySlashStart);
            player.OnParryThrustStart.AddListener(ParryThrustStart);
            player.OnParrySuccess.AddListener(ParrySuccess);
            player.OnBlockTypeChange.AddListener(BlockSwitch);
            player.OnTypedBlockSuccess.AddListener(RegisterTypedBlock);
        }
    }

    #region Footsteps
    public void StepL(int heavy)
    {
        //AudioSource source = (heavy > 0) ? footSourceHeavy : footSourceLight;
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), true);
        if (Time.time - stepLTime > footstepDelay)
        {
            //Debug.Log(Time.time - stepLTime);
            source.PlayOneShot(clip);
            stepLTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
        OnStepL.Invoke();
    }

    public void StepL()
    {
        StepL(0);
    }
    public void StepR(int heavy)
    {
        //AudioSource source = (heavy > 0) ? footSourceHeavy : footSourceLight;
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), false);
        if (Time.time - stepRTime > footstepDelay)
        {
            //Debug.Log(Time.time - stepRTime);
            source.PlayOneShot(clip);
            stepRTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
        OnStepR.Invoke();
    }

    public void StepR()
    {
        StepR(0);
    }

    public void Tap()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.tap);
    }

    public void Thud()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.default_thud);
    }

    public void Dash()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.dash);
        OnDashDust.Invoke();
    }

    public void Slide()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.default_slide);
        OnDashDust.Invoke();
    }

    public void StartContinuousSlide()
    {
        
        if (footSourceHeavy.clip != animSounds.continuousSlide)
        {
            footSourceHeavy.Stop();
            footSourceHeavy.clip = animSounds.continuousSlide;
        }
        footSourceHeavy.loop = true;
        if (!footSourceHeavy.isPlaying)
        {
            footSourceHeavy.Play();
        }
    }

    public void StopContinuousSlide()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.loop = false;
    }

    public void Roll()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.default_roll);
        
    }

    public void Dust()
    {
        OnDust.Invoke();
    }

    #endregion

    #region Swimming

    public void Swim()
    {
        //waterSource.Stop();
        waterSource.PlayOneShot(animSounds.swim);
    }

    public void SplashBig()
    {
        waterSource.Stop();
        waterSource.PlayOneShot(animSounds.splashBig);
    }

    public void SplashSmall()
    {
        waterSource.Stop();
        waterSource.PlayOneShot(animSounds.splashSmall);
    }
    #endregion

    #region Combat

    public void SlashLight()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(animSounds.slashLight);
    }

    public void SlashHeavy()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(animSounds.slashHeavy);
    }

    public void ThrustLight()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(animSounds.thrustLight);
    }

    public void ThrustHeavy()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(animSounds.thrustHeavy);
    }

    public void ArrowDraw()
    {
        OnArrowDraw.Invoke();
    }

    public void ArrowNock()
    {
        combatWhiffSource.PlayOneShot(animSounds.bowPull);
        OnArrowNock.Invoke();
    }

    public void ArrowFire()
    {
        combatWhiffSource.Stop();
        combatHitSource.PlayOneShot(animSounds.bowFire);
    }

    public void GunFire()
    {
        combatWhiffSource.Stop();
        combatHitSource.PlayOneShot(animSounds.bowFire);
    }

    public void GunReload()
    {
        combatWhiffSource.Stop();
        combatHitSource.PlayOneShot(animSounds.bowPull);
        OnGunLoad.Invoke();
    }

    public void ChargeStart()
    {
        combatWhiffSource.Stop();
        combatHitSource.PlayOneShot(animSounds.chargeStart);
    }

    public void BlockSwitch()
    {
        if (actor is PlayerActor player)
        {
            Vector3 position = player.inventory.GetBlockWeapon().model.transform.position;
            Vector3 direction = Camera.main.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            if (player.IsBlockingSlash())
            {
                //FXController.CreateCross(position, direction);
                combatHitSource.PlayOneShot(animSounds.blockSwitch);
            }
            else if (player.IsBlockingThrust())
            {
                //FXController.CreateCircle(position, direction);
                combatHitSource.PlayOneShot(animSounds.blockSwitch);
            }
            
        }
    }

    public void ParrySlashStart()
    {
        Vector3 position = actor.transform.position +
                actor.transform.right * parryPosition.x +
                actor.transform.up * parryPosition.y +
                actor.transform.forward * parryPosition.z;
        Vector3 rotation = actor.transform.forward;
        FXController.CreateCross(position, rotation);
    }

    public void ParryThrustStart()
    {
        Vector3 position = actor.transform.position +
                actor.transform.right * parryPosition.x +
                actor.transform.up * parryPosition.y +
                actor.transform.forward * parryPosition.z;
        Vector3 rotation = actor.transform.forward;
        FXController.CreateCircle(position, rotation);
    }

    public void ParrySuccess()
    {
        if (actor is PlayerActor player)
        {
            Vector3 position = actor.transform.position +
                actor.transform.right * parryPosition.x +
                actor.transform.up * parryPosition.y +
                actor.transform.forward * parryPosition.z;
            Vector3 rotation = actor.transform.forward;
            FXController.CreateParrySuccess(position, rotation);
        }
    }
    #endregion

    #region Hit Particles

    public void ShowHitParticle()
    {
        DamageKnockback damage = actor.GetComponent<IDamageable>().GetLastTakenDamage();
        if (damage != null)
        {
            bool isCrit = damage.didCrit;
            bool isSlash = damage.isSlash;
            bool isThrust = damage.isThrust || (damage.isRanged && damage.GetTypes().HasType(DamageType.Piercing));

            if (isSlash || isThrust || damage.hitClip != null)
            {
                FXController.CreateBleed(actor.hitParticlePosition, actor.hitParticleDirection, isSlash, isCrit, fxMaterial, damage.hitClip);
                
            }
            FXController.DamageScreenShake(actor.hitParticleDirection, isCrit, false);
        }
       
    }

    public void RegisterTypedBlock()
    {
        didTypedBlock = true;
    }

    public void ShowBlockParticle()
    {
        DamageKnockback damage = actor.GetComponent<IDamageable>().GetLastTakenDamage();
        if (damage != null)
        {
            bool isCrit = damage.didCrit;
            bool isSlash = damage.isSlash;
            bool isThrust = damage.isThrust || (damage.isRanged && damage.GetTypes().HasType(DamageType.Piercing));

            if (isSlash || isThrust)
            {
                AudioClip clip = (isCrit || didTypedBlock) ? FXController.GetSwordCriticalSoundFromFXMaterial(FXController.FXMaterial.Metal) : FXController.GetSwordHitSoundFromFXMaterial(FXController.FXMaterial.Metal);
                FXController.CreateSpark(actor.hitParticlePosition, actor.hitParticleDirection, clip);
                FXController.DamageScreenShake(actor.hitParticleDirection, isCrit, true);
            }
            if (true)//didTypedBlock)
            {
                if (isSlash)
                {
                    FXController.CreateCross(actor.hitParticlePosition, actor.hitParticleDirection);
                }
                else if (isThrust)
                {
                    FXController.CreateCircle(actor.hitParticlePosition, actor.hitParticleDirection);
                }
                
            }
        }
        didTypedBlock = false;
    }

    #endregion
    #region Terrain Handling

    public AudioClip GetFootStepFromTerrain(string materialName, bool isLeft)
    {
        string material = materialName.ToLower();
        if (material.Contains("metal"))
        {
            return (isLeft) ? animSounds.metal_stepL : animSounds.metal_stepR;
        }
        else if (material.Contains("stone"))
        {
            return (isLeft) ? animSounds.stone_stepL : animSounds.stone_stepR;
        }
        else if (material.Contains("grass"))
        {
            return (isLeft) ? animSounds.grass_stepL : animSounds.grass_stepR;
        }
        else if (material.Contains("dirt"))
        {
            return (isLeft) ? animSounds.dirt_stepL : animSounds.dirt_stepR;
        }
        else if (material.Contains("tile"))
        {
            return (isLeft) ? animSounds.tile_stepL : animSounds.tile_stepR;
        }
        else if (material.Contains("ice"))
        {
            return (isLeft) ? animSounds.ice_stepL : animSounds.ice_stepR;
        }
        else if (material.Contains("water"))
        {
            return (isLeft) ? animSounds.water_stepL : animSounds.water_stepR;
        }
        else
        {
            return (isLeft) ? animSounds.default_stepL : animSounds.default_stepR;
        }
    }

    #endregion
#if (UNITY_EDITOR)
    public void PopulateWithDefaults()
    {
        Debug.Log("Populating AnimationSoundHandler on " + this);

        // footsteps
        animSounds.default_stepL = Resources.Load<AudioClip>("Sounds/Footsteps/tile-stepL");
        animSounds.default_stepR = Resources.Load<AudioClip>("Sounds/Footsteps/tile-stepR");
        animSounds.default_thud = Resources.Load<AudioClip>("Sounds/Footsteps/thud1");
        animSounds.dash = Resources.Load<AudioClip>("Sounds/Footsteps/dash1");
        animSounds.default_slide = Resources.Load<AudioClip>("Sounds/Footsteps/dirt-slide");
        animSounds.default_roll = Resources.Load<AudioClip>("Sounds/Footsteps/roll1");
        animSounds.tap = Resources.Load<AudioClip>("Sounds/Footsteps/tap1");
        // swim
        animSounds.swim = Resources.Load<AudioClip>("Sounds/Water/swim1");
        animSounds.splashBig = Resources.Load<AudioClip>("Sounds/Water/splash1");
        animSounds.splashSmall = Resources.Load<AudioClip>("Sounds/Water/splash2");
        // combat
        animSounds.slashLight = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_light");
        animSounds.slashHeavy = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_heavy");
        animSounds.thrustLight = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_light");
        animSounds.thrustHeavy = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_heavy");

        animSounds.bowPull = Resources.Load<AudioClip>("Sounds/Effects/bow-draw1");
        animSounds.bowFire = Resources.Load<AudioClip>("Sounds/Effects/bow-fire");
    }
#endif
}