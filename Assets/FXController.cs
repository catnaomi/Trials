using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    public static FXController instance;

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
        Water,
    }
    
    public Dictionary<FX, GameObject> fxDictionary;
    public Dictionary<string, AudioClip> clipDictionary;

    void Awake()
    {
        instance = this;

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
    }

    public GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration)
    {
        return CreateFX(name, position, rotation, duration, null);
    }

    public GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration, AudioClip audioClipOverwrite)
    {
        GameObject newFX = GameObject.Instantiate(fxDictionary[name], position, rotation);

        if (audioClipOverwrite != null)
        {
            AudioSource audioSource = newFX.GetComponentInChildren<AudioSource>();
            audioSource.clip = audioClipOverwrite;
            audioSource.Play();
        }

        GameObject.Destroy(newFX, duration);
        return newFX;
    }

    public GameObject CreateSwordSlash()
    {
        GameObject newFX = GameObject.Instantiate(fx_slash);
        return newFX;
    }

    public GameObject CreateSwordThrust()
    {
        GameObject newFX = GameObject.Instantiate(fx_thrust);
        return newFX;
    }

    public GameObject CreateDizzy()
    {
        GameObject newFX = GameObject.Instantiate(fx_dizzy);
        return newFX;
    }

    public GameObject CreateBladeWarning()
    {
        GameObject newFX = GameObject.Instantiate(fx_warn);
        return newFX;
    }

    public GameObject CreateSpiral()
    {
        GameObject newFX = GameObject.Instantiate(fx_spiral);
        return newFX;
    }
    public GameObject CreateGunTrail(Vector3 start, Vector3 end, Vector3 direction, float duration, AudioClip soundOverride)
    {
        GameObject newFX = GameObject.Instantiate(fx_gunTrail);
        newFX.transform.position = start;
        newFX.transform.rotation = Quaternion.LookRotation(direction);

        LineRenderer line = newFX.GetComponentInChildren<LineRenderer>();
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        GameObject.Destroy(newFX, duration);

        return newFX;
    }

    public void CreateSpark(Vector3 position, Vector3 direction, AudioClip soundOverride)
    {
        CreateFX(FXController.FX.FX_Sparks, position, Quaternion.LookRotation(-direction), 1f, soundOverride);
    }

    public void CreateCross(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(fx_cross, position, direction);
    }

    public void CreateCircle(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(fx_circle, position, direction);
    }

    public void CreateParrySuccess(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(fx_parry_success, position, direction);
    }

    public GameObject CreateMiragiaParticleSingle(Vector3 position)
    {
        GameObject newFX = GameObject.Instantiate(fx_miragia);
        newFX.transform.position = position;

        return newFX;
    }

    public GameObject CreateMiragiaParticleSingleSound(Vector3 position)
    {
        GameObject newFX = GameObject.Instantiate(fx_miragia_sound);
        newFX.transform.position = position;

        return newFX;
    }

    static void CreateFromPrefab(GameObject prefab, Vector3 position, Vector3 direction)
    {
        GameObject newFX = GameObject.Instantiate(prefab);
        newFX.transform.position = position;
        newFX.transform.rotation = Quaternion.LookRotation(direction);
    }

    public AudioClip GetSwordHitSoundFromFXMaterial(FXMaterial material)
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

    public AudioClip GetSwordCriticalSoundFromFXMaterial(FXMaterial material)
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

    public Color GetColorForDamageType(DamageType type)
    {
        switch (type)
        {
            default:
                return trueColor;
            case DamageType.Fire:
                return fireColor;
        }
    }

    public Color GetSecondColorForDamageType(DamageType type)
    {
        switch (type)
        {
            default:
                return trueColor2;
            case DamageType.Fire:
                return fireColor2;
        }
    }

    public void ImpulseScreenShake(Vector3 force)
    {
        impulse.GenerateImpulseWithVelocity(force);
    }

    public void DamageScreenShake(Vector3 direction, bool isCrit, bool isBlock)
    {
        float mag = hitImpulseMagnitude;
        if (isCrit)
        {
            mag = critImpulseMagnitude;
        }
        else if (isBlock)
        {
            mag = blockImpulseMagnitude;
        }
        ImpulseScreenShake(direction * mag);
    }

    public GameObject CreateBleed(Vector3 position, Vector3 direction, bool isSlash, bool isCrit, FXMaterial hurtMaterial, AudioClip soundOverride)
    {
        GameObject particlePrefab;
        if (hurtMaterial == FXMaterial.Ice)
        {
            particlePrefab = isSlash ? fx_iceSlash : fx_icePoint;
        }
        else
        {
            particlePrefab = isSlash ? fx_bleedSword : fx_bleedPoint;
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

    public void ShowPlayerHealParticle()
    {
        ShowPlayerHealParticleInstance();
    }
}
