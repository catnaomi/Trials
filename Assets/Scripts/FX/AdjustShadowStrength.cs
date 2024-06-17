using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class AdjustShadowStrength : MonoBehaviour
{
    [Header("Realtime Shadow Strengths")]
    public float strength1 = 1f;
    public float strength2 = 1f;

    [Header("Transitions")]
    public float transitionTime = 0f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [ReadOnly, SerializeField] bool isTransitioning;

    [Header("Debugging")]
    public bool inspectorSwap;
    [ReadOnly, SerializeField] bool usingStrength2 = false;

    Light light;
    Coroutine transition;
    // Start is called before the first frame update
    void Start()
    {
        light = this.GetComponent<Light>();
        if (light == null)
        {
            Debug.LogError("No light component found on object " + this.gameObject.name, this);
            this.enabled = false;
            return;
        }
        if (PortalManager.instance != null)
        {
            PortalManager.instance.OnSwap.AddListener(OnPortalSwap);
            usingStrength2 = PortalManager.instance.inWorld2;
        }
        SwapTo(usingStrength2);
    }

    // Update is called once per frame
    void Update()
    {
        if (inspectorSwap)
        {
            inspectorSwap = false;
            SwapTo(!usingStrength2, true);
        }
    }

    private void OnPortalSwap()
    {
        if (PortalManager.instance != null)
        {
            SwapTo(PortalManager.instance.inWorld2, true);
        }
    }

    public void SwapTo(bool s2, bool shouldTransition = false)
    {
        if (shouldTransition == false  || transitionTime <= 0)
        {
            light.shadowStrength = s2 ? strength2 : strength1;
            usingStrength2 = s2;
            return;
        }
        else
        {
            if (isTransitioning && transition != null)
            {
                StopCoroutine(transition);
            }
            transition = StartCoroutine(TransitionTo(s2));
        }
    }

    IEnumerator TransitionTo(bool s2)
    {
        isTransitioning = true;
        float t = 0;
        float start = light.shadowStrength;
        float end = s2 ? strength2 : strength1;
        while (t < transitionTime)
        {
            light.shadowStrength = Mathf.Lerp(start, end, transitionCurve.Evaluate(t / transitionTime));
            t += Time.deltaTime;
            yield return null;
        }
        light.shadowStrength = end;
        usingStrength2 = s2;
        isTransitioning = false;
    }
}
