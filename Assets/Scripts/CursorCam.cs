using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CursorCam : MonoBehaviour
{
    public PlayerActor player;

    InputHandler inputHandler;

    public Vector3 unlockedOffset = new Vector3(4, 4, 4);
    public float verticalRotateSpeed = 180f;
    public float horizontalRotateSpeed = 180f;
    public float eyeHeight = 1f;
    public float playerLookDistance = 10f;
    [ReadOnly] public Vector3 playerLookVector;
    [ReadOnly] public Vector3 cursorPos;

    Vector3 playerEyePos;
    // Use this for initialization
    void Start()
    {
        inputHandler = InputHandler.main;
        playerLookVector = player.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        playerEyePos = player.transform.position + Vector3.up * eyeHeight;
        this.transform.position = playerEyePos +
            player.transform.forward * unlockedOffset.z +
            player.transform.right * unlockedOffset.x +
            player.transform.up * unlockedOffset.y;



        cursorPos = playerEyePos + playerLookVector * playerLookDistance;

        transform.LookAt(cursorPos);
    }
}
