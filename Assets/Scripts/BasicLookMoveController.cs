using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicLookMoveController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 look = Gamepad.current.rightStick.ReadValue();
        Vector2 move = Gamepad.current.leftStick.ReadValue();

        this.transform.rotation *= Quaternion.AngleAxis(look.x * rotateSpeed * Time.deltaTime, Vector3.up);
        this.transform.rotation *= Quaternion.AngleAxis(look.y * rotateSpeed * Time.deltaTime, -Vector3.right);
        this.transform.position += (this.transform.forward * move.y + this.transform.right * move.x) * moveSpeed * Time.deltaTime;
        this.transform.rotation = Quaternion.LookRotation(this.transform.forward, Vector3.up);
    }
}
