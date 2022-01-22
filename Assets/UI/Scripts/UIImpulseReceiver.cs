using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIImpulseReceiver : MonoBehaviour
{
    UIImpulseController controller;
    Vector3 initialPosition;
    public float multiplier = 1000f;
    public float moveSpeed = 0.01f;
    public bool moving;
    public float time = 1f;
    float clock;
    [Header("Actor Settings")]
    public bool onlyOnActorHurt;
    public bool isPlayer;
    public Actor actor;

    void Start()
    {
        controller = UIImpulseController.impulse;
        initialPosition = this.transform.localPosition;
        if (isPlayer)
        {
            actor = PlayerActor.player;
        }
        if (actor == null)
        {
            Debug.Log("Disabling UI Impuse Receiver: No Actors.");
            this.enabled = false;
            return;
        }
        if (onlyOnActorHurt)
        {
            moving = false;
            actor.OnHurt.AddListener(StartMoving);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            this.transform.localPosition += controller.delta * multiplier;
            
            if (onlyOnActorHurt)
            {
                if (clock < 0f)
                {
                    moving = false;
                }
                else
                {
                    clock -= Time.deltaTime;
                }
            }
        }
        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, initialPosition, moveSpeed * Time.deltaTime);
    }

    public void StartMoving()
    {
        moving = true;
        clock = time;
    }
}
