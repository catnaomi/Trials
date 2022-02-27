using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

public class AnimationFXHandler : MonoBehaviour
{
    public AnimationFXSounds animSounds;
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
    [Header("Events")]
    public UnityEvent OnDust;
    public UnityEvent OnDashDust;
    [Header("Anim Events")]
    public UnityEvent OnArrowDraw;
    public UnityEvent OnArrowNock;
    Actor actor;
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<Actor>();
    }

    #region Footsteps
    public void StepL(int heavy)
    {
        //AudioSource source = (heavy > 0) ? footSourceHeavy : footSourceLight;
        AudioSource source = footSourceLight;
        AudioClip clip = GetFootStepFromTerrain(actor.GetCurrentGroundPhysicsMaterial(), true);
        if (Time.time - stepLTime > footstepDelay)
        {
            Debug.Log(Time.time - stepLTime);
            source.PlayOneShot(clip);
            stepLTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
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
            Debug.Log(Time.time - stepRTime);
            source.PlayOneShot(clip);
            stepRTime = Time.time;
        }
        if (actor != null && actor.ShouldDustOnStep()) OnDust.Invoke();
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