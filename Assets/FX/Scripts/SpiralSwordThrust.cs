using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpiralSwordThrust : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;
    public Transform pseudoParent;
    public ParticleSystem[] bloodParticles = new ParticleSystem[3];

    int currentIndex = 0;
    public float bloodFadeTime = 0.5f;
    public float bloodFadeDelay = 0.5f;
    bool thrusting;
    bool nextIsCrit;
    Vector3 contactPoint;

    [Header("Other FX")]
    public CinemachineImpulseSource impulse;
    [Range(0f, 1f)]
    public float hitVolume = 0.5f;
    [Range(0f, 1f)]
    public float critVolume = 1f;
    public float impulseMag = 0.1f;
    public float impulseCritMag = 0.2f;
    public float impulseBlockMult = 0.5f;
    [Space(10)]
    public ParticleSystem spiral;
    public Transform spiralParent;
    public float rotateSpeed = 360f;
    public UnityEvent BeginSpiral;
    public UnityEvent EndSpiral;
    public UnityEvent OnBleed;
    public float spiralFadeDelay = 0.5f;
    public float spiralFadeTime = 0.5f;
    
    void Start()
    {
        InitSpiral();
    }

    void Update()
    {
        if (thrusting)
        {
            SetSpiralPosition();
            spiral.transform.localRotation = Quaternion.identity;
            if (!spiral.isEmitting)
            {
                spiral.Clear();
                spiral.Play();
            }
        }
    }

    public void BeginThrust()
    {
        if (bottomPoint == null || topPoint == null)
        {
            Debug.Log("one or more of thrust reference points missing!");
            return;
        }
        thrusting = true;
    }

    public void EndThrust()
    {
        thrusting = false;
    }

    public void SetTopPoint(Transform t)
    {
        topPoint = t;
    }

    public void SetBottomPoint(Transform t)
    {
        bottomPoint = t;
    }

    public void Bleed()
    {
        bool isCrit = IsNextCrit();
        bloodParticles[currentIndex].transform.position = contactPoint;
        bloodParticles[currentIndex].transform.rotation = Quaternion.LookRotation(pseudoParent.transform.forward);
        bloodParticles[currentIndex].gameObject.SetActive(true);
        bloodParticles[currentIndex].Play();
        if (isCrit) Debug.Log("Crit!!!");
        AudioClip clip = (isCrit) ? FXController.GetSwordCriticalSoundFromFXMaterial(FXController.FXMaterial.Blood) : FXController.GetSwordHitSoundFromFXMaterial(FXController.FXMaterial.Blood);
        float volume = (isCrit) ? critVolume : hitVolume;
        this.GetComponent<AudioSource>().Stop();
        this.GetComponent<AudioSource>().PlayOneShot(clip, volume);
        float force = (isCrit) ? impulseCritMag : impulseMag;
        Shake(force);
        OnBleed.Invoke();
    }

    public void Block(Vector3 point)
    {
        bool isCrit = IsNextCrit();
        AudioClip clip = (isCrit) ? FXController.GetSwordCriticalSoundFromFXMaterial(FXController.FXMaterial.Metal) : FXController.GetSwordHitSoundFromFXMaterial(FXController.FXMaterial.Metal);
        FXController.CreateFX(FXController.FX.FX_Sparks, point, Quaternion.identity, 1f, clip);

        float force = (isCrit) ? impulseCritMag : impulseMag;
        Shake(force * impulseBlockMult);
    }

    public void Shake(float force)
    {
        impulse.GenerateImpulseWithForce(force);
    }


    public void StopBleeding()
    {
    }

    public void SetContactPoint(Vector3 position)
    {
        contactPoint = position;
    }

    public void SetNextCrit(bool crit)
    {
        nextIsCrit = crit;
    }

    public bool IsNextCrit()
    {
        bool crit = nextIsCrit;
        nextIsCrit = false;
        return crit;
    }

    public void InitSpiral()
    {
        if (spiral != null)
        {
            spiral.gameObject.SetActive(true);
            spiral.GetComponent<ParticleSystem>().Stop();
            spiral.GetComponent<ParticleSystem>().Clear();
            SetSpiralPosition();
        }

    }

    public void SetSpiralPosition()
    {
        if (topPoint == null) return;
        spiralParent.position = topPoint.position;
        spiralParent.rotation = Quaternion.LookRotation(pseudoParent.transform.forward);
    }
} 
