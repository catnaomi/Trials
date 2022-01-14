using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

public class AnimationFXHandler : MonoBehaviour
{
    [Header("Footsteps")]
    public AudioSource footSourceLight;
    public AudioSource footSourceHeavy;
    public AudioClip default_stepL;
    public AudioClip default_stepR;
    public AudioClip default_thud;
    public AudioClip dash;
    public AudioClip default_slide;
    public AudioClip default_roll;
    public AudioClip tap;
    [Space(10)]
    public Transform footL;
    public Transform footR;
    [Header("Swim")]
    public AudioSource waterSource;
    public AudioClip swim;
    public AudioClip splashSmall;
    public AudioClip splashBig;
    [Header("Combat")]
    public AudioSource combatWhiffSource;
    public AudioSource combatHitSource;
    public AudioClip slashLight;
    public AudioClip thrustLight;
    public AudioClip slashHeavy;
    public AudioClip thrustHeavy;
    [Space(10)]
    public AudioClip bowPull;
    public AudioClip bowFire;
    [Header("Events")]
    public UnityEvent OnDust;
    public UnityEvent OnDashDust;
    [Header("Anim Events")]
    public UnityEvent OnArrowDraw;
    public UnityEvent OnArrowNock;
    // Start is called before the first frame update
    void Start()
    {
    }

    #region Footsteps
    public void StepL(int heavy)
    {
        AudioSource source = (heavy > 0) ? footSourceHeavy : footSourceLight;
        if (!footSourceHeavy.isPlaying && !footSourceLight.isPlaying) source.PlayOneShot(default_stepL);
        if (heavy > 0) OnDust.Invoke();
    }

    public void StepL()
    {
        StepL(0);
    }
    public void StepR(int heavy)
    {
        AudioSource source = (heavy > 0) ? footSourceHeavy : footSourceLight;
        if (!footSourceHeavy.isPlaying && !footSourceLight.isPlaying) source.PlayOneShot(default_stepR);
        if (heavy > 0) OnDust.Invoke();
    }

    public void StepR()
    {
        StepR(0);
    }

    public void Tap()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(tap);
    }

    public void Thud()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(default_thud);
    }

    public void Dash()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(dash);
        OnDashDust.Invoke();
    }

    public void Slide()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(default_slide);
        OnDashDust.Invoke();
    }

    public void Roll()
    {
        footSourceHeavy.Stop();
        footSourceHeavy.PlayOneShot(default_roll);
        
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
        waterSource.PlayOneShot(swim);
    }

    public void SplashBig()
    {
        waterSource.Stop();
        waterSource.PlayOneShot(splashBig);
    }

    public void SplashSmall()
    {
        waterSource.Stop();
        waterSource.PlayOneShot(splashSmall);
    }
    #endregion

    #region Combat

    public void SlashLight()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(slashLight);
    }

    public void SlashHeavy()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(slashHeavy);
    }

    public void ThrustLight()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(thrustLight);
    }

    public void ThrustHeavy()
    {
        combatWhiffSource.Stop();
        combatWhiffSource.PlayOneShot(thrustHeavy);
    }

    public void ArrowDraw()
    {
        OnArrowDraw.Invoke();
    }

    public void ArrowNock()
    {
        combatWhiffSource.PlayOneShot(bowPull);
        OnArrowNock.Invoke();
    }

    public void ArrowFire()
    {
        combatWhiffSource.Stop();
        combatHitSource.PlayOneShot(bowFire);
    }
    #endregion

#if (UNITY_EDITOR)
    public void PopulateWithDefaults()
    {
        Debug.Log("Populating AnimationSoundHandler on " + this);

        // footsteps
        default_stepL = Resources.Load<AudioClip>("Sounds/Footsteps/tile-stepL");
        default_stepR = Resources.Load<AudioClip>("Sounds/Footsteps/tile-stepR");
        default_thud = Resources.Load<AudioClip>("Sounds/Footsteps/thud1");
        dash = Resources.Load<AudioClip>("Sounds/Footsteps/dash1");
        default_slide = Resources.Load<AudioClip>("Sounds/Footsteps/dirt-slide");
        default_roll = Resources.Load<AudioClip>("Sounds/Footsteps/roll1");
        tap = Resources.Load<AudioClip>("Sounds/Footsteps/tap1");
        // swim
        swim = Resources.Load<AudioClip>("Sounds/Water/swim1");
        splashBig = Resources.Load<AudioClip>("Sounds/Water/splash1");
        splashSmall = Resources.Load<AudioClip>("Sounds/Water/splash2");
        // combat
        slashLight = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_light");
        slashHeavy = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_heavy");
        thrustLight = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_light");
        thrustHeavy = Resources.Load<AudioClip>("Sounds/Effects/sound_temp_sword_swing_heavy");

        bowPull = Resources.Load<AudioClip>("Sounds/Effects/bow-draw1");
        bowFire = Resources.Load<AudioClip>("Sounds/Effects/bow-fire");
    }
#endif
}