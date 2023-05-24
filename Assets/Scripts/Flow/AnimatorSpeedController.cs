using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSpeedController : MonoBehaviour
{
    Animator animator;
    public float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.speed != speed)
        {
            animator.speed = speed;
        }
    }
}
