using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RigidbodyLauncher : MonoBehaviour
{
    public bool Launch;
    [Space(10)]
    public GameObject prefab;
    public float force;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame)
        {
            Launch = true;
        }
        if (Launch)
        {
            Launch = false;

            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(true);
            obj.transform.position = this.transform.position;
            Rigidbody rigid = obj.GetComponent<Rigidbody>();
            rigid.AddForce(this.transform.forward * force, ForceMode.Impulse);

        }
    }
}
