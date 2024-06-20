using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransferHallway : MonoBehaviour
{
    public string thisScene;
    public string targetScene;

    public string identifierLetter = "A";
    [HideInInspector] public Transform _center;

    SceneTransferHallway other;

    public static bool IsTransferInProgress;

    public UnityEvent OnTransferInto;
    public UnityEvent OnTransferFrom;
    // Start is called before the first frame update
    void Start()
    {
        GetCenter();
    }


    void GetCenter()
    {
        _center = transform.Find("_center");
    }
    public void OnLoadTrigger()
    {
        SceneLoader.AllowSceneActivation(true);
        SceneLoader.ShouldReloadScenes(false);
        SceneLoader.ShouldLoadInitScene(false);
        SceneLoader.LoadScenes(thisScene, targetScene);
    }

    // return true on success
    bool GetOtherSide()
    {
        string format = "_to_{0}_{1}";
        GameObject obj = GameObject.Find(string.Format(format, thisScene, identifierLetter));
        if (obj == null)
        {
            Debug.LogWarning("Other Side of Hallway not found. Is the other scene loaded and its entrance labelled properly?");
            return false;
        }
        other = obj.GetComponent<SceneTransferHallway>();
        if (other == null)
        {
            Debug.LogWarning("Other Side of Hallway lacking script.");
            return false;
        }
        return true;
    }

    public void StartTransfer()
    {
        if (!PlayerActor.player.IsAlive() || TimeTravelController.time.IsRewinding()) return;
        if (!IsTransferInProgress)
        {
            IsTransferInProgress = true;
            StartCoroutine(TransferRoutine());
        }

    }

    IEnumerator TransferRoutine()
    {
        IsTransferInProgress = true;

        if (_center == null)
        {
            GetCenter();
        }
        Debug.Assert(_center != null);
        bool loaded = false;

        do
        {
            loaded = SceneManager.GetSceneByName(targetScene).isLoaded;
            if (!loaded) yield return new WaitForSecondsRealtime(0.5f);
        } while (!loaded);

        bool otherSideFound = false;

        do
        {
            otherSideFound = GetOtherSide();
            if (!otherSideFound) yield return new WaitForSecondsRealtime(0.1f);
        } while (!otherSideFound);

        if (other._center == null)
        {
            other.GetCenter();
        }
        Vector3 offset = PlayerActor.player.transform.position - _center.position;

        PlayerActor.player.WarpTo(other._center.position + offset);

        Vector3 transferVector = other._center.position - _center.position;

        CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
        brain.ActiveVirtualCamera.OnTargetObjectWarped(brain.ActiveVirtualCamera.Follow, transferVector);

        IsTransferInProgress = false;


        OnTransferFrom.Invoke();
        other.OnTransferInto.Invoke();

        int attempts = 0;
        do
        {
            SceneLoader.SetActiveScene(targetScene);
            attempts++;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        while (SceneManager.GetActiveScene().name != targetScene);

        Debug.Log($"Transfered to {targetScene} after {attempts} attempts.");
    }
}
