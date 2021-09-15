using System.Collections;
using UnityEngine;

public class AttributeDisplay : MonoBehaviour
{
    protected AttributeValue attribute;
    protected float smoothed;
    public void SetAttribute(AttributeValue a)
    {
        attribute = a;
    }


    public virtual void UpdateGUI()
    {
        return;
    }
    public void SetSmoothValue(float smooth)
    {
        smoothed = smooth;
    }
}