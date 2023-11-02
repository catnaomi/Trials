using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DojoBossXOParticleController : MonoBehaviour
{
    public ParticleSystem circle;
    public AudioSource circleAudio;
    public ParticleSystem cross;
    public AudioSource crossAudio;

    public float interval = 0.1f;


    public UnityEvent OnTelegraphStart;
    public void Telegraph(string sequence)
    {
        string[] charSequence = sequence.Split(" ");

        StartCoroutine(TelegraphCoroutine(charSequence));
        OnTelegraphStart.Invoke();
    }

    IEnumerator TelegraphCoroutine(string[] charSequence)
    {
        for (int i = 0; i < charSequence.Length; i++)
        {
            string parry = charSequence[i];

            if (parry.ToUpper() == "O")
            {
                circle.Play();
                circleAudio.Play();
            }
            else if (parry.ToUpper() == "X")
            {
                cross.Play();
                crossAudio.Play();
            }
            yield return new WaitForSeconds(interval);
        }
    }


    public void TelegraphComplex(string sequence)
    {
        string[] charSequence = sequence.Split(",");

        StartCoroutine(TelegraphComplexCoroutine(charSequence));
        OnTelegraphStart.Invoke();
    }


    IEnumerator TelegraphComplexCoroutine(string[] charSequence)
    {
        for (int i = 0; i < charSequence.Length; i++)
        {
            string parry = charSequence[i];

            if (parry.ToUpper() == "O")
            {
                circle.Play();
                circleAudio.Play();
            }
            else if (parry.ToUpper() == "X")
            {
                cross.Play();
                crossAudio.Play();
            }
            else if (float.TryParse(parry, out float interval))
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    public void TelegraphOne(string sequence)
    {
        if (sequence == "X")
        {
            cross.Play();
            crossAudio.Play();
        }
        else if (sequence == "O")
        {
            circle.Play();
            circleAudio.Play();
        }
    }
}
