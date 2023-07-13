using Cinemachine;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantFXHelper : MonoBehaviour
{
    IceGiantMecanimActor actor;
    public ParticleSystem stompParticle;
    public ParticleSystem footReformParticleLeft;
    public ParticleSystem footReformParticleRight;
    public ParticleSystem stepParticle;
    public ParticleSystem stepSmallParticle;
    public ParticleSystem handOutParticle;
    public ParticleSystem spinHandParticle;
    public ParticleSystem smallIceParticle;
    public AudioSource waterExplosionSource;
    public AudioSource waterSplashSource;
    public AudioClip splash1;
    public AudioClip splash2;
    bool splashAlternator;

    bool wasRotatingLastFrame;
    // Start is called before the first frame update
    void Start()
    {
        actor = this.GetComponent<IceGiantMecanimActor>();
    }

    // Update is called once per frame
    void Update()
    {
        bool rotating = actor.IsRotating();
        if (rotating && !wasRotatingLastFrame)
        {
            spinHandParticle.transform.position = this.transform.position + this.transform.forward;
            spinHandParticle.Play();
            spinHandParticle.GetComponent<AudioSource>().Play();
        }
        else if (!rotating && wasRotatingLastFrame)
        {
            spinHandParticle.Stop();
            spinHandParticle.GetComponent<AudioSource>().Stop();
        }
        wasRotatingLastFrame = rotating;

        if (actor.spinning)
        {
            Vector3 position = actor.LeftHand.position;
            position.y = 0f;

            spinHandParticle.transform.position = position;
        }
    }

    public void PlayReformFoot(bool isLeft)
    {
        ParticleSystem particle = (isLeft) ? footReformParticleLeft : footReformParticleRight;
        particle.Play();
    }

    public void StompFX(bool isLeft)
    {

        Transform foot = (isLeft) ? actor.leftLeg.transform : actor.rightLeg.transform;

        Vector3 position = foot.position;
        position.y = this.transform.position.y;
        PlayParticleAtPosition(stompParticle, position);
        PlayWaterExplosion();
    }

    public void HandShockwaveInFX()
    {
        Vector3 position = actor.LeftHand.position;
        position.y = this.transform.position.y;
        // activate particle only
        PlayParticleAtPosition(smallIceParticle, position);
        PlayWaterSplash();
    }
    public void HandShockwaveOutFX()
    {
        Vector3 position = actor.LeftHand.position;
        position.y = this.transform.position.y;
        PlayParticleAtPosition(handOutParticle, position);
        PlayWaterSplash();
    }

    public void StepFX(Vector3 position, bool isWeak)
    {

        if (!isWeak)
        {
            PlayParticleAtPosition(stepParticle, position);
        }
        else
        {
            PlayParticleAtPosition(stepSmallParticle, position);
        }
        PlayWaterSplash();
    }
    void PlayParticleAtPosition(ParticleSystem particle, Vector3 position)
    {
        particle.transform.position = position;
        particle.Play();
        if (particle.TryGetComponent(out CinemachineImpulseSource impulse))
        {
            impulse.GenerateImpulse();
        }
        if (particle.TryGetComponent(out AudioSource source))
        {
            source.Play();
        }

    }

    public void PlayWaterExplosion()
    {
        if (waterExplosionSource != null)
        {
            waterExplosionSource.Play();
        }
    }

    public void PlayWaterSplash()
    {
        if (waterSplashSource != null)
        {
            waterSplashSource.PlayOneShot((splashAlternator) ? splash1 : splash2);
            splashAlternator = !splashAlternator;
        }
    }

    public void SpinFXStart()
    {
        Vector3 position = actor.LeftHand.position;
        position.y = 0f;

        spinHandParticle.transform.position = position;
        spinHandParticle.Play();
        spinHandParticle.GetComponent<AudioSource>().Play();
    }

    public void SpinFXStop()
    {
        spinHandParticle.Stop();
        spinHandParticle.GetComponent<AudioSource>().FadeOut(1f, this);
    }
}
