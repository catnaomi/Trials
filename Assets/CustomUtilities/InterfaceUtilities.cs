using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace CustomUtilities
{
    public class InterfaceUtilities
    {

        public static void GizmosDrawText(Vector3 position, Color color, string text)
        {
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color;
            Handles.color = color;
            Handles.Label(position, text, style);
        }

        public static void GizmosDrawTransform(Transform transform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        public static void GizmosDrawWireTransform(Transform transform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        public static string GetClosestString(string search, IEnumerable<string> options)
        {
            string lead = "";
            int leadVal = -1;

            foreach(string candidate in options)
            {
                if (candidate.Equals(search))
                {
                    return candidate;
                }
                if (!candidate.ToUpper().Contains(search.ToUpper()))
                {
                    continue;
                }
                int candidateVal = LevenshteinDistance.Compute(search, candidate);
                if (leadVal == -1 || candidateVal < leadVal)
                {
                    lead = candidate;
                    leadVal = candidateVal;
                }
            }

            return lead;
        }

        public static void SetLayerRecursively(GameObject obj, string layer)
        {
            obj.layer = LayerMask.NameToLayer(layer);

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
