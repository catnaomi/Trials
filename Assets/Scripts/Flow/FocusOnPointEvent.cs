using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOnPointEvent : MonoBehaviour
{
    public Transform point;

    public void Focus()
    {
        FocusCamera.FocusOnTransform(point);
    }
}
