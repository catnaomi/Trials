using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamageablePoint))]
public class IceGiantDamagePointWeakness : MonoBehaviour
{
    DamageablePoint point;
    public DamageResistance resistance1;
    public DamageResistance resistance2;
    public GameObject indicator1;
    public GameObject indicator2;

    private void Awake()
    {
        indicator1.SetActive(false);
        indicator2.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        point = GetComponent<DamageablePoint>();
        
        ApplyResistances(resistance1, indicator1);
    }

    public void UpdateResistances()
    {
        DamageResistance resistance = null;
        GameObject indicator = null;
        if (Random.value > 0.5f)
        {
            resistance = resistance1;
            indicator = indicator1;
        }
        else
        {
            resistance = resistance2;
            indicator = indicator2;
        }

        ApplyResistances(resistance, indicator);
        
    }

    public void ApplyResistances(DamageResistance resistance, GameObject indicator)
    {
        point.resistance = resistance;
        GameObject newIndicator = Instantiate(indicator, indicator.transform.parent);
        newIndicator.SetActive(true);
        FreeRigidbodyEvent free = newIndicator.GetComponentInChildren<FreeRigidbodyEvent>();
        point.OnDie.AddListener(free.FreeRigidbody);
    }

    public void Reform()
    {
        UpdateResistances();
    }
}
