using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LineSwordThrust : MonoBehaviour
{
    public Transform topPoint;
    public Transform bottomPoint;
    [ReadOnly] public List<Vector3> points;
    public Transform pseudoParent;
    public ParticleSystem[] bloodParticles = new ParticleSystem[3];
    public LineRenderer[] lineRenderers = new LineRenderer[3];
    public float[] lineTimers = new float[3];
    
    int currentIndex = 0;
    float TRAIL_FPS = 60f;
    public float lineFadeTime = 0.5f;
    public float lineWidth = 0.1f;
    public float sublineWidth = 0.25f;
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
    public UnityEvent OnBleed;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("UpdateAtFPS");
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < lineTimers.Length; i++)
        {
            lineTimers[i] = lineTimers[i] - Time.deltaTime;
            if (lineTimers[i] <= 0)
            {
                lineRenderers[i].gameObject.SetActive(false);
                lineTimers[i] = 0;
            }
        }
        if (thrusting)
        {
            lineTimers[currentIndex] = lineFadeTime;
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

    public void OnDestroy()
    {
        StopCoroutine("UpdateAtFPS");
    }
    public void BeginThrust()
    {
        if (bottomPoint == null || topPoint == null)
        {
            Debug.Log("one or more of thrust reference points missing!");
            return;
        }
        thrusting = true;
        points.Clear();
        lineRenderers[currentIndex].gameObject.SetActive(true);
        lineTimers[currentIndex] = lineFadeTime;
        lineRenderers[currentIndex].SetPosition(0, bottomPoint.position);
        lineRenderers[currentIndex].SetPosition(1, (lineRenderers[currentIndex].GetPosition(0) + topPoint.position) / 2f);
        lineRenderers[currentIndex].SetPosition(2, topPoint.position);
        lineRenderers[currentIndex].widthMultiplier = lineWidth;

        LineRenderer subline = lineRenderers[currentIndex].transform.GetChild(0).GetComponent<LineRenderer>();
        subline.SetPosition(0, bottomPoint.position + Vector3.up * -0.01f);
        subline.SetPosition(1, (lineRenderers[currentIndex].GetPosition(0) + topPoint.position) / 2f);
        subline.SetPosition(2, topPoint.position);
        subline.widthMultiplier = sublineWidth;
    }

    public void EndThrust()
    {
        thrusting = false;
        currentIndex++;
        currentIndex %= lineRenderers.Length;
    }

    IEnumerator UpdateAtFPS()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / TRAIL_FPS);
            UpdateTrail();
        }
    }
    private void UpdateTrail()
    {
        if (bottomPoint == null || topPoint == null)
        {
            return;
        }
        if (thrusting)
        {
            //lineRenderers[currentIndex].SetPosition(0, bottomPoint.position);
            lineRenderers[currentIndex].SetPosition(1, (lineRenderers[currentIndex].GetPosition(0) + topPoint.position) / 2f);
            lineRenderers[currentIndex].SetPosition(2, topPoint.position);
            LineRenderer subline = lineRenderers[currentIndex].transform.GetChild(0).GetComponent<LineRenderer>();
            //subline.SetPosition(0, bottomPoint.position);
            subline.SetPosition(1, (subline.GetPosition(0) + topPoint.position) / 2f);
            subline.SetPosition(2, topPoint.position);

            lineTimers[currentIndex] = lineFadeTime;
        }

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            LineRenderer line = lineRenderers[i];
            LineRenderer subline = lineRenderers[i].transform.GetChild(0).GetComponent<LineRenderer>();

            line.widthMultiplier = (lineTimers[i] / lineFadeTime) * lineWidth;
            subline.widthMultiplier = (lineTimers[i] / lineFadeTime) * sublineWidth;
        }
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
} 
