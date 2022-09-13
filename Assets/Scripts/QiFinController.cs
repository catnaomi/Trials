using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimancerComponent))]
public class QiFinController : MonoBehaviour
{
    AnimancerComponent animancer;
    public int layer = 6;
    public Vector2 delta;
    public Vector2 smoothedDelta;
    public float smoothSpeed = 1f;
    public float multiplier = 1f;
    public MixerTransition2DAsset mixer;
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float teleportDistance = 10f;
    MixerState<Vector2> state;
    Vector3 lastPosition;
    bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        animancer = this.GetComponent<AnimancerComponent>();

        animancer.Layers[layer].IsAdditive = true;
        animancer.Layers[layer].Weight = 1f;

        
        lastPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            state = (MixerState<Vector2>)animancer.Layers[layer].Play(mixer);
            initialized = true;
        }
        Vector3 rawDelta = (lastPosition - this.transform.position) * multiplier;
        delta.x = Vector3.Dot(rawDelta, this.transform.right);
        delta.y = Vector3.Dot(rawDelta, this.transform.up);

        if (Vector3.Distance(smoothedDelta, rawDelta) > teleportDistance)
        {
            smoothedDelta = rawDelta;
        }
        smoothedDelta = Vector3.MoveTowards(smoothedDelta, rawDelta, smoothSpeed * Time.deltaTime);

        state.Parameter = smoothedDelta;
        state.Speed = Mathf.Clamp(smoothedDelta.magnitude, minSpeed, maxSpeed);

        
        lastPosition = this.transform.position;
    }
}
