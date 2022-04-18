using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public Rigidbody rigidbody;
    public List<TimeTravelData> timeTravelStates;
    public TimeTravelController timeTravelController;
    bool isRewinding;
    bool isFrozen;
    RigidbodyTimeTravelData lastData;

    void Start()
    {
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        if (rigidbody == null)
        {
            rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                this.enabled = false;
                return;
            }
        }
        timeTravelStates = new List<TimeTravelData>();
        timeTravelController.RegisterAffectee(this);
    }
    public GameObject GetObject()
    {
        return this.gameObject;
    }

    public List<TimeTravelData> GetTimeStates()
    {
        return timeTravelStates;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public void LoadTimeState(TimeTravelData data)
    {
        rigidbody.position = data.position;
        rigidbody.rotation = data.rotation;
        if (data is RigidbodyTimeTravelData rigidData)
        {
            if (!isRewinding && !isFrozen)
            {
                ResumeRigidbody(rigidData);
            }
            lastData = rigidData;
        }
    }

    public TimeTravelData SaveTimeState()
    {
        RigidbodyTimeTravelData data = new RigidbodyTimeTravelData()
        {
            position = rigidbody.position,
            rotation = rigidbody.rotation,
            heading = rigidbody.transform.forward,
            time = Time.time,
            velocity = rigidbody.velocity,
            angularVelocity = rigidbody.angularVelocity
        };
        timeTravelStates.Add(data);
        return data;
    }

    public void ResumeRigidbody(RigidbodyTimeTravelData data)
    {
        if (data != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(data.velocity, ForceMode.VelocityChange);
            rigidbody.AddTorque(data.angularVelocity, ForceMode.VelocityChange);
        }
    }
    public void StartFreeze()
    {
        lastData = (RigidbodyTimeTravelData)SaveTimeState();
        rigidbody.isKinematic = true;
        isFrozen = true;
    }

    public void StartRewind()
    {
        isRewinding = true;
        rigidbody.isKinematic = true;
    }

    public void StopFreeze()
    {
        isFrozen = false;
        LoadTimeState(lastData);
    }

    public void StopRewind()
    {
        isRewinding = false;
        LoadTimeState(lastData);
    }

    public bool ShouldApplyTimeVisualEffect()
    {
        return IsFrozen();
    }
}