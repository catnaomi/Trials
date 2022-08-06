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
    float TRAIL_FPS = 60f;
    //float lineTimer = 0f;
    public float bloodFadeTime = 0.5f;
    public float bloodFadeDelay = 0.5f;
    float bloodTimer;
    bool thrusting;
    bool bleeding;
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
    float maxSpiralSpeed;
    float spiralFadeClock;
    bool eraseSpiral;
    // Start is called before the first frame update
    void Start()
    {
        InitSpiral();
    }

    // Update is called once per frame
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
            spiralFadeClock = spiralFadeTime + spiralFadeDelay;
        }
        else
        {

            //SetSpiralPosition();
            if (spiral.isEmitting)
            {
                spiralFadeClock -= Time.deltaTime;
                if (spiralFadeClock <= 0)
                {
                    //spiral.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                
                //spiral.Pause();
            }
            /*
            if (spiral.line.positionCount > 0)
            {
                spiralFadeClock -= Time.deltaTime;
                if (spiralFadeClock <= spiralFadeTime)
                {
                    int count = Mathf.FloorToInt(maxPositionCount * (spiralFadeClock/spiralFadeTime));
                    if (count < 0) count = 0;
                    spiral.line.positionCount = count;
                }
                //spiral.line.positionCount--;
                /*Vector3[] posArray = new Vector3[spiral.line.positionCount];
                spiral.line.GetPositions(posArray);
                posArray = posArray.Skip(1).ToArray();
                spiral.line.SetPositions(posArray);
            }
            if (!spiral.updateLine)
            {
                //spiral.transform.localRotation *= Quaternion.Euler(0f, 0f,  rotateSpeed * Time.deltaTime);
            }
            */
            
        }
        for (int j = 0; j < bloodParticles.Length; j++)
        {
            if (bloodParticles[j].particleCount <= 0)
            {
                //bloodParticles[j].Stop();
                //bloodParticles[j].gameObject.SetActive(false);
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
        bloodTimer = bloodFadeDelay + bloodFadeTime;
        bleeding = true;
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
        bleeding = false;
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

            //LineRenderer line = spiral.GetComponent<LineRenderer>();
            //line.positionCount = 1;
            //line.SetPosition(0, topPoint.position);
            spiral.GetComponent<ParticleSystem>().Stop();
            spiral.GetComponent<ParticleSystem>().Clear();
            SetSpiralPosition();
        }

    }

    public void SetSpiralPosition()
    {
        if (topPoint == null) return;
        //spiral.transform.position = topPoint.position;
        //spiral.transform.rotation = Quaternion.LookRotation(pseudoParent.transform.forward);
        spiralParent.position = topPoint.position;
        spiralParent.rotation = Quaternion.LookRotation(pseudoParent.transform.forward);
    }
} 
