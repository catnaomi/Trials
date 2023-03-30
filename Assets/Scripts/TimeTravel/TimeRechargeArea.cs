using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRechargeArea : MonoBehaviour
{
    TimeTravelController timeTravelController;
    public float maxDistance = 5f;
    [SerializeField, ReadOnly] bool isPlayerInside;
    // Start is called before the first frame update
    void Start()
    {
        timeTravelController = TimeTravelController.time;
    }

    // Update is called once per frame
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
