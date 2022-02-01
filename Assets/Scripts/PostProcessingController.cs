using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingController : MonoBehaviour
{
    public static PostProcessingController instance;
    public Volume MagicVolume;
    static Dictionary<string, Coroutine> coroutineMap;

    private void Awake()
    {
        instance = this;
        coroutineMap = new Dictionary<string, Coroutine>();
    }
    public static void SetVolumeWeight(Volume volume, float weight, float time)
    {
        if (time == 0f)
        {
            volume.weight = weight;
        }
        else
        {
            if (coroutineMap.TryGetValue(volume.name, out Coroutine existingCoroutine) && existingCoroutine != null)
            {
                instance.StopCoroutine(existingCoroutine);
            }
            coroutineMap[volume.name] = instance.StartCoroutine(instance.FadeWeight(volume, weight, time));
        }
    }

    public static void SetVolumeWeight(Volume volume, float weight)
    {
        SetVolumeWeight(volume, weight, 0f);
    }
    IEnumerator FadeWeight(Volume volume, float weight, float time)
    {
        float t = 0f;
        float originalWeight = volume.weight;
        while (volume.weight != weight)
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime / time);
            float newWeight = Mathf.Lerp(originalWeight, weight, t);
            SetVolumeWeight(volume, newWeight);
            yield return null;
        }
    }
}
