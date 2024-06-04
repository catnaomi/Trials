using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Animancer.Easing;

public class WyrmAnimHandler : MonoBehaviour
{
    public float sineSpeed = 1f;
    public float sineAmplitude = 1f;
    
    public float lookAngleSpeed = 90f;
    public float lookAngleAccel = 360f;
    public float sineAngle = 45f;
    public float friction = 0.95f;

    public float maxSineAngleSpeed = 90f;
    public float sinTSpeed = 1f;
    [ReadOnly] public float sinT;
    [Header("Animated Fields")]
    public float sineAnimMult = 1f;
    [Header("References")]
    public Transform bodyRoot;
    [SerializeField] TailSegment[] segments;

    Vector3 lastDirection;
    double time;
    [Serializable]
    struct TailSegment
    {
        public Transform transform;
        [ReadOnly] public Vector3 direction;
        public float distanceOffset;
        [Range(0f, 1f)] public float sineOffset;
        public float amplitudeMult;
        public float maxAngleDiff;
        [ReadOnly] public float angSpeed;
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < segments.Length; i++)
        {

            TailSegment segment = segments[i];

            segment.direction = bodyRoot.forward;

            segments[i] = segment;
        }
        lastDirection = bodyRoot.forward;
    }

    // Update is called once per frame
    void Update()
    {
        float angVel = Vector3.SignedAngle(lastDirection, bodyRoot.forward, Vector3.up);

        sinT = Mathf.MoveTowards(sinT, 1 - Mathf.Clamp01(Mathf.Abs(angVel) / (maxSineAngleSpeed * Time.deltaTime)), Time.deltaTime * sinTSpeed);

        float sineAngleReal = sineAngle * sinT;
        time += Time.deltaTime;

        float speed = sineSpeed * sineAnimMult;
        if (sinT == 0 && speed != 0)
        {
            if (angVel > 0)
            {
                time = 0;// Mathf.PI * 0.5f * (1 / speed);
            }
            else
            {
                time = Mathf.PI * (1 / speed);
            }
        }
        for (int i = 0; i < segments.Length; i++)
        {

            TailSegment segment = segments[i];

            Vector3 referenceDirection = bodyRoot.forward;
            Vector3 refPosition = bodyRoot.position;

            float offset = segment.sineOffset * Mathf.PI * 2f;
            float sine = Mathf.Sin((float)(time * sineAnimMult * sineSpeed - offset));

            if (i > 0)
            {
                referenceDirection = segments[i - 1].direction;
                refPosition = segments[i - 1].transform.position;
            }
            else
            {
                referenceDirection = Quaternion.AngleAxis(sine * sineAngle * sinT, Vector3.up) * referenceDirection;
                Debug.DrawRay(refPosition, referenceDirection * -5f, Color.red);
            }
            if (Vector3.Angle(segment.direction, referenceDirection) > Mathf.Epsilon)
            {
                float angleDiff = Vector3.SignedAngle(segment.direction, referenceDirection, Vector3.up);
                if (Mathf.Abs(angleDiff) > segment.maxAngleDiff)
                {
                    angleDiff = Mathf.Clamp(angleDiff, -segment.maxAngleDiff, segment.maxAngleDiff);
                    segment.direction = Quaternion.AngleAxis(angleDiff, -Vector3.up) * referenceDirection;
                }
                segment.angSpeed += Mathf.Sign(angleDiff) * Mathf.Lerp(0, lookAngleAccel, Mathf.Clamp01(Mathf.Abs(angleDiff)/30f)) * Time.deltaTime;
                segment.angSpeed = Mathf.Clamp(segment.angSpeed, -lookAngleSpeed, lookAngleSpeed);
                
            }
            segment.angSpeed *= friction;
            segment.direction = Quaternion.AngleAxis(segment.angSpeed * Time.deltaTime, Vector3.up) * segment.direction;

            
            Vector3 position = refPosition + segment.direction * -segment.distanceOffset;
            Debug.DrawRay(position, segment.direction * -5f, Color.grey);

            segment.transform.position = position;
            segment.transform.rotation = Quaternion.LookRotation(segment.direction, Vector3.up);
            //float sine = Mathf.Sin(Time.time * sineAnimMult * sineSpeed - (segment.sineOffset * Mathf.PI * 2f));


            /*
            if (i == 0)
            {
                //head
                float offset = segment.sineOffset * Mathf.PI * 2f;
                float sine = Mathf.Sin(Time.time * sineAnimMult * sineSpeed - offset) * sineAmplitude * segment.amplitudeMult;
                float angleSine = Mathf.Sin(Time.time * sineAnimMult * sineSpeed - offset);

                segment.direction = Quaternion.AngleAxis(angleSine * sineAngle * segment.amplitudeMult, Vector3.up) * bodyRoot.forward;

                Vector3 right = GetRight(segment.direction);

                

                Vector3 position = bodyRoot.position + segment.direction * -segment.distanceOffset + right * sine;

                Debug.DrawRay(position + Vector3.up * 1f, Vector3.down * 2f, Color.cyan);
                Debug.DrawRay(position, segment.direction * -5f, Color.grey);

                segment.transform.position = position;
            }
            else
            {
                TailSegment prev = segments[i - 1];
                float angleDiff = Vector3.SignedAngle(segment.direction, prev.direction, Vector3.up);
                if (Mathf.Abs(angleDiff) > segment.maxAngleDiff)
                {
                    angleDiff = Mathf.Clamp(angleDiff, -segment.maxAngleDiff, segment.maxAngleDiff);
                    segment.direction = Quaternion.AngleAxis(angleDiff, -Vector3.up) * prev.direction;
                }
                segment.direction = Vector3.RotateTowards(segment.direction, prev.direction, lookAngleSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);

                Vector3 right = GetRight(segment.direction);
                float offset = segment.sineOffset * Mathf.PI * 2f;
                float sine = Mathf.Sin(Time.time * sineSpeed * sineAnimMult - offset) * sineAmplitude * segment.amplitudeMult;

                Vector3 position = prev.transform.position + segment.direction * -segment.distanceOffset + right * sine;

                Debug.DrawRay(position + Vector3.up * 1f, Vector3.down * 2f, Color.cyan);
                Debug.DrawRay(position, segment.direction * -5f, Color.grey);

                segment.transform.position = position;

            }
            */

            segments[i] = segment;
        }
        lastDirection = bodyRoot.forward;
    }
        
    Vector3 GetRight(Vector3 direction)
    {
        return Vector3.Cross(direction, Vector3.up);
    }
}
