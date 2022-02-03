using System.Collections;
using UnityEngine;

public class ProjectileTimeTravelData : TimeTravelData
{
    public RigidbodyTimeTravelData[] rigidbodyDatas;
    public bool hitboxActive;
    public bool inFlight;
    public Transform parent;
    public GameObject origin;
}