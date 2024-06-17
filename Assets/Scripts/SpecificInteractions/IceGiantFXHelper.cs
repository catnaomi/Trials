using Cinemachine;
using CustomUtilities;
using UnityEngine;

public class IceGiantFXHelper : MonoBehaviour
{
    IceGiantMecanimActor actor;
    public ParticleSystem stompParticle;
    public ParticleSystem footReformParticleLeft;
    public ParticleSystem footReformParticleRight;
    public ParticleSystem[] footReformParticles;
    public ParticleSystem stepParticle;
    public ParticleSystem stepSmallParticle;
    public ParticleSystem handOutParticle;
    public ParticleSystem spinHandParticle;
    public ParticleSystem smallIceParticle;
    public AudioSource waterExplosionSource;
    public AudioSource waterSplashSource;
    bool splashAlternator;
    bool wasRotatingLastFrame;

    void Start()
    {
        actor = GetComponent<IceGiantMecanimActor>();
        footReformParticles = new ParticleSystem[2]{ footReformParticleLeft, footReformParticleRight };
    }

    void Update()
    {
        bool rotating = actor.IsRotating();
        if (rotating && !wasRotatingLastFrame)
        {
            spinHandParticle.transform.position = transform.position + transform.forward;
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

    public void PlayReformFoot(int legIndex)
    {
        footReformParticles[legIndex].Play();
    }

    public void StompFX(int legIndex)
    {
        Transform foot = actor.legs[legIndex].transform;
        Vector3 position = foot.position;
        position.y = transform.position.y;
        PlayParticleAtPosition(stompParticle, position);
        PlayWaterExplosion();
    }

    public void HandShockwaveInFX()
    {
        Vector3 position = actor.LeftHand.position;
        position.y = transform.position.y;
        // activate particle only
        PlayParticleAtPosition(smallIceParticle, position);
        PlayWaterSplash();
    }

    public void HandShockwaveOutFX()
    {
        Vector3 position = actor.LeftHand.position;
        position.y = transform.position.y;
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
        SoundFXAssetManager.PlaySound(waterExplosionSource, "Swim/Splash/Bigger");
    }

    public void PlayWaterSplash()
    {
        SoundFXAssetManager.PlaySound(waterSplashSource, "Swim/Splash", splashAlternator ? "Small" : "Big");
        splashAlternator = !splashAlternator;
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
