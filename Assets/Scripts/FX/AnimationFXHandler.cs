using UnityEngine;
using UnityEngine.Events;
using CustomUtilities;

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
    public UnityEvent OnSlideStart;
    public UnityEvent OnSlideEnd;
    [Header("Spiral")]
    public UnityEvent StartSpiral;
    public UnityEvent EndSpiral;
    [Header("Anim Events")]
    public UnityEvent OnArrowDraw;
    public UnityEvent OnArrowNock;
    [Header("Flashes")]
    public FlashRenderer flash;
    [Space(10)]
    public UnityEvent OnGunLoad;
    Actor actor;
    bool didTypedBlock;
    
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
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), true);
        if (Time.time - stepLTime > footstepDelay)
        {
            source.PlayOneShot(clip);
            stepLTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
        OnStepL.Invoke();
        Debug.DrawRay(footL.position, Vector3.up * 0.2f, Color.blue, 1f);
    }

    public void StepL()
    {
        StepL(0);
    }

    public void StepR(int heavy)
    {
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), false);
        if (Time.time - stepRTime > footstepDelay)
        {
            source.PlayOneShot(clip);
            stepRTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
        OnStepR.Invoke();
        Debug.DrawRay(footR.position, Vector3.up * 0.2f, Color.red, 1f);
    }

    public void StepR()
    {
        StepR(0);
    }

    public void StepWeapon()
    {
        // TODO: step effect where weapon connects
    }

    public void Slide(int active)
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(animSounds.default_slide);
        if (active > 0)
        {
            OnSlideStart.Invoke();
        }
        else
        {
            OnSlideEnd.Invoke();
        }
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
            Vector3 direction = Camera.main.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            if (player.IsBlockingSlash())
            {
                combatHitSource.PlayOneShot(animSounds.blockSwitch);
                FlashColor(new Color(1,1,1,0.5f));
            }
            else if (player.IsBlockingThrust())
            {
                combatHitSource.PlayOneShot(animSounds.blockSwitch);
                FlashColor(new Color(1, 1, 1, 0.5f));
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

    public void FlashColor(Color color)
    {
        if (flash != null)
        {
            flash.Flash(color);
        }
    }

    public void FlashWhite()
    {
        FlashColor(Color.white);
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
            if (actor is PlayerActor)
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
        string clipName = "default_step";
        string material = materialName.ToLower();
        string[] allMaterials = typeof(FXController.FXMaterial).GetEnumNames();
        foreach (string curMaterial in allMaterials)
        {
            var curMaterialLower = curMaterial.ToLower();
            if (material.Contains(curMaterialLower))
            {
                clipName = $"{curMaterialLower}_step";
                break;
            }
        }

        clipName += isLeft ? "L" : "R";
        var clipField = typeof(AnimationFXSounds).GetField(clipName);
        return (AudioClip)clipField.GetValue(animSounds);
    }

    #endregion
}