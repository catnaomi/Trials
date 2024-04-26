using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController instance;

    [Header("References")]
    public AudioSource musicSource;
    public float volume
    {
        get
        {
            return musicSource.volume;
        }
        set
        {
            musicSource.volume = value;
        }
    }

    [Header("Script Managed Properties")]
    public float volumeSetting;
    public bool paused;
    public float targetVolume
    {
        get
        {
            return paused ? (pauseVolumeMultiplier * volumeSetting) : volumeSetting;
        }
    }

    [Header("Constants")]
    public float pauseVolumeMultiplier;
    public float volumeVelocity;
    public float startVolume;
    
    Dictionary<string, AudioClip> tracks;

    void Awake()
    {
        instance = this;
        tracks = new Dictionary<string, AudioClip>();
    }

    void Update()
    {
        if (volume != targetVolume)
        {
            var volumeAdditionThisUpdate = volumeVelocity * Time.unscaledDeltaTime * (targetVolume > volume ? 1f : -1f);
            var difference = targetVolume - volume;
            if (Math.Abs(volumeAdditionThisUpdate) >= Math.Abs(difference))
            {
                volume = targetVolume;
            }
            else
            {
                volume += volumeAdditionThisUpdate;
            }
        }
    }
    
    void Play(AudioClip track)
    {
        volume = startVolume;
        musicSource.PlayOneShot(track);
    }

    public void Play(string trackName)
    {
        if (tracks.TryGetValue(trackName, out AudioClip track))
        {
            Play(track);
        }
        else
        {
            var loadRequest = Resources.LoadAsync<AudioClip>($"Music/{trackName}");
            loadRequest.completed += (_) =>
            {
                track = (AudioClip)loadRequest.asset;
                if (track != null)
                {
                    tracks.Add(trackName, track);
                    Play(track);
                }
                else
                {
                    Debug.LogError($"Couldn't find music track {trackName}");
                }
            };
        }
    }

    public void Stop()
    {
        musicSource.Stop();
    }
}
