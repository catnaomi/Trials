using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundFXAssetManager : MonoBehaviour
{
    public static SoundFXAssetManager instance;

    public Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (instance != null)
        {
            throw new Exception("Duplicated SoundFXAssetManager");
        }
        instance = this;
        DontDestroyOnLoad(this);
        
        // TODO: organize sound assets so that where possible name and path are identical
        // TODO: organize the actual loads
        LoadSound("Footsteps/dash1", "Dash");
        LoadSound("Footsteps/tap1", "Tap");
        LoadSound("Footsteps/thud1", "Thud");
        LoadSound("Footsteps/dirt-slide", "Slide");
        LoadSound("Footsteps/roll1", "Roll");
        LoadSound("Effects/sound_temp_bash", "Shield/Bash");
        LoadSound("Effects/sound_temp_clash", "Metal/Clash");
        LoadSound("Effects/bow-fire", "Bow/Fire", "Gun/Fire");
        LoadSound("Effects/bow-hit1", "Bow/Hit");
        LoadSound("Effects/bow-draw1", "Bow/Draw", "Bow/Pull", "Gun/Reload"); // TODO: de-alias draw/pull
        LoadSound("Effects/sword-parry01", "Parry/Start");
        LoadSound("Effects/sword-parry02", "Parry/Success");
        LoadSound("Effects/slide", "Slide/Continuous");
        LoadSound("Water/splash2", "Splash/Small");
        LoadSound("Water/splash1", "Splash/Big");
        LoadSound("Effects/sword_swing1", "Slash/Light");
        LoadSound("Effects/sword_swing1", "Thrust/Light");
        LoadSound("Effects/sound_temp_sword_swing_light", "Slash/Heavy");
        LoadSound("Effects/sound_temp_sword_swing_light", "Thrust/Heavy");
        LoadSound("Effects/icecharge1", "Charge/Start");
        LoadSound("Effects/click1", "Block/Switch");
        LoadSound("Effects/sound_temp_bash_hit", "Shield/Bash/Hit");

        LoadSound("Effects/sword-bleed1", "Sword/Blood/NoCritical");
        LoadSound("Effects/metal-hit1", "Sword/Metal/NoCritical");
        LoadSound("Effects/wood-cut1", "Sword/Wood/NoCritical");
        LoadSound("Effects/stone-hit1", "Sword/Stone/NoCritical", "Sword/Ice/NoCritical");

        LoadSound("Effects/sword-bleed2", "Sword/Blood/Critical");
        LoadSound("Effects/metal-hit2", "Sword/Metal/Critical");
        LoadSound("Effects/wood-break1", "Sword/Wood/Critical");
        LoadSound("Effects/stone-break1", "Sword/Stone/Critical", "Sword/Ice/Critical");
        
        LoadSound("Effects/sword_swing1", "Sword/Swing/Light", "Sword/Swing/Medium", "Sword/Swing/Heavy");
        LoadSound("Footsteps/tile-stepL", "Step/Default/Left", "Step/Metal/Left", "Step/Ice/Left");
        LoadSound("Footsteps/tile-stepR", "Step/Default/Right", "Step/Metal/Right", "Step/Ice/Right");
        LoadSound("Footsteps/stone-stepL", "Step/Stone/Left");
        LoadSound("Footsteps/stone-stepR", "Step/Stone/Right");
        LoadSound("Footsteps/grass-stepL", "Step/Grass/Left");
        LoadSound("Footsteps/grass-stepR", "Step/Grass/Right");
        LoadSound("Footsteps/dirt-stepL", "Step/Dirt/Left");
        LoadSound("Footsteps/dirt-stepR", "Step/Dirt/Right");
        LoadSound("Footsteps/tile-stepL", "Step/Tile/Left");
        LoadSound("Footsteps/tile-stepR", "Step/Tile/Right");
        LoadSound("Footsteps/water-stepL", "Step/Water/Left");
        LoadSound("Footsteps/water-stepR", "Step/Water/Right");
    }

    void LoadSound(string path, params string[] names)
    {
        if (path == null)
        {
            path = names[0];
        }

        var audioClip = Resources.Load<AudioClip>(Path.Combine("Sounds", path));
        if (audioClip == null)
        {
            Debug.LogError($"Couldn't load sound effect {path}!");
            audioClip = AudioClip.Create("Empty Clip", 0, 2, 0, false);
        }

        foreach (var name in names)
        {
            soundEffects.Add(name, audioClip);
        }
    }

    public static AudioClip GetSound(string name)
    {
        return instance.soundEffects[name];
    }

    public static AudioClip GetSound(params string[] nameSections)
    {
        var fullName = nameSections[0];
        foreach (var nameSection in new ArraySegment<string>(nameSections, 1, nameSections.Length - 1))
        {
            fullName += "/";
            fullName += nameSection;
        }
        return GetSound(fullName);
    }
}
