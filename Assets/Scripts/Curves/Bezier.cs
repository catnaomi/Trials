using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 GetPoint(float t, Vector3[] controlPoints)
    {
        if (controlPoints.Length <= 0)
        {
            return Vector3.zero;
        }
        else if (controlPoints.Length == 1)
        {
            return controlPoints[0];
        }
        else if (controlPoints.Length == 2)
        {
            return Vector3.Lerp(controlPoints[0], controlPoints[1], t);
        }
        else if (controlPoints.Length == 3)
        {
            return QuadraticCurve(t, controlPoints[0], controlPoints[1], controlPoints[2]);
        }
        else if (controlPoints.Length >= 4)
        {
            return CubicCurve(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
        }
        return Vector3.zero;
    }

    static Vector3 QuadraticCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
    }

    static Vector3 CubicCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;
    }

    public static Vector3 GetTangent(float t, Vector3[] controlPoints)
    {
        if (controlPoints.Length <= 1)
        {
            return Vector3.forward;
        }
        else if (controlPoints.Length == 2)
        {
            return (controlPoints[1] - controlPoints[0]).normalized;
        }
        else if (controlPoints.Length == 3)
        {
            return QuadraticDerivative(t, controlPoints[0], controlPoints[1], controlPoints[2]);
        }
        else if (controlPoints.Length >= 4)
        {
            return CubicDerivative(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
        }
        return Vector3.forward;
    }

    static Vector3 QuadraticDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
    }

    static Vector3 CubicDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 3 * Mathf.Pow(1 - t, 2) * (p1 - p0) + 6 * (1 - t) * t * (p2 - p1) + 3 * Mathf.Pow(t, 2) * (p3 - p2);
    }     
}
