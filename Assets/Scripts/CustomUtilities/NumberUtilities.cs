using UnityEngine;
using System.Collections;

namespace CustomUtilities
{
    public class NumberUtilities
    {
        // Smoothing rate dictates the proportion of source remaining after one second
        //
        public static float Damp(float source, float target, float smoothing, float dt)
        {
            return Mathf.Lerp(target, source, 1 - Mathf.Pow(smoothing, dt));
        }

        public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt)
        {
            return Vector3.Lerp(target, source, 1 - Mathf.Pow(smoothing, dt));
        }

        public static Vector3 FlattenVector(Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        public static float TimeDelayedSmooth(float current, float target, float timeToStart, float timeToEnd, float currentTime)
        {
            float duration = timeToEnd - timeToStart;

            float progress = Mathf.Clamp((currentTime - timeToStart) / duration, 0f, 1f);

            return Mathf.Lerp(current, target, progress);
        }

        public static float TimeDelayedSmoothDelta(float current, float target, double timeToStart, float maxDelta, double currentTime)
        {
            if (currentTime >= timeToStart)
            {
                return Mathf.MoveTowards(current, target, maxDelta * Time.deltaTime);
            }
            else
            {
                return current;
            }
        }

        public static Vector3 ArrayToVector3(float[] array)
        {
            if (array.Length < 3) return Vector3.zero;
            return new Vector3(array[0], array[1], array[2]);
        }

        public static Quaternion ArraytoQuaternion(float[] array)
        {
            if (array.Length < 4) return Quaternion.identity;

            return new Quaternion(array[0], array[1], array[2], array[3]);
        }

        public static float[] Vector3ToArray(Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }

        public static float[] QuaternionToArray(Quaternion quaternion)
        {
            return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }
    }
}
