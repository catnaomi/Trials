using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    // TODO: figure out a way to future-proof this/ make more dynamic

    public GameObject fx_block;
    public GameObject fx_hit;
    public GameObject fx_stagger;
    public GameObject fx_sparks;
    public GameObject fx_slash;
    public GameObject fx_thrust;
    public GameObject fx_dizzy;
    public GameObject fx_warn;
    public GameObject fx_miragia;
    public GameObject fx_miragia_sound;
    public GameObject fx_parry_success;
    [Space(5)]
    public GameObject fx_gunTrail;
    [Space(5)]
    public GameObject fx_spiral;
    [Space(5)]
    public GameObject fx_circle;
    public GameObject fx_cross;

    public static FXController main;

    [Header("Damage Colors")]
    public Color trueColor;
    public Color trueColor2;
    public Color fireColor;
    public Color fireColor2;

    [Header("Screen Shake")]
    public CinemachineImpulseSource impulse;
    public float hitImpulseMagnitude = 0.2f;
    public float critImpulseMagnitude = 0.4f;
    public float blockImpulseMagnitude = 0.5f;

    [Header("Slash & Thrust By Material")]
    public GameObject fx_bleedSword;
    public GameObject fx_bleedPoint;
    [Space(10)]
    public GameObject fx_iceSlash;
    public GameObject fx_icePoint;

    ParticleSystem healParticle;
    public enum FX {
        FX_Block,
        FX_Hit,
        FX_Stagger,
        FX_Sparks,
        FX_BleedSword,
        FX_BleedPoint,
    }

    public enum FXMaterial
    {
        Blood,
        Metal,
        Wood,
        Stone,
        Dirt,
        Glass,

        Ice,
    }
    public static Dictionary<FX, GameObject> fxDictionary;
    public static Dictionary<string, AudioClip> clipDictionary;
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (main != null) return;
        fxDictionary = new Dictionary<FX, GameObject>()
        {
            { FX.FX_Block,  this.fx_block },
            { FX.FX_Hit, this.fx_hit },
            { FX.FX_Stagger, this.fx_stagger },
            { FX.FX_Sparks, this.fx_sparks },
            { FX.FX_BleedSword, this.fx_bleedSword },
            { FX.FX_BleedPoint, this.fx_bleedPoint },
        };

        clipDictionary = new Dictionary<string, AudioClip>()
        {
            { "sword_swing_light",  Resources.Load<AudioClip>("Sounds/Effects/sword_swing1") },
            { "sword_swing_medium", Resources.Load<AudioClip>("Sounds/Effects/sword_swing1") },
            { "sword_swing_heavy", Resources.Load<AudioClip>("Sounds/Effects/sword_swing1") },

            { "sword_hit_light",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_hit_light") },
            { "sword_hit_heavy", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_hit_heavy") },


            { "shield_bash",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bash") },
            { "shield_bash_hit", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bash_hit") },

            { "metal_clash", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_clash") },

            { "bow_draw",  Resources.Load<AudioClip>("Sounds/Effects/bow-draw1") },
            { "bow_fire", Resources.Load<AudioClip>("Sounds/Effects/bow-fire") },
            { "bow_hit", Resources.Load<AudioClip>("Sounds/Effects/bow-hit1") },

            { "parry_start", Resources.Load<AudioClip>("Sounds/Effects/sword-parry01") },
            { "parry_success", Resources.Load<AudioClip>("Sounds/Effects/sword-parry02") },

            // material on hits
            // blood, metal, wood, stone, dirt, glass

            { "sword_blood",  Resources.Load<AudioClip>("Sounds/Effects/sword-bleed1") },
            { "sword_blood_crit",  Resources.Load<AudioClip>("Sounds/Effects/sword-bleed2") },

            { "sword_metal",  Resources.Load<AudioClip>("Sounds/Effects/metal-hit2") },
            { "sword_metal_crit",  Resources.Load<AudioClip>("Sounds/Effects/stone-break1") },

            { "sword_wood",  Resources.Load<AudioClip>("Sounds/Effects/metal-hit1") },
            { "sword_wood_crit",  Resources.Load<AudioClip>("Sounds/Effects/wood-break1") },

            { "sword_bleed",  Resources.Load<AudioClip>("Sounds/Effects/sword-bleed1") },
        };

        if (PlayerActor.player != null)
        {
            healParticle = PlayerActor.player.transform.Find("_healparticle").GetComponent<ParticleSystem>();
        }

        main = this;
    }
    public static GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration)
    {
        return CreateFX(name, position, rotation, duration, null);
    }

    public static GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration, AudioClip audioClipOverwrite)
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(fxDictionary[name], position, rotation);

        if (audioClipOverwrite != null)
        {
            AudioSource audioSource = newFX.GetComponentInChildren<AudioSource>();

            //audioSource.playOnAwake = true;
            //audioSource.Stop();
            audioSource.clip = audioClipOverwrite;
            audioSource.Play();
        }

        GameObject.Destroy(newFX, duration);
        return newFX;
    }

    public static GameObject CreateSwordSlash()
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_slash);
        return newFX;
    }

    public static GameObject CreateSwordThrust()
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_thrust);
        return newFX;
    }

    public static GameObject CreateDizzy()
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_dizzy);
        return newFX;
    }

    public static GameObject CreateBladeWarning()
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_warn);
        return newFX;
    }

    public static GameObject CreateSpiral()
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_spiral);
        return newFX;
    }
    public static GameObject CreateGunTrail(Vector3 start, Vector3 end, Vector3 direction, float duration, AudioClip soundOverride)
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_gunTrail);
        newFX.transform.position = start;
        newFX.transform.rotation = Quaternion.LookRotation(direction);

        LineRenderer line = newFX.GetComponentInChildren<LineRenderer>();
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        GameObject.Destroy(newFX, duration);

        return newFX;
    }

    public static void CreateSpark(Vector3 position, Vector3 direction, AudioClip soundOverride)
    {
        EnsureSingleton();
        CreateFX(FXController.FX.FX_Sparks, position, Quaternion.LookRotation(-direction), 1f, soundOverride);
    }

    public static void CreateCross(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(main.fx_cross, position, direction);
    }

    public static void CreateCircle(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(main.fx_circle, position, direction);
    }

    public static void CreateParrySuccess(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(main.fx_parry_success, position, direction);
    }

    public static GameObject CreateMiragiaParticleSingle(Vector3 position)
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_miragia);
        newFX.transform.position = position;

        return newFX;
    }

    public static GameObject CreateMiragiaParticleSingleSound(Vector3 position)
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(main.fx_miragia_sound);
        newFX.transform.position = position;

        return newFX;
    }

    static void CreateFromPrefab(GameObject prefab, Vector3 position, Vector3 direction)
    {
        EnsureSingleton();
        GameObject newFX = GameObject.Instantiate(prefab);
        newFX.transform.position = position;
        newFX.transform.rotation = Quaternion.LookRotation(direction);
    }

    public static AudioClip GetSwordHitSoundFromFXMaterial(FXMaterial material)
    {
        switch (material)
        {
            default:
            case FXMaterial.Blood:
                return clipDictionary["sword_blood"];
            case FXMaterial.Metal:
                return clipDictionary["sword_metal"];
            case FXMaterial.Wood:
                return clipDictionary["sword_wood"];
        }
    }

    public static AudioClip GetSwordCriticalSoundFromFXMaterial(FXMaterial material)
    {
        switch (material)
        {
            default:
            case FXMaterial.Blood:
                return clipDictionary["sword_blood_crit"];
            case FXMaterial.Wood:
                return clipDictionary["sword_wood_crit"];
            case FXMaterial.Metal:
                return clipDictionary["sword_metal_crit"];
        }
    }

    public static Color GetColorForDamageType(DamageType type)
    {
        switch (type)
        {
            default:
                return main.trueColor;
            case DamageType.Fire:
                return main.fireColor;
        }
    }

    public static Color GetSecondColorForDamageType(DamageType type)
    {
        switch (type)
        {
            default:
                return main.trueColor2;
            case DamageType.Fire:
                return main.fireColor2;
        }
    }

    public static void ImpulseScreenShake(Vector3 force)
    {
        EnsureSingleton();
        main.impulse.GenerateImpulseWithVelocity(force);
    }

    public static void DamageScreenShake(Vector3 direction, bool isCrit, bool isBlock)
    {
        EnsureSingleton();
        float mag = main.hitImpulseMagnitude;
        if (isCrit)
        {
            mag = main.critImpulseMagnitude;
        }
        else if (isBlock)
        {
            mag = main.blockImpulseMagnitude;
        }
        ImpulseScreenShake(direction * mag);
    }

    public static void EnsureSingleton()
    {
        if (main == null)
        {
            FXController fx = GameObject.FindObjectOfType<FXController>();
            if (fx != null)
            {
                fx.Init();
            }
        }
    }

    public static GameObject CreateBleed(Vector3 position, Vector3 direction, bool isSlash, bool isCrit, FXMaterial hurtMaterial, AudioClip soundOverride)
    {
        EnsureSingleton();
        GameObject particlePrefab = main.fx_bleedSword;
        if (hurtMaterial == FXMaterial.Ice)
        {
            particlePrefab = (isSlash ? main.fx_iceSlash : main.fx_icePoint);
        }
        else
        {
            particlePrefab = (isSlash ? main.fx_bleedSword : main.fx_bleedPoint);
        }
        GameObject newFX = GameObject.Instantiate(particlePrefab);
        newFX.transform.position = position;
        newFX.transform.rotation = Quaternion.LookRotation(direction);
        if (soundOverride != null)
        {
            AudioSource source = newFX.GetComponentInChildren<AudioSource>();
            source.clip = soundOverride;
            source.Play();
        }
        else if (isCrit)
        {
            AudioClip clip = GetSwordCriticalSoundFromFXMaterial(hurtMaterial);
            AudioSource source = newFX.GetComponentInChildren<AudioSource>();

            source.clip = clip;
            source.Play();
        }
        Destroy(newFX, 10f);
        return newFX;
    }

    void ShowPlayerHealParticleInstance()
    {
        if (healParticle != null)
        {
            healParticle.Play();
            AudioSource audioSource = healParticle.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
        }
    }

    public static void ShowPlayerHealParticle()
    {
        main.ShowPlayerHealParticleInstance();
    }
}
