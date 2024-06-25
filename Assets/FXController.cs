using Cinemachine;
using System;
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

    public struct SlashAndThrustPrefabs
    {
        public GameObject slashPrefab;
        public GameObject thrustPrefab;
    }

    public Dictionary<FXMaterial, SlashAndThrustPrefabs> hitFXPrefabFromMaterial = new Dictionary<FXMaterial, SlashAndThrustPrefabs>();

    // Unity cannot serialize dictionaries so we manually populate them
    [Header("Damage Colors")]
    public Color trueDamageColor;
    public Color fireDamageColor;
    Dictionary<DamageType, Color> colorFromDamageType = new Dictionary<DamageType, Color>();

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

    // Unity cannot serialize dictionaries so we manually populate them
    [Header("Sound Constants")]
    public float criticalHitVolume;
    public float noCriticalHitVolume;

    ParticleSystem healParticle;

    public enum FX
    {
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

    Dictionary<FX, GameObject> fxObjects;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        fxObjects = new Dictionary<FX, GameObject>()
        {
            { FX.FX_Block,  fx_block },
            { FX.FX_Hit, fx_hit },
            { FX.FX_Stagger, fx_stagger },
            { FX.FX_Sparks, fx_sparks },
            { FX.FX_BleedSword, fx_bleedSword },
            { FX.FX_BleedPoint, fx_bleedPoint },
        };

        // Populate dictionaries filled by unity inspector values
        colorFromDamageType.Add(DamageType.TrueDamage, trueDamageColor);
        colorFromDamageType.Add(DamageType.Fire, fireDamageColor);

        SlashAndThrustPrefabs bloodHitFXs;
        bloodHitFXs.slashPrefab = fx_bleedSword;
        bloodHitFXs.thrustPrefab = fx_bleedPoint;
        hitFXPrefabFromMaterial.Add(FXMaterial.Blood, bloodHitFXs);
        SlashAndThrustPrefabs iceHitFXs;
        iceHitFXs.slashPrefab = fx_iceSlash;
        iceHitFXs.thrustPrefab = fx_icePoint;
        hitFXPrefabFromMaterial.Add(FXMaterial.Ice, iceHitFXs);

        if (PlayerActor.player != null)
        {
            healParticle = PlayerActor.player.transform.Find("_healparticle").GetComponent<ParticleSystem>();
        }
    }

    public static GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration)
    {
        return CreateFX(name, position, rotation, duration, null);
    }

    public static GameObject CreateFX(FX name, Vector3 position, Quaternion rotation, float duration, AudioClip audioClipOverwrite)
    {
        GameObject newFX = GameObject.Instantiate(instance.fxObjects[name], position, rotation);

        if (audioClipOverwrite != null)
        {
            AudioSource audioSource = newFX.GetComponentInChildren<AudioSource>();
            audioSource.PlayOneShot(audioClipOverwrite);
        }

        GameObject.Destroy(newFX, duration);
        return newFX;
    }

    public static GameObject CreateSwordSlash()
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_slash);
        return newFX;
    }

    public static GameObject CreateSwordThrust()
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_thrust);
        return newFX;
    }

    public static GameObject CreateDizzy()
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_dizzy);
        return newFX;
    }

    public static GameObject CreateBladeWarning()
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_warn);
        return newFX;
    }

    public static GameObject CreateSpiral()
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_spiral);
        return newFX;
    }
    public static GameObject CreateGunTrail(Vector3 start, Vector3 end, Vector3 direction, float duration, AudioClip soundOverride)
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_gunTrail);
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
        CreateFX(FX.FX_Sparks, position, Quaternion.LookRotation(-direction), 1f, soundOverride);
    }

    public static void CreateCross(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(instance.fx_cross, position, direction);
    }

    public static void CreateCircle(Vector3 position, Vector3 direction)
    {
        CreateFromPrefab(instance.fx_circle, position, direction);
    }

    public static void CreateParrySuccess(Vector3 position, Vector3 direction, AudioSource parrySoundSource)
    {
        CreateFromPrefab(instance.fx_parry_success, position, direction);
        SoundFXAssetManager.PlaySound(parrySoundSource, "Parry/Success");
    }

    public static GameObject CreateMiragiaParticleSingle(Vector3 position)
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_miragia);
        newFX.transform.position = position;

        return newFX;
    }

    public static GameObject CreateMiragiaParticleSingleSound(Vector3 position)
    {
        GameObject newFX = GameObject.Instantiate(instance.fx_miragia_sound);
        newFX.transform.position = position;

        return newFX;
    }

    static void CreateFromPrefab(GameObject prefab, Vector3 position, Vector3 direction)
    {
        GameObject newFX = GameObject.Instantiate(prefab);
        newFX.transform.position = position;
        newFX.transform.rotation = Quaternion.LookRotation(direction);
    }

    public static void PlaySwordHitSound(AudioSource source, FXMaterial material, bool isCritical)
    {
        var volume = isCritical ? instance.criticalHitVolume : instance.noCriticalHitVolume;
        SoundFXAssetManager.PlaySound(source, volume, "Sword", material.ToString(), isCritical ? "Critical" : "NoCritical");
    }

    public static Color GetColorFromDamageType(DamageType damageType)
    {
        return instance.colorFromDamageType[damageType];
    }

    public static void ImpulseScreenShake(Vector3 force)
    {
        instance.impulse.GenerateImpulseWithVelocity(force);
    }

    public static void DamageScreenShake(Vector3 direction, bool isCrit, bool isBlock)
    {
        float mag = instance.hitImpulseMagnitude;
        if (isCrit)
        {
            mag = instance.critImpulseMagnitude;
        }
        else if (isBlock)
        {
            mag = instance.blockImpulseMagnitude;
        }
        ImpulseScreenShake(direction * mag);
    }

    public static GameObject CreateBleed(Vector3 position, Vector3 direction, bool isSlash, bool didTink, bool isCritical, FXMaterial hurtMaterial)
    {
        if (!instance.hitFXPrefabFromMaterial.TryGetValue(hurtMaterial, out var particlePrefabs))
        {
            particlePrefabs = instance.hitFXPrefabFromMaterial[FXMaterial.Blood];
        }
        var particlePrefab = isSlash ? particlePrefabs.slashPrefab : particlePrefabs.thrustPrefab;
        GameObject newFX = GameObject.Instantiate(particlePrefab);
        newFX.transform.position = position;
        newFX.transform.rotation = Quaternion.LookRotation(direction);
        AudioSource source = newFX.GetComponentInChildren<AudioSource>();
        PlaySwordHitSound(source, hurtMaterial, isCritical);

        Destroy(newFX, 10f);
        return newFX;
    }
    
    public static GameObject CreateBlock(Vector3 position, Quaternion rotation, float duration, bool isTypedBlock)
    {
        var newFX = CreateFX(FX.FX_Sparks, position, rotation, duration, null);
        AudioSource audioSource = newFX.GetComponentInChildren<AudioSource>();
        PlaySwordHitSound(audioSource, FXMaterial.Metal, isTypedBlock);
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
