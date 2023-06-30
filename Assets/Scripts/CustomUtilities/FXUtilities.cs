using System.Collections;
using UnityEngine;

namespace CustomUtilities
{
    public static class FXUtilities 
    {
        public static void FadeOut(this AudioSource audioSource, float FadeTime, MonoBehaviour routineSource)
        {
            if (routineSource != null)
            {
                routineSource.StartCoroutine(FadeOutRoutine(audioSource, FadeTime));
            }
        }

        static IEnumerator FadeOutRoutine(AudioSource audioSource, float FadeTime)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }
    }
}