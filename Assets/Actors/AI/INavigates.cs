using System.Collections;
using UnityEngine;

public interface INavigates
{
    public void SetDestination(Vector3 position);

    public void SetDestination(GameObject target);

    public void ResumeNavigation();
    public void StopNavigation();

    public Vector3 GetDestination();
}