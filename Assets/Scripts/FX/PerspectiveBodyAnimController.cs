using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveBodyAnimController : MonoBehaviour
{
    AnimancerComponent animancer;
    public ClipTransition idle;
    public float heightOffset = 999f;
    public float lookWeight = 1f;
    public float lookBodyWeight = 0f;
    public float lookHeadWeight = 1f;
    public float lookEyesWeight = 0f;
    public float lookClampWeight = 0.5f;
    Vector3 camForwards;
    Vector3 camPosition;
    private void Awake()
    {
        if (heightOffset >= 999f)
        {
            heightOffset = Camera.main.transform.position.y - this.transform.position.y;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        animancer = this.GetComponent<AnimancerComponent>();
        animancer.Play(idle);
        animancer.Layers[0].ApplyAnimatorIK = true;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        
        animancer.Animator.SetLookAtPosition(camPosition + camForwards);
        animancer.Animator.SetLookAtWeight(lookWeight, lookBodyWeight, lookHeadWeight, lookEyesWeight, lookClampWeight);
    }

    private void FixedUpdate()
    {
        this.transform.position = Camera.main.transform.position + Vector3.up * heightOffset;
        camForwards = Camera.main.transform.forward;
        camPosition = Camera.main.transform.position;

        Vector3 viewDirection = Camera.main.transform.forward;
        viewDirection.y = 0f;
        this.transform.rotation = Quaternion.LookRotation(viewDirection.normalized);
    }
    private void OnAnimatorMove()
    {
        if (animancer.Animator.deltaRotation != Quaternion.identity)
        {
            this.transform.rotation = animancer.Animator.rootRotation;
        }
    }
}
