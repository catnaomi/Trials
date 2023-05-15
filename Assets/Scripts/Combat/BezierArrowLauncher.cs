using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BezierArrowLauncher : MonoBehaviour
{
    public bool Launch;
    [Space(10)]
    public GameObject arrow;
    public float duration;
    public DamageKnockback damageKnockback;
    public Transform[] targetTransforms;
    Vector3[] controlPoints;
    
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

            SetControlPoints();
            Launch = false;
            BezierProjectileController.Launch(
                arrow,
                transform.position,
                duration,
                this.transform,
                this.damageKnockback,
                controlPoints
                ); 
        }
    }

    private void OnDrawGizmos()
    {
        SetControlPoints();
        float distance = 0f;
        Color color1 = Color.blue;
        Color color2 = Color.red;
        for (int i = 1; i < controlPoints.Length; i++)
        {
            distance += Vector3.Distance(controlPoints[i], controlPoints[i - 1]);
        }
        int sections = Mathf.FloorToInt(distance);
        for (int j = 1; j < sections; j++)
        {
            Vector3 point1 = Bezier.GetPoint((float)j / (float)sections, controlPoints);
            Vector3 point2 = Bezier.GetPoint((float)(j-1) / (float)sections, controlPoints);
            Gizmos.color = Color.Lerp(color1, color2, (float)j / (float)sections);
            Gizmos.DrawLine(point1, point2);
        }
    }

    void SetControlPoints()
    {
        if (controlPoints == null || controlPoints.Length != targetTransforms.Length)
        {
            controlPoints = new Vector3[targetTransforms.Length];
        }

        for (int i = 0; i < targetTransforms.Length; i++)
        {
            controlPoints[i] = targetTransforms[i].position;
        }
    }
}
