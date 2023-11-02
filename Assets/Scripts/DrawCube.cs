using System.Collections;
using UnityEngine;

public static class DrawCube
{

    public static void ForDebug(Vector3 center, Vector3 size, Quaternion orientation, Color color, float duration)
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
    public static void ForDebug(Vector3 center, Vector3 size, Quaternion orientation, Color color)
    {
        Vector3 forward = orientation * Vector3.forward;
        Vector3 up = orientation * Vector3.up;
        Vector3 right = orientation * Vector3.right;


        Vector3 p1 = center + forward * size.z * 0.5f + up * size.y * 0.5f + right * size.x * -0.5f;
        Vector3 p2 = center + forward * size.z * 0.5f + up * size.y * 0.5f + right * size.x * 0.5f;
        Vector3 p3 = center + forward * size.z * -0.5f + up * size.y * 0.5f + right * size.x * 0.5f;
        Vector3 p4 = center + forward * size.z * -0.5f + up * size.y * 0.5f + right * size.x * -0.5f;

        Vector3 p5 = center + forward * size.z * 0.5f + up * size.y * -0.5f + right * size.x * -0.5f;
        Vector3 p6 = center + forward * size.z * 0.5f + up * size.y * -0.5f + right * size.x * 0.5f;
        Vector3 p7 = center + forward * size.z * -0.5f + up * size.y * -0.5f + right * size.x * 0.5f;
        Vector3 p8 = center + forward * size.z * -0.5f + up * size.y * -0.5f + right * size.x * -0.5f;
        // corners
        Debug.DrawLine(p1, p5, color);
        Debug.DrawLine(p2, p6, color);
        Debug.DrawLine(p3, p7, color);
        Debug.DrawLine(p4, p8, color);


        // top
        Debug.DrawLine(p1, p2, color);
        Debug.DrawLine(p2, p3, color);
        Debug.DrawLine(p3, p4, color);
        Debug.DrawLine(p4, p1, color);

        // bottom
        Debug.DrawLine(p5, p6, color);
        Debug.DrawLine(p6, p7, color);
        Debug.DrawLine(p7, p8, color);
        Debug.DrawLine(p8, p5, color);
    }

    public static void ForDebug(Vector3 center, Vector3 size, Quaternion orientation)
    {
        ForDebug(center, size, orientation, Color.white);
    }


    public static void ForDebug(Vector3 center, Vector3 size)
    {
        ForDebug(center, size, Quaternion.identity, Color.white);
    }
}