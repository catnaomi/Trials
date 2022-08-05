using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyToTargetThenEmitSecondary : MonoBehaviour
{
    public ParticleSystem mainParticles;
    public ParticleSystem secondaryParticles;
    public Vector3 startingPosition;
    public Vector3 endingPosition;
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float timeToArrive = 1f;
    float clock;
    public bool flying;

    public void Fly(Vector3 start, Vector3 end)
    {
        startingPosition = start;
        endingPosition = end;
        clock = 0f;
        flying = true;
        mainParticles.Play();
        secondaryParticles.Stop();
    }

    private void Update()
    {
        if (flying)
        {
            clock += Time.deltaTime;
            float t = Mathf.Clamp01(clock / timeToArrive);
            this.transform.position = Vector3.Lerp(startingPosition, endingPosition, curve.Evaluate(t));

            if (clock >= timeToArrive)
            {
                mainParticles.Stop();
                secondaryParticles.Play();
                flying = false;
            }
        }
    }
}
