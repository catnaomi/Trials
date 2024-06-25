using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundFXAssetManager : MonoBehaviour
{
    public static SoundFXAssetManager instance;

    public Dictionary<string, AudioClip[]> soundEffects = new Dictionary<string, AudioClip[]>();
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        LoadSoundAssets();
        
    }

    public void LoadSoundAssets()
    {
        soundEffects.Clear();
        LoadSound(null, "Player/FallDamage");
        LoadSound(null, "Player/Dash");
        LoadSound(null, "Player/Tap");
        LoadSound(null, "Player/Thud");
        LoadSound(null, "Player/Roll");
        LoadSound(null, "Player/Block/Switch");
        LoadSound(null, "Player/Slide/Slide");
        LoadSound(null, "Player/Slide/Continuous");

        LoadSound("Sword/Swing/Light", "Slash/Light", "Thrust/Light", "Slash/Heavy", "Thrust/Heavy");
        //LoadSound("Sword/Swing/Heavy", "Slash/Heavy", "Thrust/Heavy");
        LoadSound(null, "Sword/Blood/NoCritical");
        LoadSound(null, "Sword/Metal/NoCritical");
        LoadSound(null, "Sword/Wood/NoCritical");
        LoadSound(null, "Sword/Stone/NoCritical");
        LoadSound(null, "Sword/Ice/NoCritical");
        LoadSound(null, "Sword/Blood/Critical");
        LoadSound(null, "Sword/Metal/Critical");
        LoadSound(null, "Sword/Wood/Critical");
        LoadSound(null, "Sword/Stone/Critical", "Parry/Success");
        LoadSound(null, "Sword/Ice/Critical");
        LoadSound(null, "Sword/Blood/Weak");
        LoadSound(null, "Sword/Metal/Weak");
        LoadSound(null, "Sword/Wood/Weak");
        LoadSound(null, "Sword/Stone/Weak");
        LoadSound(null, "Sword/Ice/Weak");


        LoadSound(null, "Bow/Fire", "Gun/Fire");
        LoadSound(null, "Bow/Hit");
        LoadSound(null, "Bow/Draw", "Gun/Reload");

        LoadSound(null, "Enemy/IceGiant/Charge");

        LoadSound(null, "Swim/Swim");
        LoadSound(null, "Swim/Splash/Small");
        LoadSound(null, "Swim/Splash/Big");
        LoadSound(null, "Swim/Splash/Bigger");

        LoadSound(null, "Step/Default/Left", "Step/Metal/Left", "Step/Ice/Left", "Step/Wood/Left");
        LoadSound(null, "Step/Default/Right", "Step/Metal/Right", "Step/Ice/Right", "Step/Wood/Right");
        LoadSound(null, "Step/Stone/Left");
        LoadSound(null, "Step/Stone/Right");
        LoadSound(null, "Step/Grass/Left");
        LoadSound(null, "Step/Grass/Right");
        LoadSound(null, "Step/Dirt/Left");
        LoadSound(null, "Step/Dirt/Right");
        LoadSound(null, "Step/Water/Left");
        LoadSound(null, "Step/Water/Right");
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
            audioClip = AudioClip.Create("Empty Clip", 1, 2, 48000, false);
        }

        foreach (var name in names)
        {
            soundEffects.Add(name, new AudioClip[] { audioClip });
        }
    }

    void LoadSoundVariants(string path, int variationCount, params string[] names)
    {
        if (variationCount <= 0)
        {
            LoadSound(path, names);
            return;
        }

        if (path == null)
        {
            path = names[0];
        }

        AudioClip[] clips = new AudioClip[variationCount];

        for (int i = 0; i < variationCount; i++)
        {
            var audioClip = Resources.Load<AudioClip>(Path.Combine("Sounds", path+"_"+(i+1)));
            if (audioClip == null)
            {
                Debug.LogError($"Couldn't load sound effect {path}!");
                audioClip = AudioClip.Create("Empty Clip", 1, 2, 48000, false);
            }
            clips[i] = audioClip;
        }

        List<string> totalNames = new List<string>();
        totalNames.AddRange(names);

        foreach (string name in names)
        {
            soundEffects.Add(name, clips);
            for (int i = 0; i < variationCount; i++)
            {
                soundEffects.Add(name + "_" + (i + 1), new AudioClip[] { clips[i] });
            }
        }
        
    }

    public static AudioClip GetSound(string name)
    {
        if (instance.soundEffects.ContainsKey(name) == false)
        {
            Debug.LogError($"Couldn't find sound effect {name}!");
            return null;
        }
        if (instance.soundEffects.Count == 0)
        {
            return instance.soundEffects[name][0];
        }
        else
        {
            return instance.soundEffects[name][UnityEngine.Random.Range(0, instance.soundEffects[name].Length)];
        }
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


    public static void PlaySound(AudioSource source, string name)
    {
        source.PlayOneShot(GetSound(name));
    }

    public static void PlaySound(AudioSource source, params string[] nameParts)
    {
        source.PlayOneShot(GetSound(nameParts));
    }

    public static void PlaySound(AudioSource source, float volume, string name)
    {
        source.PlayOneShot(GetSound(name), volume);
    }

    public static void PlaySound(AudioSource source, float volume, params string[] nameParts)
    {
        source.PlayOneShot(GetSound(nameParts), volume);
    }
}
