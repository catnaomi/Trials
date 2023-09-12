using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpChecker : MonoBehaviour
{
    public float vertical;
    public float horizontal;
    public List<Vector3> jumpPath;
    public int pathInterval = 10;
    Vector3 initialPosition;
    Vector3 lastDirection;
    int frameModulo;
    [SerializeField, ReadOnly] bool jumping;
    [SerializeField, ReadOnly] bool jumpCanEnd;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerActor.player != null)
        {
            PlayerActor.player.OnJumpStart.AddListener(StartJump);
        }
        jumpPath = new List<Vector3>();
    }

    void StartJump()
    {
        jumping = true;
        frameModulo = Time.frameCount % pathInterval;
        initialPosition = PlayerActor.player.transform.position;
        vertical = 0;
        horizontal = 0;
        jumpCanEnd = false;
        jumpPath.Clear();
    }
    // Update is called once per frame
    void Update()
    {
        if (PlayerActor.player == null) return;
        if (jumping)
        {
            Vector3 current = PlayerActor.player.transform.position;
            float y = current.y - initialPosition.y;
            if (y > vertical)
            {
                vertical = y;
            }

            Vector3 xz = current - initialPosition;
            xz.y = 0;
            if (xz.magnitude > horizontal)
            {
                horizontal = xz.magnitude;
            }
            if (xz.magnitude > 0)
            {
                lastDirection = xz.normalized;
            }

            if (Time.frameCount % pathInterval == frameModulo)
            {
                jumpPath.Add(current);
            }

            if (PlayerActor.player.isGrounded)
            {
                if (jumpCanEnd)
                {
                    jumping = false;
                    jumpPath.Add(current);
                }
            }
            else
            {
                jumpCanEnd = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(initialPosition, Vector3.up * vertical);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(initialPosition, lastDirection * horizontal);

        Gizmos.color = Color.green;
        for (int i = 1; i < jumpPath.Count; i++)
        {
            Gizmos.DrawLine(jumpPath[i], jumpPath[i - 1]);
        }
    }
}
