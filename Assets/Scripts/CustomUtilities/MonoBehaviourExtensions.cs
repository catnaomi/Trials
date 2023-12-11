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

        public static Coroutine StartTimer(this MonoBehaviour mono, float duration, UnityAction callback)
        {
            return StartTimer(mono, duration, false, callback);
        }

        static IEnumerator TimerRoutine(float duration, bool loops, UnityAction callback)
        {
            do {
                yield return new WaitForSeconds(duration);
                callback.Invoke();
            } while (loops);
        }

        public static Coroutine StartTimerRealtime(this MonoBehaviour mono, float duration, bool loops, UnityAction callback)
        {
            Coroutine routine = mono.StartCoroutine(TimerRoutineRealtime(duration, loops, callback));
            return routine;
        }

        public static Coroutine StartTimerRealtime(this MonoBehaviour mono, float duration, UnityAction callback)
        {
            return StartTimerRealtime(mono, duration, false, callback);
        }

        static IEnumerator TimerRoutineRealtime(float duration, bool loops, UnityAction callback)
        {
            do
            {
                yield return new WaitForSecondsRealtime(duration);
                callback.Invoke();
            } while (loops);
        }
    }
}