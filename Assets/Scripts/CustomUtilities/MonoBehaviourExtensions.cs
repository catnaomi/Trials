using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;

namespace CustomUtilities
{
    public static class MonoBehaviourExtensions
    {


        public static Coroutine StartTimer(this MonoBehaviour mono, float duration, bool loops, UnityAction callback)
        {
            Coroutine routine = mono.StartCoroutine(TimerRoutine(duration, loops, callback));
            return routine;
        }

        static IEnumerator TimerRoutine(float duration, bool loops, UnityAction callback)
        {
            do {
                yield return new WaitForSecondsRealtime(duration);
                callback.Invoke();
            } while (loops);
        }
    }
}