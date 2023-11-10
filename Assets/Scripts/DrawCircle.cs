using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DrawCircle {

    const int DEFAULT_RESOLUTION = 36;
    public static void DrawWireCircle(Vector3 center, Vector3 normal, float radius, Color color, float duration = 0f, int resolution = 36)
    {
        Vector3 up = Vector3.Cross(normal, (normal != Vector3.up) ? Vector3.up : Vector3.forward);

        float interval = 360f / (float)resolution;
        for (int i = 0; i < resolution; i++)
        {
            Vector3 p1 = center + GetDegreePoint(i, normal, up, radius, interval);
            Vector3 p2 = center + GetDegreePoint(i - 1, normal, up, radius, interval);
            Debug.DrawLine(p1, p2, color, duration);
        }
    }

    public static void DrawWireCircle(Vector3 center, Vector3 normal, float radius)
    {
        DrawWireCircle(center, normal, radius, Color.white);
    }

    public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0f, int resolution = 36)
    {
        DrawWireCircle(center, Vector3.forward, radius, color, duration, resolution);
        DrawWireCircle(center, Vector3.up, radius, color, duration, resolution);
        DrawWireCircle(center, Vector3.right, radius, color, duration, resolution);
    }


    public static void DrawWireSphere(Vector3 center, float radius)
    {
        DrawWireSphere(center, radius, Color.white);
    }

    static Vector3 GetDegreePoint(int i, Vector3 normal, Vector3 up, float radius, float interval)
    {
        float degrees = interval * i;
        return GetCirclePoint(normal, up, degrees, radius);
    }
    static Vector3 GetCirclePoint(Vector3 normal, Vector3 up, float degrees, float radius)
    {
        Vector3 dir = Quaternion.AngleAxis(degrees, normal) * up.normalized;

        return dir.normalized * radius;
    }
}
