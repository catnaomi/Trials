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
    public float playbackSpeed
    {
        get
        {
            return musicSource.pitch;
        }
        set
        {
            musicSource.pitch = value;
        }
    }

    [Header("Script Managed Properties")]
    public float volumeSetting = 1f;
    public bool paused = false;
    public bool timeStopped = false;
    public float targetVolume
    {
        get
        {
            return paused ? (pauseVolumeMultiplier * volumeSetting) : volumeSetting;
        }
    }
    public float targetPlaybackSpeed
    {
        get
        {
            if (timeStopped && !paused)
            {
                return playbackSpeedDuringTimeStop;
            }
            return 1f;
        }
    }

    [Header("Constants")]
    public float pauseVolumeMultiplier;
    public float volumeSpeed;
    public float startVolume;
    public float playbackSpeedDuringTimeStop;
    public float playbackSpeedSpeed;
    
    Dictionary<string, AudioClip> tracks;
    AudioClip playing;

    void Awake()
    {
        instance = this;
        tracks = new Dictionary<string, AudioClip>();
    }

    float AnimateMusicProperty(float currentValue, float targetValue, float speed)
    {
        if (currentValue != targetValue)
        {
            var additionThisUpdate = speed * Time.unscaledDeltaTime * (targetValue > currentValue ? 1f : -1f);
            var difference = targetValue - currentValue;
            if (Math.Abs(additionThisUpdate) >= Math.Abs(difference))
            {
                currentValue = targetValue;
            }
            else
            {
                currentValue += additionThisUpdate;
            }
        }
        return currentValue;
    }

    void Update()
    {
        volume = AnimateMusicProperty(volume, targetVolume, volumeSpeed);
        playbackSpeed = AnimateMusicProperty(playbackSpeed, targetPlaybackSpeed, playbackSpeedSpeed);

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

    public static void Play(string trackName)
    {
        if (instance.tracks.TryGetValue(trackName, out AudioClip track))
        {
            instance.Play(track);
        }
        else
        {
            var loadRequest = Resources.LoadAsync<AudioClip>($"Music/{trackName}");
            loadRequest.completed += (_) =>
            {
                track = (AudioClip)loadRequest.asset;
                if (track != null)
                {
                    instance.tracks.Add(trackName, track);
                    instance.Play(track);
                }
                else
                {
                    Debug.LogError($"Couldn't find music track {trackName}");
                }
            };
        }
    }

    public static void Stop()
    {
        instance.playing = null;
        instance.musicSource.Stop();
    }
}
