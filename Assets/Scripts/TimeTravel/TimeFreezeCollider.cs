using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFreezeCollider : MonoBehaviour
{
    public bool inspectorFreeze;
    [ReadOnly]public bool freeze;
    // Start is called before the first frame update
    void Update()
    {
       if (inspectorFreeze && !freeze)
        {
            freeze = true;
            TimeTravelController.time.StartFreeze();
        } 
       else if (!inspectorFreeze && freeze)
        {
            freeze = false;
            TimeTravelController.time.freeze = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IAffectedByTimeTravel>(out IAffectedByTimeTravel affected) && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            TimeTravelController.time.AddFrozen(affected);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IAffectedByTimeTravel>(out IAffectedByTimeTravel affected) && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            TimeTravelController.time.RemoveFrozen(affected);
        }
    }
}
