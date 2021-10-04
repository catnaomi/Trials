using System.Collections;
using UnityEngine;

public interface IGeneratesModel
{
    public GameObject GenerateModel();
    public GameObject GetModel();
    public void DestroyModel();
}