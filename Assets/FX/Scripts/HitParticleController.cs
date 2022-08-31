using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticleController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public LineRenderer lineRenderer;
    public float fadeDelay = 2f;
    public float fadeTime = 1f;
    float clock;
    bool emitting;

    Vector3[] linePoints = { 0.75f * Vector3.forward, 0.5f * Vector3.forward, 0.25f * Vector3.forward, Vector3.zero, -0.25f * Vector3.forward, -0.5f * Vector3.forward, -0.75f * Vector3.forward };
    public void SetPoints(Vector3 position, Vector3 direction)
    {
        this.transform.position = position;
        this.transform.rotation = Quaternion.LookRotation(direction);
    }
    public void Emit()
    {
        particleSystem.Play();
    }
}
