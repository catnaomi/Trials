using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossXOParticleController : MonoBehaviour
{
    public ParticleSystem circle;
    public ParticleSystem cross;

    public float interval = 0.1f;
    
    public void Telegraph(string sequence)
    {
        string[] charSequence = sequence.Split(" ");

        StartCoroutine(TelegraphCoroutine(charSequence));
    }

    IEnumerator TelegraphCoroutine(string[] charSequence)
    {
        for (int i = 0; i < charSequence.Length; i++)
        {
            string parry = charSequence[i];
           
            if (parry.ToUpper() == "O")
            {
                circle.Play();
            }
            else if (parry.ToUpper() == "X")
            {
                cross.Play();
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
