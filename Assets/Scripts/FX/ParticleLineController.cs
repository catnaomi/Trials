using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[ExecuteInEditMode, RequireComponent(typeof(ParticleSystem),typeof(LineRenderer))]
public class ParticleLineController : MonoBehaviour
{
    public bool runInEditMode = true;
    public bool updateLine = true;
    public LineRenderer line;
    public ParticleSystem particleSystem;
    Particle[] particles;
    // Start is called before the first frame update
    void OnEnable()
    {
        Init();   
    }

    // Update is called once per frame
    void Update()
    {
        if ((!runInEditMode && !Application.isPlaying) || !updateLine) return;
        line.positionCount = particleSystem.particleCount;
        particleSystem.GetParticles(particles);
        Array.Sort(particles, (a,b) =>
        {
            return (int)Mathf.Sign(b.remainingLifetime - a.remainingLifetime);
        });
        for (int i = 0; i < particleSystem.particleCount; i++)
        {
            line.SetPosition(i, particles[i].position);
        }
        //line.SetPosition(particleSystem.particleCount - 1, Vector3.zero);
    }

    private void Init()
    {
        line = this.GetComponent<LineRenderer>();
        particleSystem = this.GetComponent<ParticleSystem>();
        particles = new Particle[particleSystem.main.maxParticles];
    }
}
