using Animancer;
using System.Collections;
using UnityEngine;

namespace CustomUtilities
{
    public static class ActorUtilities
    {
        public static bool GetGrounded(Transform transform, out RaycastHit hit)
        {
            return Physics.Raycast(transform.position, -transform.up, out hit, 0.25f, MaskReference.Terrain);
        }

        public static bool GetGrounded(Transform transform)
        {
            return Physics.Raycast(transform.position, -transform.up, 0.25f, MaskReference.Terrain);
        }
    }
}