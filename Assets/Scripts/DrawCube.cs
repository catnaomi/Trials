using System.Collections;
using UnityEngine;

public static class DrawCube
{

    public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion orientation, Color color, float duration = 0f)
    {
        Vector3 forward = orientation * Vector3.forward;
        Vector3 up = orientation * Vector3.up;
        Vector3 right = orientation * Vector3.right;


        Vector3 p1 = center + forward * size.z *  0.5f + up * size.y *  0.5f + right * size.x * -0.5f;
        Vector3 p2 = center + forward * size.z *  0.5f + up * size.y *  0.5f + right * size.x *  0.5f;
        Vector3 p3 = center + forward * size.z * -0.5f + up * size.y *  0.5f + right * size.x *  0.5f;
        Vector3 p4 = center + forward * size.z * -0.5f + up * size.y *  0.5f + right * size.x * -0.5f;

        Vector3 p5 = center + forward * size.z *  0.5f + up * size.y *  -0.5f + right * size.x * -0.5f;
        Vector3 p6 = center + forward * size.z *  0.5f + up * size.y *  -0.5f + right * size.x *  0.5f;
        Vector3 p7 = center + forward * size.z * -0.5f + up * size.y *  -0.5f + right * size.x *  0.5f;
        Vector3 p8 = center + forward * size.z * -0.5f + up * size.y *  -0.5f + right * size.x * -0.5f;
        // corners
        Debug.DrawLine(p1,p5, color, duration);
        Debug.DrawLine(p2,p6, color, duration);
        Debug.DrawLine(p3,p7, color, duration);
        Debug.DrawLine(p4,p8, color, duration);


        // top
        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(p2, p3, color, duration);
        Debug.DrawLine(p3, p4, color, duration);
        Debug.DrawLine(p4, p1, color, duration);

        // bottom
        Debug.DrawLine(p5, p6, color, duration);
        Debug.DrawLine(p6, p7, color, duration);
        Debug.DrawLine(p7, p8, color, duration);
        Debug.DrawLine(p8, p5, color, duration);
    }

    public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion orientation)
    {
        DrawWireCube(center, size, orientation, Color.white);
    }


    public static void DrawWireCube(Vector3 center, Vector3 size)
    {
        DrawWireCube(center, size, Quaternion.identity, Color.white);
    }

    public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
    {
        DrawWireCube(bounds.center, bounds.extents, Quaternion.identity, color, duration);
    }

    public static void DrawBounds(Collider collider, Color color, float duration = 0f)
    {
        DrawBounds(collider.bounds, color, duration);
    }

    public static void DrawBoundsAt(Bounds bounds, Vector3 center, Color color, float duration = 0f)
    {
        DrawWireCube(center, bounds.extents, Quaternion.identity, color, duration);
    }

    public static void DrawBoundsAt(Collider collider, Vector3 center, Color color, float duration = 0f)
    {
        DrawBoundsAt(collider.bounds, center, color, duration);
    }
}