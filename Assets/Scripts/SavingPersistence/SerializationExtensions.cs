using UnityEngine;

// Quaternions and Vectors half self referential properties
// so we convert them to float[] for storage
public static class SerializationExtensions
{
    public static float[] toFloatArray(this Quaternion quaternion)
    {
        return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
    }

    public static Quaternion toQuaternion(this float[] quaternion_components)
    {
        return new Quaternion(quaternion_components[0], quaternion_components[1], quaternion_components[2], quaternion_components[3]);
    }

    public static float[] toFloatArray(this Vector3 vector)
    {
        return new float[] { vector.x, vector.y, vector.z };
    }

    public static Vector3 toVector3(this float[] vector_components)
    {
        return new Vector3(vector_components[0], vector_components[1], vector_components[2]);
    }
}