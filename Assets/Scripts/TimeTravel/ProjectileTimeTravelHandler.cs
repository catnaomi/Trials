using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
public class ProjectileTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public Rigidbody[] rigidbodies;
    public List<TimeTravelData> timeTravelStates;
    public TimeTravelController timeTravelController;
    bool isRewinding;
    bool isFrozen;
    bool frozenOnCreate;
    Projectile projectile;
    ProjectileTimeTravelData lastData;

    void Start()
    {
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        projectile = this.GetComponent<Projectile>();
        timeTravelStates = new List<TimeTravelData>();
        timeTravelController.RegisterAffectee(this);
        if (timeTravelController.IsFreezing() && timeTravelController.globalFreeze)
        {
            frozenOnCreate = true;
            projectile.enabled = false;
        }
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
        if (this == null) return;
        this.transform.position = data.position;
        this.transform.rotation = data.rotation;
        if (data is ProjectileTimeTravelData pdata)
        {
            lastData = pdata;
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                Rigidbody rigidbody = rigidbodies[i];
                rigidbody.isKinematic = true;
            }
        }
    }

    public TimeTravelData SaveTimeState()
    {
        ProjectileTimeTravelData data = new ProjectileTimeTravelData()
        {
            position = this.transform.position,
            rotation = this.transform.rotation,
            heading = this.transform.forward,
            time = Time.time,
            origin = projectile.origin,
            inFlight = projectile.inFlight,
            parent = projectile.transform.parent,
        };
        RigidbodyTimeTravelData[] datas = new RigidbodyTimeTravelData[rigidbodies.Length];
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody rigidbody = rigidbodies[i];
            RigidbodyTimeTravelData rdata = new RigidbodyTimeTravelData()
            {
                position = rigidbody.position,
                rotation = rigidbody.rotation,
                heading = rigidbody.transform.forward,
                time = Time.time,
                velocity = rigidbody.velocity,
                angularVelocity = rigidbody.angularVelocity,
            };
            datas[i] = rdata;
        }
        data.rigidbodyDatas = datas;
        timeTravelStates.Add(data);
        return data;
    }

    public void StartFreeze()
    {
        isFrozen = true;
        //lastData = (ProjectileTimeTravelData)SaveTimeState();

    }

    public void StartRewind()
    {
        isRewinding = true;
    }

    public void StopFreeze()
    {
        isFrozen = false;
        LoadTimeState(lastData);
        if (lastData.inFlight || frozenOnCreate) LaunchNew();
    }

    public void StopRewind()
    {
        isRewinding = false;
        LoadTimeState(lastData);
        if (lastData.inFlight) LaunchNew();
    }

    void ResumeLaunch()
    {
        if (lastData == null) return;
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody rigidbody = rigidbodies[i];

            RigidbodyTimeTravelData rdata = lastData.rigidbodyDatas[i];
            //rigidbody.position = rdata.position;
            //rigidbody.rotation = rdata.rotation;
            rigidbody.isKinematic = false;
            rigidbody.AddForce(rdata.velocity, ForceMode.VelocityChange);
            rigidbody.AddTorque(rdata.angularVelocity, ForceMode.VelocityChange);
        }
    }
    void LaunchNew()
    {
        Vector3 velocity = Vector3.zero;
        if (frozenOnCreate && projectile is ArrowController arrow1)
        {
            velocity = arrow1.initForce;
        }
        else if (lastData == null)
        {
            return;
        }
        else if (lastData.rigidbodyDatas.Length > 0)
        {
            velocity = lastData.rigidbodyDatas[0].velocity;
        }
        if (projectile is ArrowController arrow)
        {
            ArrowController.Launch(arrow.prefabRef, lastData.position, lastData.rotation, velocity, lastData.origin.transform, arrow.damageKnockback);
        }
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        timeTravelController.DeregisterAffectee(this);
    }

    public bool ShouldApplyTimeVisualEffect()
    {
        return IsFrozen();
    }

    public void ClearTimeData()
    {

        if (IsFrozen())
        {
            StopFreeze();
        }
        else if (isRewinding)
        {
            StopRewind();
        }
        timeTravelStates.Clear();
    }
}