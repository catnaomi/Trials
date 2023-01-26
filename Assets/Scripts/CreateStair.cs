using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CreateStair : MonoBehaviour
{
    public int stairCount;
    public Vector3 delta;
    [Space(10)]
    public bool go;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (go)
        {
            go = false;
            InstantiateStairs();
        }
    }

    public void InstantiateStairs()
    {
        Debug.Log("Creating " + stairCount + " stair(s).");
        GameObject baseStair = Instantiate(this.gameObject,this.transform.parent);
        DestroyImmediate(baseStair.GetComponent<CreateStair>());
        baseStair.transform.position = this.transform.position;
        baseStair.transform.rotation = this.transform.rotation;
        baseStair.name = "Stair 0";
        for (int i = 1; i < stairCount; i++)
        {
            GameObject newStair = Instantiate(baseStair, this.transform.parent);
            newStair.transform.rotation = this.transform.rotation;
            newStair.transform.localPosition = this.transform.InverseTransformPoint(this.transform.position) + (delta * i);
            newStair.name = "Stair " + i;
        }
        Debug.Log("Stair Creation Complete");
    }
}
