using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationFXSounds", menuName = "ScriptableObjects/FX/Create AnimationFXSounds", order = 1), SerializeField]
public class AnimationFXSounds : ScriptableObject
{
    [Header("Movement")]
    public AudioClip default_stepL;
    public AudioClip default_stepR;
    public AudioClip default_thud;
    public AudioClip dash;
    public AudioClip default_slide;
    public AudioClip default_roll;
    public AudioClip tap;
    [Header("Swim")]
    public AudioClip swim;
    public AudioClip splashSmall;
    public AudioClip splashBig;
    [Header("Combat")]
    public AudioClip slashLight;
    public AudioClip thrustLight;
    public AudioClip slashHeavy;
    public AudioClip thrustHeavy;
    [Space(10)]
    public AudioClip bowPull;
    public AudioClip bowFire;
    [Header("Per Material")]
    public AudioClip metal_stepL;
    public AudioClip metal_stepR;
    [Space(5)]
    public AudioClip stone_stepL;
    public AudioClip stone_stepR;
    [Space(5)]
    public AudioClip grass_stepL;
    public AudioClip grass_stepR;
    [Space(5)]
    public AudioClip dirt_stepL;
    public AudioClip dirt_stepR;
    [Space(5)]
    public AudioClip tile_stepL;
    public AudioClip tile_stepR;
    [Space(5)]
    public AudioClip ice_stepL;
    public AudioClip ice_stepR;
    [Space(5)]
    public AudioClip water_stepL;
    public AudioClip water_stepR;

#if (UNITY_EDITOR)
    public void PopulateWithDefaults()
    {
        Debug.Log("Populating AnimationFXSounds on " + this);

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