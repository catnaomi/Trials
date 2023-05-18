using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimancerComponent))]
public class QiFinController : MonoBehaviour
{
    
    AnimancerComponent animancer;
    Animator animator;
    [ReadOnly]public Vector2 delta;
    [ReadOnly]public Vector2 smoothedDelta;
    [ReadOnly]public float speed;
    [Space(10)]
    public float smoothSpeed = 1f;
    public float multiplier = 1f;
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float teleportDistance = 10f;
    [Header("Animancer")]
    public bool useAnimancer;
    public int layer = 6;
    public MixerTransition2DAsset mixer;
    MixerState<Vector2> state;

    Vector3 lastPosition;
    bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        animancer = this.GetComponent<AnimancerComponent>();

        if (useAnimancer)
        {
            animancer.Layers[layer].IsAdditive = true;
            animancer.Layers[layer].Weight = 1f;
        }
        

        
        lastPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            if (useAnimancer)
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
        speed = Mathf.Clamp(smoothedDelta.magnitude, minSpeed, maxSpeed);
        if (useAnimancer)
        {
            state.Parameter = smoothedDelta;
            state.Speed = speed;
        }
        else
        {
            animator.SetFloat("FinX", smoothedDelta.x);
            animator.SetFloat("FinY", smoothedDelta.y);
            animator.SetFloat("FinSpeed", speed);
        }

        
        lastPosition = this.transform.position;
    }
}
