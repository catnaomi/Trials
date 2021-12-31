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
    private static float fixedDeltaTime;
    private static float hitpauseLength;

    private static float slowMotionLength;
    private static float slowMotionAmount;

    public static FXController main;

    [Header("Damage Colors")]
    public Color trueColor;
    public Color trueColor2;
    public Color fireColor;
    public Color fireColor2;
    public enum FX {
        FX_Block,
        FX_Hit,
        FX_Stagger,
        FX_Sparks,
    }
    public static Dictionary<FX, GameObject> fxDictionary;
    public static Dictionary<string, AudioClip> clipDictionary;
    private void OnEnable()
    {
        fxDictionary = new Dictionary<FX, GameObject>()
        {
            { FX.FX_Block,  this.fx_block },
            { FX.FX_Hit, this.fx_hit },
            { FX.FX_Stagger, this.fx_stagger },
            { FX.FX_Sparks, this.fx_sparks }
        };

        clipDictionary = new Dictionary<string, AudioClip>()
        {
            { "sword_swing_light",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_light") },
            { "sword_swing_medium", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_medium") },
            { "sword_swing_heavy", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_heavy") },

            { "sword_hit_light",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_hit_light") },
            { "sword_hit_heavy", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_hit_heavy") },

            { "shield_bash",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bash") },
            { "shield_bash_hit", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bash_hit") },

            { "metal_clash", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_clash") },

            { "bow_draw",  Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bow_draw") },
            { "bow_fire", Resources.Load<AudioClip>("Sounds/Effects/sound_temp_bow_fire") },

            { "parry_start", Resources.Load<AudioClip>("Sounds/Effects/sword-parry01") },
            { "parry_success", Resources.Load<AudioClip>("Sounds/Effects/sword-parry02") },
        };

        fixedDeltaTime = Time.fixedDeltaTime;

        main = this;
    }

    public static void CreateFX(FX name, Vector3 position, Quaternion rotation, float duration)
    {
        CreateFX(name, position, rotation, duration, null);
    }

    public static void CreateFX(FX name, Vector3 position, Quaternion rotation, float duration, AudioClip audioClipOverwrite)
    {
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
    }

    public static GameObject CreateSwordSlash()
    {
        GameObject newFX = GameObject.Instantiate(main.fx_slash);
        return newFX;
    }

    public static void Hitpause(float duration)
    {
        hitpauseLength = duration;
        Debug.Log(string.Format("Attempting to hit pause for {0} second(s)", hitpauseLength));
        var coroutine = main.HitpauseCoroutine();
        main.StartCoroutine(coroutine);
    }

    IEnumerator HitpauseCoroutine()
    {
        yield return new WaitForEndOfFrame();
        HitpauseStart();
        Debug.Log(string.Format("Yielding for {0} second(s)", hitpauseLength));
        yield return new WaitForSecondsRealtime(hitpauseLength);
        Debug.Log("Ending hit pause Coroutine!");
        HitpauseEnd();
    }
    private static void HitpauseStart()
    {
        Time.timeScale = 0.0f;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    }

    private static void HitpauseEnd()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    }

    public static void SlowMo(float amount, float duration)
    {
        slowMotionLength = duration;
        slowMotionAmount = amount;
        Debug.Log(string.Format("Attempting to slow time by {1} for {0} second(s)", slowMotionLength, slowMotionAmount));
        var coroutine = main.SlowMoCoroutine();
        main.StartCoroutine(coroutine);
    }

    public static void CancelSlowMo()
    {
        Debug.Log("cancelling slow motion");
        main.StopCoroutine("SlowMoCoroutine");
        SlowMoEnd();
    }

    IEnumerator SlowMoCoroutine()
    {
        yield return new WaitForEndOfFrame();
        SlowMoStart();
        Debug.Log(string.Format("Yielding for {0} second(s)", slowMotionLength));
        yield return new WaitForSecondsRealtime(slowMotionLength);
        Debug.Log("Ending slowmo Coroutine!");
        SlowMoEnd();
    }
    private static void SlowMoStart()
    {
        Time.timeScale = slowMotionAmount;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    }

    private static void SlowMoEnd()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
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
}
