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
    public float volumeSetting = 1f;
    public bool paused = false;
    public bool timeStopped
    {
        set
        {
            musicSource.pitch = value ? playbackSpeedDuringTimeStop : 1f;
        }
    }
    public float targetVolume
    {
        get
        {
            return paused ? (pauseVolumeMultiplier * volumeSetting) : volumeSetting;
        }
    }

    [Header("Constants")]
    public float pauseVolumeMultiplier;
    public float volumeSpeed;
    public float startVolume;
    public float playbackSpeedDuringTimeStop;
    
    Dictionary<string, AudioClip> tracks;
    AudioClip playing;

    void Awake()
    {
        instance = this;
        tracks = new Dictionary<string, AudioClip>();
    }

    void Update()
    {
        if (volume != targetVolume)
        {
            var volumeAdditionThisUpdate = volumeSpeed * Time.unscaledDeltaTime * (targetVolume > volume ? 1f : -1f);
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

        // Loop if track ends
        if (playing != null && !musicSource.isPlaying) {
            musicSource.PlayOneShot(playing);
        }
    }
    
    void Play(AudioClip track)
    {
        volume = startVolume;
        playing = track;
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
        playing = null;
        musicSource.Stop();
    }
}
