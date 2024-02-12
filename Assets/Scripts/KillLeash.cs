using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillLeash : MonoBehaviour
{
    public float maxRange = 100f;
    [SerializeField, ReadOnly] Vector3 startingPosition;
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = this.transform.position;
        this.StartTimer(1f + Random.value * 0.1f, true, CheckDistance);
    }

    void CheckDistance()
    {
        if (Vector3.Distance(startingPosition, this.transform.position) > maxRange)
        {
            Debug.Log("killleashed: " + this);
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = this.enabled ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(Application.isPlaying ? startingPosition : this.transform.position, maxRange);
        Gizmos.DrawRay(Application.isPlaying ? startingPosition : this.transform.position, this.transform.right * maxRange);
    }
}
