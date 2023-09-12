using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BowStringHandler : MonoBehaviour
{
    public LineRenderer line;
    public bool nocked;
    public Transform mount1;
    public Transform mount2;
    public Transform hand;

    private void LateUpdate()
    {
        if (mount1 == null || mount1 == null) return;
        line.SetPosition(0, mount1.position);
        line.SetPosition(2, mount2.position);

        line.SetPosition(1, (nocked && hand != null) ? hand.position : (mount1.position + mount2.position) * 0.5f);
    }
}
