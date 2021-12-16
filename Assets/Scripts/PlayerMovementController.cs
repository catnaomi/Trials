using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController),typeof(PlayerAnimController))]
public class PlayerMovementController : MonoBehaviour
{
    CharacterController cc;

    [SerializeField]
    private Vector2 move;
    [SerializeField]
    private Vector2 look;

    public CameraState camState = CameraState.Free;
    [Header("Movement Settings")]
    public float walkSpeedMax = 5f;
    public AnimationCurve walkSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float sprintSpeed = 10f;
    // Start is called before the first frame update
    void Start()
    {
        cc = this.GetComponent<CharacterController>();        
    }

    // Update is called once per frame
    void Update()
    {
        float speed = walkSpeedCurve.Evaluate(move.magnitude) * walkSpeedMax;
        Vector3 movementDirection = Vector3.zero;
        if (camState == CameraState.Free)
        {

            movementDirection = Camera.main.transform.forward;
            movementDirection.Scale(new Vector3(1f, 0f, 1f));
        }

        cc.Move(movementDirection * speed * Time.deltaTime);
        
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    public enum CameraState
    {
        Free,
        Lock,
        Aim
    }
}
