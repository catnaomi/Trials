using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckDistanceFromPlayerTrigger : MonoBehaviour
{
    public float distance = 5f;
    public Transform target;
    public UnityEvent OnSuccess;

    public void CheckDistance()
    {
        if (PlayerActor.player != null && target != null && Vector3.Distance(target.position, PlayerActor.player.transform.position) < distance)
        {
            OnSuccess.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(target.position, distance);
    }
}
