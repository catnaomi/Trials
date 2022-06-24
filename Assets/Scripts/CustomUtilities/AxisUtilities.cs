using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace CustomUtilities
{
    public class AxisUtilities
    {

        public enum AxisDirection
        {
            Zero,       // 0
            Forward,    // 1
            Backward,   // 2
            Up,         // 3
            Down,       // 4
            Left,       // 5
            Right       // 6
        }
        /*
         * takes a given direction and finds the closest axis to that direction relative to a transform.
         * ex: stagger in direction of knockback
         * 
         * params:
         * - direction: direction to find closest axis of.
         * - transform: transform of the relative directions to map to. uses world directions if null.
         * - ranges: determines valid directions. options are Horizontal, Vertical, and Sagittal.
         * 
         * returns: axis direction string corresponding to that axis. (up, left, down, etc.)
         */
        public static AxisDirection DirectionToAxisDirection(Vector3 direction, Transform transform, params string[] ranges)
        {
            Vector3 directionNormal = direction.normalized;

            Vector3 forward;
            Vector3 up;
            Vector3 right;

            if (transform != null)
            {
                forward = transform.forward;
                up = transform.up;
                right = transform.right;

            }
            else
            {
                forward = Vector3.forward;
                up = Vector3.up;
                right = Vector3.right;
            }

            Dictionary<Vector3, AxisDirection> directionMap = new Dictionary<Vector3, AxisDirection>();
            foreach (string range in ranges)
            {
                if (range.ToUpper().Contains("NONE") && direction == Vector3.zero)
                {
                    return AxisDirection.Zero;
                }
                if (range.ToUpper().Contains("SAGITTAL"))
                {
                    directionMap[forward] = AxisDirection.Forward;
                    directionMap[-forward] = AxisDirection.Backward;
                }
                if (range.ToUpper().Contains("HORIZONTAL"))
                {
                    directionMap[right] = AxisDirection.Right;
                    directionMap[-right] = AxisDirection.Left;
                }
                if (range.ToUpper().Contains("VERTICAL"))
                {
                    directionMap[up] = AxisDirection.Up;
                    directionMap[-up] = AxisDirection.Down;
                }

            }


            Vector3 leadingDirection = Vector3.zero;

            foreach (Vector3 testDirection in directionMap.Keys)
            {
                if (Vector3.Distance(directionNormal, testDirection) < Vector3.Distance(directionNormal, leadingDirection) ||
                        leadingDirection == Vector3.zero)
                {
                    leadingDirection = testDirection;
                }
            }

            if (directionMap.TryGetValue(leadingDirection, out AxisDirection result))
            {
                return result;
            }
            else
            {
                return AxisDirection.Zero;
            }
        }

        public static AxisDirection DirectionToAxisDirection(Vector3 direction, params string[] ranges)
        {
            return DirectionToAxisDirection(direction, null, ranges);
        }

        /*
         * as above, but with 2D vectors.
         */
        public static AxisDirection DirectionToAxisDirection(Vector2 direction, params string[] ranges)
        {
            Vector3 directionConvert = new Vector3(direction.x, direction.y);

            return DirectionToAxisDirection(directionConvert, ranges);
        }

        /*
         * takes a position and finds the closest axis of that position relative to a transform.
         * ex: check if position is to left or right of transform
         * 
         * params:
         * - position: position to find closest axis of.
         * - transform: transform of the relative directions to map to. uses world directions if null.
         * - ranges: determines valid directions. options are Horizontal, Vertical, and Sagittal.
         */
        public static AxisDirection PositionToAxisDirection(Vector3 position, Transform transform, params string[] ranges)
        {
            Vector3 direction = transform.position - position;
            return DirectionToAxisDirection(direction, transform, ranges);
        }

        /*
         * maps each axis direction (up, left, down, etc.) to one of a given list of transforms. 
         * ex: determine responses to each direction when changing target with right stick.
         * 
         * params:
         * - anchor: transform of the relative position to map to.
         * - transforms: each other transform that directions need to be determined for.
         */
        public static Dictionary<AxisDirection, Transform> MapTransformsToAxisDirections(Transform basis, Vector3 anchor, IEnumerable<Transform> transforms)
        {
            Dictionary<AxisDirection, Transform> map = new Dictionary<AxisDirection, Transform>();

            Dictionary<Transform, Dictionary<AxisDirection, float>> transformRankMap = new Dictionary<Transform, Dictionary<AxisDirection, float>>();
            foreach (Transform testTransform in transforms)
            {
                Dictionary<AxisDirection, float> rank = new Dictionary<AxisDirection, float>();
                foreach (AxisDirection axis in GetCardinals())
                {
                    Vector3 testDirection = testTransform.position - anchor;
                    Vector3 axisDir = AxisDirectionToTransformDirection(basis, axis);
                    float dot = Vector3.Dot(testDirection, axisDir);
                    rank[axis] = dot;
                }
                transformRankMap[testTransform] = rank;
            }

            List<Transform> remainingTransform = new List<Transform>();
            remainingTransform.AddRange(transforms);
            Vector2 metric = new Vector2(1, 0);
            bool deleteOnFind = remainingTransform.Count > 4;
            foreach (AxisDirection axis in GetCardinals())
            {
                AxisDirection axis2 = GetRightHandDirection(axis);
                float leading = Mathf.Infinity;
                Transform lead = null;
                foreach (Transform testTransform in remainingTransform)
                {
                    float val1 = transformRankMap[testTransform][axis];
                    float val2 = transformRankMap[testTransform][axis2];
                    Vector2 testVector = new Vector2(val1, val2);

                    float dist = Vector2.Distance(testVector, metric);
                    if (dist < leading)
                    {
                        leading = dist;
                        lead = testTransform;
                    }
                }
                if (deleteOnFind && lead != null)
                {
                    remainingTransform.Remove(lead);
                }
                map[axis] = lead;
            }
            return map;
            /*
            foreach (AxisDirection axis in Enum.GetValues(typeof(AxisDirection)))
            {
                Transform leadingTransform = null;
                foreach (Transform testTransform in transforms)
                {
                    if (leadingTransform == null)
                    {
                        leadingTransform = testTransform;
                    }
                    else
                    {
                        Vector3 leadingDirection = leadingTransform.position - anchor;
                        Vector3 testDirection = testTransform.position - anchor;

                        float leadingDistance = Vector3.Distance(leadingDirection, AxisDirectionToTransformDirection(basis, axis));
                        float testDistance = Vector3.Distance(testDirection, AxisDirectionToTransformDirection(basis, axis));

                        if (axis == AxisDirection.Left)
                        {

                        }
                        if (testDistance < leadingDistance)
                        {
                            leadingTransform = testTransform;
                        }
                    }
                }
                map[axis] = leadingTransform;
            }

            return map;
            */
        }

        /*
        public static Dictionary<AxisDirection, GameObject> MapGameObjectsToAxisDirections(GameObject anchorObject, IEnumerable<GameObject> objects)
        {
            Transform anchor = anchorObject.transform;
            List<Transform> transforms = new List<Transform>();
            foreach (GameObject gameObject in objects)
            {
                transforms.Add(gameObject.transform);
            }

            Dictionary<AxisDirection, GameObject> gameObjectMap = new Dictionary<AxisDirection, GameObject>();
            Dictionary<AxisDirection, Transform> transformMap = MapTransformsToAxisDirections(anchor, transforms);

            foreach (AxisDirection axis in transformMap.Keys)
            {
                gameObjectMap[axis] = transformMap[axis].gameObject;
            }

            return gameObjectMap;
        }
        */

        public static AxisDirection GetRightHandDirection(AxisDirection axis)
        {
            if (axis == AxisDirection.Up)
            {
                return AxisDirection.Right;
            }
            else if (axis == AxisDirection.Right)
            {
                return AxisDirection.Down;
            }
            else if (axis == AxisDirection.Down)
            {
                return AxisDirection.Left;
            }
            else if (axis == AxisDirection.Left)
            {
                return AxisDirection.Up;
            }
            return AxisDirection.Zero;
        }

        public static AxisDirection[] GetCardinals()
        {
            return new AxisDirection[] { AxisDirection.Up, AxisDirection.Down, AxisDirection.Left, AxisDirection.Right };
        }

        /*
         * converts an axis direction to a relative direction based relative to a transform. 
         * 
         * params:
         * - axis: axis direction to convert.
         * - transform: transform to find relative directions of.
         */
        public static Vector3 AxisDirectionToTransformDirection(Transform transform, AxisDirection axis)
        {
            switch (axis)
            {
                case AxisDirection.Up:
                    return transform.up;
                case AxisDirection.Right:
                    return transform.right;
                case AxisDirection.Forward:
                    return transform.forward;
                case AxisDirection.Down:
                    return -transform.up;
                case AxisDirection.Left:
                    return -transform.right;
                case AxisDirection.Backward:
                    return -transform.forward;
                default:
                    return Vector3.zero;
            }
        }

        /*
         * converts axis direction(s) (up, left, down, etc.)
         * to a Ternary Vector. (-1, 0, 1).
         * x is horizontal/right/left
         * y is vertical/up/down
         * z is sagittal/forward/backward
         * 
         * params:
         * - axes: axis directions to convert.
         */
        public static Vector3 AxisDirectionsToTernaryVector(params AxisDirection[] axes)
        {
            Vector3 ternaryVector = new Vector3(0, 0, 0);
            foreach (AxisDirection axis in axes)
            {
                switch (axis)
                {
                    case AxisDirection.Up:
                        ternaryVector += new Vector3(0, 1, 0);
                        continue;
                    case AxisDirection.Down:
                        ternaryVector += new Vector3(0, -1, 0);
                        continue;
                    case AxisDirection.Right:
                        ternaryVector += new Vector3(1, 0, 0);
                        continue;
                    case AxisDirection.Left:
                        ternaryVector += new Vector3(-1, 0, 0);
                        continue;
                    case AxisDirection.Forward:
                        ternaryVector += new Vector3(0, 0, 1);
                        continue;
                    case AxisDirection.Backward:
                        ternaryVector += new Vector3(0, 0, -1);
                        continue;
                }
            }

            return ternaryVector;
        }

        public static string AxisDirectionToString(AxisDirection axis)
        {
            return Enum.GetName(typeof(AxisDirection), axis);
        }

        public static AxisDirection InvertAxis(AxisDirection axis, bool invertHorizontal, bool invertVertical, bool invertSagittal)
        {
            if (axis == AxisDirection.Left && invertHorizontal)
            {
                return AxisDirection.Right;
            }
            else if (axis == AxisDirection.Right && invertHorizontal)
            {
                return AxisDirection.Left;
            }
            else if (axis == AxisDirection.Up && invertVertical)
            {
                return AxisDirection.Down;
            }
            else if (axis == AxisDirection.Down && invertVertical)
            {
                return AxisDirection.Up;
            }
            else if (axis == AxisDirection.Forward && invertSagittal)
            {
                return AxisDirection.Backward;
            }
            else if (axis == AxisDirection.Backward && invertSagittal)
            {
                return AxisDirection.Forward;
            }
            else
            {
                return axis;
            }
        }

        public static AxisDirection ConvertAxis(AxisDirection axis, string source, string destination)
        {
            int sign = 0;

            if (source.ToUpper().Contains("SAGGITAL"))
            {
                if (axis == AxisDirection.Forward)
                {
                    sign = 1;
                }
                else if (axis == AxisDirection.Backward)
                {
                    sign = -1;
                }
            }
            else if (source.ToUpper().Contains("HORIZONTAL"))
            {
                if (axis == AxisDirection.Right)
                {
                    sign = 1;
                }
                else if (axis == AxisDirection.Left)
                {
                    sign = -1;
                }
            }
            else if (source.ToUpper().Contains("VERTICAL"))
            {
                if (axis == AxisDirection.Up)
                {
                    sign = 1;
                }
                else if (axis == AxisDirection.Down)
                {
                    sign = -1;
                }
            }


            if (destination.ToUpper().Contains("SAGGITAL"))
            {
                if (sign == 1)
                {
                    return AxisDirection.Forward;
                }
                else if (sign == -1)
                {
                    return AxisDirection.Backward;
                }
            }
            else if (destination.ToUpper().Contains("HORIZONTAL"))
            {
                if (sign == 1)
                {
                    return AxisDirection.Right;
                }
                else if (sign == -1)
                {
                    return AxisDirection.Left;
                }
            }
            else if (destination.ToUpper().Contains("VERTICAL"))
            {
                if (sign == 1)
                {
                    return AxisDirection.Up;
                }
                else if (sign == -1)
                {
                    return AxisDirection.Down;
                }
            }
            return axis;
        }

        public static Vector3[] GetSortedPositionsByDistances(Vector3 origin, params Vector3[] vectors)
        {
            Array.Sort(vectors, (a, b) =>
            {
                return (int)Mathf.Sign(Vector3.Distance(origin, a) - Vector3.Distance(origin, b));
            });


            return vectors;
        }

        public static IEnumerable<Vector3> GetSortedPositionsByDistances(Vector3 origin, IEnumerable<Vector3> vectors)
        {

            ((List<Vector3>)vectors).Sort((a, b) =>
            {
                return (int)Mathf.Sign(Vector3.Distance(origin, a) - Vector3.Distance(origin, b));
            });

            return vectors;
        }

        public static Transform[] GetSortedTransformsByDistances(Vector3 origin, params Transform[] transforms)
        {
            Array.Sort(transforms, (a, b) =>
            {
                return (int)Mathf.Sign(Vector3.Distance(origin, a.position) - Vector3.Distance(origin, b.position));
            });


            return transforms;
        }

        public static IEnumerable<Transform> GetSortedTransformsByDistances(Vector3 origin, IEnumerable<Transform> transforms)
        {

            ((List<Transform>)transforms).Sort((a, b) =>
            {
                return (int)Mathf.Sign(Vector3.Distance(origin, a.position) - Vector3.Distance(origin, b.position));
            });

            return transforms;
        }
    }

    
}