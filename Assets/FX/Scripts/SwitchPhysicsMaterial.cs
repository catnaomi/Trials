using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwitchPhysicsMaterial : MonoBehaviour
{
    public int index;
    public PhysicMaterial[] materials;
    int lastIndex;
    Collider collider;
    // Start is called before the first frame update
    void Start()
    {
        collider = this.GetComponent<Collider>();
        collider.sharedMaterial = materials[index];
        lastIndex = index;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastIndex != index)
        {
            collider.sharedMaterial = materials[index];
            lastIndex = index;
        }
    }
}
