using UnityEngine;
using UnityEngine.Events;
using CustomUtilities;

public class AnimationFXHandler : MonoBehaviour
{
    public FXController.FXMaterial fxMaterial;

    public enum LeftOrRight
    {
        Left,
        Right,
    }

    [Header("Footsteps")]
    public AudioSource footSourceLight;
    public AudioSource footSourceHeavy;
    public float footstepDelay = 0.25f;
    float[] stepTimes = new float[2];
    [Space(10)]
    public Transform[] feet;
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
    public UnityEvent[] OnStep;
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

    void Awake()
    {
        actor = GetComponent<Actor>();
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

        OnStep = new[] {OnStepL, OnStepR};
    }

    private void Start()
    {
        if ((feet == null || feet.Length == 0) && this.TryGetComponent(out HumanoidPositionReference positionReference))
        {
            feet = new[] { positionReference.FootL, positionReference.FootR };
        }
    }
    #region Footsteps

    public void Step(LeftOrRight leftOrRight)
    {
        var footIndex = (int)leftOrRight;
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), leftOrRight);

        if (Time.time - stepTimes[footIndex] > footstepDelay)
        {
            source.PlayOneShot(clip);
            stepTimes[footIndex] = Time.time;
        }

        if (actor != null && actor.ShouldDustOnStep())
        {
            OnDust.Invoke();
        }
        OnStep[footIndex].Invoke();
        Debug.DrawRay(feet[footIndex].position, Vector3.up * 0.2f, leftOrRight == LeftOrRight.Left ? Color.blue : Color.red, 1f);
    }
    public void StepL()
    {
        Step(LeftOrRight.Left);
    }
    public void StepR()
    {
        Step(LeftOrRight.Right);
    }

    public void StepWeapon()
    {
        // TODO: step effect where weapon connects
    }

    public void Slide(int active)
    {
        footSourceHeavy.Stop();
        SoundFXAssetManager.PlaySound(footSourceHeavy, "Player/Slide/Slide");
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
        SoundFXAssetManager.PlaySound(footSourceHeavy, "Player/Tap");
    }

    public void Thud()
    {
        footSourceHeavy.Stop();
        SoundFXAssetManager.PlaySound(footSourceHeavy, "Player/Thud");
    }

    public void Dash()
    {
        footSourceHeavy.Stop();
        SoundFXAssetManager.PlaySound(footSourceHeavy, "Player/Dash");
        OnDashDust.Invoke();
    }

    public void StartContinuousSlide()
    {
        var continuousSlideSound = SoundFXAssetManager.GetSound("Player/Slide/Continuous");
        if (footSourceHeavy.clip != continuousSlideSound)
        {
            footSourceHeavy.Stop();
            footSourceHeavy.clip = continuousSlideSound;
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
        SoundFXAssetManager.PlaySound(footSourceHeavy, "Player/Roll");

    }

    public void Dust()
    {
        OnDust.Invoke();
    }

    #endregion
    #region Swimming

    public void Swim()
    {
        SoundFXAssetManager.PlaySound(waterSource, "Swim/Swim");
    }

    public void SplashBig()
    {
        waterSource.Stop();
        SoundFXAssetManager.PlaySound(waterSource, "Swim/Splash/Big");
    }

    public void SplashSmall()
    {
        waterSource.Stop();
        SoundFXAssetManager.PlaySound(waterSource, "Swim/Splash/Small");
    }

    #endregion
    #region Combat

    public void Swing(bool isSlash, bool isHeavy)
    {
        combatWhiffSource.Stop();
        SoundFXAssetManager.PlaySound(combatWhiffSource, isSlash ? "Slash" : "Thrust", isHeavy ? "Heavy" : "Light");
    }

    public void SlashLight()
    {
        Swing(true, false);
    }

    public void SlashHeavy()
    {
        Swing(true, true);
    }

    public void ThrustLight()
    {
        Swing(false, false);
    }

    public void ThrustHeavy()
    {
        Swing(false, true);
    }

    public void ArrowDraw()
    {
        OnArrowDraw.Invoke();
    }

    public void ArrowNock()
    {
        SoundFXAssetManager.PlaySound(combatWhiffSource, "Bow/Draw");
        OnArrowNock.Invoke();
    }

    public void ArrowFire()
    {
        combatWhiffSource.Stop();
        SoundFXAssetManager.PlaySound(combatHitSource, "Bow/Fire");
    }

    public void GunFire()
    {
        combatWhiffSource.Stop();
        SoundFXAssetManager.PlaySound(combatHitSource, "Gun/Fire");
    }

    public void GunReload()
    {
        combatWhiffSource.Stop();
        SoundFXAssetManager.PlaySound(combatHitSource, "Gun/Reload");
        OnGunLoad.Invoke();
    }

    public void ChargeStart()
    {
        combatWhiffSource.Stop();
        SoundFXAssetManager.PlaySound(combatHitSource, "Enemy/IceGiant/Charge");
    }

    public void BlockSwitch()
    {
        if (actor is PlayerActor player)
        {
            Vector3 direction = Camera.main.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            // TODO: i think it would be nice to have different sounds for the diff blocks
            if (player.IsBlockingSlash() || player.IsBlockingThrust())
            {
                SoundFXAssetManager.PlaySound(combatHitSource, "Player/Block/Switch");
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
            FXController.CreateParrySuccess(position, rotation, combatHitSource);
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
            FXController.CreateBleed(actor.hitParticlePosition, actor.hitParticleDirection, damage.isSlash, damage.didCrit, fxMaterial);
            FXController.DamageScreenShake(actor.hitParticleDirection, damage.didCrit, false);
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
                FXController.CreateBlock(actor.hitParticlePosition, Quaternion.identity, 1f, didTypedBlock);
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

    public AudioClip GetFootStepFromTerrain(string materialSteppedOn, LeftOrRight leftOrRight)
    {
        // Material corresponds to a physical material e.g. Water_Walkable
        // We need to reduce this to an FX Material to get its sound effect
        var materialNameSimplified = "Default";
        string[] allMaterials = typeof(FXController.FXMaterial).GetEnumNames();
        foreach (string material in allMaterials)
        {
            if (materialSteppedOn.ToLower().Contains(material.ToLower()))
            {
                materialNameSimplified = material;
                break;
            }
        }

        return SoundFXAssetManager.GetSound("Step", materialNameSimplified, leftOrRight.ToString());
    }

    #endregion
}