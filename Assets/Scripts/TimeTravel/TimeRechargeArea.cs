using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRechargeArea : MonoBehaviour
{
    TimeTravelController timeTravelController;
    public float maxDistance = 5f;
    [SerializeField, ReadOnly] bool isPlayerInside;

    void Start()
    {
        timeTravelController = TimeTravelController.time;
    }

    void Update()
    {
        if (isPlayerInside)
        {
            if (Vector3.Distance(PlayerActor.player.transform.position, this.transform.position) > maxDistance) {
                SetPlayerInside(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            SetPlayerInside(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            SetPlayerInside(false);
        }
    }

    public void  SetPlayerInside(bool inside)
    {
        isPlayerInside = inside;
        TimeTravelController.time.ToggleQuickRecharge(isPlayerInside);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, maxDistance);
    }
}
