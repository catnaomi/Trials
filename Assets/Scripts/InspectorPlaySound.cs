using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InspectorPlaySound : MonoBehaviour
{
    public string alias;

    AudioSource src;

    void Start()
    {
        src = this.GetComponent<AudioSource>();
    }
    public void Play()
    {
        SoundFXAssetManager.PlaySound(src, alias);
    }
}
