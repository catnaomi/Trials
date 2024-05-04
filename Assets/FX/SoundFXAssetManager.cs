using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXAssetManager : MonoBehaviour
{
    public Dictionary<string, AudioClip> soundEffects;

    void Awake()
    {
        // TODO: organize sound assets so that where possible name and path are identical
        LoadSound("sword/swing/light",  "Effects/sword_swing1");
        LoadSound("sword/swing/medium", "Effects/sword_swing1");
        LoadSound("sword/swing/heavy", "Effects/sword_swing1");
        LoadSound("shield/bash",  "Effects/sound_temp_bash");
        LoadSound("shield/bash/hit", "Effects/sound_temp_bash_hit");
        LoadSound("metal/clash", "Effects/sound_temp_clash");
        LoadSound("bow/draw",  "Effects/bow-draw1");
        LoadSound("bow/fire", "Effects/bow-fire");
        LoadSound("bow/hit", "Effects/bow-hit1");
        LoadSound("parry/start", "Effects/sword-parry01");
        LoadSound("parry/success", "Effects/sword-parry02");
        LoadSound("sword/blood",  "Effects/sword-bleed1");
        LoadSound("sword/blood/crit",  "Effects/sword-bleed2");
        LoadSound("sword/metal",  "Effects/metal-hit2");
        LoadSound("sword/metal/crit",  "Effects/stone-break1");
        LoadSound("sword/wood",  "Effects/metal-hit1");
        LoadSound("sword/wood/crit",  "Effects/wood-break1");
        LoadSound("sword/bleed",  "Effects/sword-bleed1");
        LoadSound("dash", "Footsteps/dash1");
        LoadSound("tap", "Footsteps/tap1");
        LoadSound("thud", "Footsteps/thud1");
        LoadSound("slide", "Footsteps/dirt-slide");
        LoadSound("roll", "Footsteps/roll1");
        LoadSound("slide/continuous", "Effects/slide");
        LoadSound("splash/small", "Water/splash2");
        LoadSound("splash/big", "Water/splash1");
        LoadSound("slash/light", "Effects/sword_swing1");
        LoadSound("slash/heavy", "Effects/sound_temp_sword_swing_light");
        LoadSound("thrust/light", "Effects/sword_swing1");
        LoadSound("thrust/heavy", "Effects/sound_temp_sword_swing_light");
        LoadSound("bow/pull", "Effects/bow-draw1");
        LoadSound("bow/fire", "Effects/bow-fire");
        LoadSound("charge/start", "Effects/icecharge1");
        LoadSound("block/switch", "Effects/click1");
        LoadSound("step/default/left", "Footsteps/tile-stepL");
        LoadSound("step/default/right", "Footsteps/tile-stepR");
        LoadSound("step/stone/left", "Footsteps/stone-stepL");
        LoadSound("step/stone/right", "Footsteps/stone-stepR");
        LoadSound("step/grass/left", "Footsteps/grass-stepL");
        LoadSound("step/grass/right", "Footsteps/grass-stepR");
        LoadSound("step/dirt/left", "Footsteps/dirt-stepL");
        LoadSound("step/dirt/right", "Footsteps/dirt-stepR");
        LoadSound("step/tile/left", "Footsteps/tile-stepL");
        LoadSound("step/tile/right", "Footsteps/tile-stepR");
        LoadSound("step/water/left", "Footsteps/water-stepL");
        LoadSound("step/water/right", "Footsteps/water-stepR");
    }

    void LoadSound(string name, string path)
    {
        var audioClip = Resources.Load<AudioClip>(Path.Combine("Sounds", path));
        if (audioClip != null)
        {
            soundEffects.Add(name, audioClip);
        }
        else
        {
            Debug.LogError($"Couldn't load sound effect {path}!");
        }
    }
}
