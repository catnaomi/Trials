using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeWhenRoot : MonoBehaviour
{
    public float delay = 1f;
    public GameObject particle;

    float clock;

    private void Start()
    {
        clock = delay;
    }

    private void Update()
    {
        if (this.transform.root == this.transform)
        {
            clock -= Time.deltaTime;
        }
        if (clock <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (particle != null)
        {
            Instantiate(particle, this.transform.position, Quaternion.identity);
            
        }
        Destroy(this.gameObject);
    }
}
