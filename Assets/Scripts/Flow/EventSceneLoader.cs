using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSceneLoader : MonoBehaviour
{
    public void LoadScene(string toLoad)
    {
        SceneLoader.LoadSceneAdditively(toLoad);
    }

    public void UnloadScene(string toUnload)
    {
        SceneLoader.EnsureScenesAreUnloaded(toUnload);
    }

    public void SetPrimary(string primary)
    {
        if (SceneLoader.IsSceneLoaded(primary))
        {
            SceneLoader.SetActiveScene(primary);
        }
        else
        {
            StartCoroutine(SetPrimaryRoutine(primary));
        }
    }

    IEnumerator SetPrimaryRoutine(string primary)
    {
        yield return new WaitUntil(() => { return SceneLoader.IsSceneLoaded(primary); });
        StartCoroutine(SetPrimaryRoutine(primary));
    }
}
