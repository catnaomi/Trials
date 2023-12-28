using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleOnDestroy : MonoBehaviour
{
    public GameObject particlePrefab;
    public bool forceX;
    public bool forceY;
    public bool forceZ;
    public Vector3 forcePosition;
    
    void OnDestroy()
    {
        Vector3 pos = this.transform.position;
        if (forceX) pos.x = forcePosition.x;
        if (forceY) pos.y = forcePosition.y;
        if (forceZ) pos.z = forcePosition.z;

        GameObject particle = Instantiate(particlePrefab);
        particle.transform.position = pos;

        particle.GetComponent<ParticleSystem>().Play();
    }
}
