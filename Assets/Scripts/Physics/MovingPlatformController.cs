using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MovingPlatformController : MonoBehaviour
{
    public bool attached;
    public Transform platform;
    public Rigidbody rigidbody;
    ParentConstraint constraint;
    Transform pseudoPlayer;

    public Vector3 playerDelta;
    public Vector3 lastPlayerPosition;
    public Vector3 lastPlatformPosition;
    Quaternion lastRotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    RigidbodyTimeTravelHandler timeTravelHandler;
    public void Start()
    {
        /*
        pseudoPlayer = new GameObject().transform;
        pseudoPlayer.SetParent(this.transform);
        pseudoPlayer.transform.localPosition = Vector3.zero;
        pseudoPlayer.transform.localRotation = Quaternion.identity;
        */
        rigidbody = platform.GetComponent<Rigidbody>();
        timeTravelHandler = rigidbody.GetComponent<RigidbodyTimeTravelHandler>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject == PlayerActor.player.gameObject)
        {
            attached = true;
        }
    }

    private void FixedUpdate()
    {
        velocity = rigidbody.velocity;
        angularVelocity = rigidbody.angularVelocity;
        if (attached)
        {
            
            playerDelta = PlayerActor.player.transform.position - lastPlayerPosition;

            Vector3 directionOffset = lastPlayerPosition - lastPlatformPosition;
            Debug.DrawRay(platform.position, directionOffset, Color.red);
            Debug.DrawRay(platform.position, Quaternion.Euler(rigidbody.angularVelocity) * directionOffset, Color.blue);
            directionOffset = (Quaternion.Euler(rigidbody.angularVelocity) * directionOffset) - (lastPlayerPosition - lastPlatformPosition);
            CharacterController cc = PlayerActor.player.GetComponent<CharacterController>();
            cc.Move(directionOffset + rigidbody.velocity * Time.fixedDeltaTime);
            PlayerActor.player.transform.rotation *= Quaternion.Euler(0, rigidbody.angularVelocity.y, 0);
        }
        lastPlayerPosition = PlayerActor.player.transform.position;
        lastPlatformPosition = rigidbody.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject == PlayerActor.player.gameObject)
        {
            attached = false;
        }
    }
}
