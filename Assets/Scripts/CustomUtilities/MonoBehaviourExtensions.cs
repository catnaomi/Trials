using UnityEngine;
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

        // Timer for rendering which both updates some render target periodically and executes callback on completion
        public delegate void UpdateCallback(float elapsedFractional);
        public static Coroutine StartRenderTimer(this MonoBehaviour behaviour, float duration, UpdateCallback updateCallback, System.Action finishedCallback)
        {
            return behaviour.StartCoroutine(Update(duration, updateCallback, finishedCallback));
        }
        static IEnumerator Update(float duration, UpdateCallback updateCallback, System.Action finishedCallback)
        {
            if (duration > 0f)
            {
                float elapsed = 0f;
                updateCallback(0f);
                while (elapsed < duration)
                {
                    yield return new WaitForEndOfFrame();
                    elapsed += Time.unscaledDeltaTime;
                    updateCallback(elapsed / duration);
                }
            }
            updateCallback(1f);
            finishedCallback?.Invoke();
        }
    }
}