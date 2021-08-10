using System.Collections;
using UnityEngine;

public class StatElementsDisplay : MonoBehaviour
{
    public GameObject elementPrefab;
    public DamageType[] elements;

    public void SetElements(DamageType[] elements)
    {
        this.elements = elements;
    }
    public void GenerateElements()
    {
        foreach(Transform transform in this.transform)
        {
            GameObject.Destroy(transform.gameObject);
        }
        foreach(DamageType element in elements)
        {
            GameObject obj = GameObject.Instantiate(elementPrefab, this.transform);
            StatElement stat = obj.GetComponent<StatElement>();
            stat.type = element;
            stat.SetSprite();
        }
    }
}