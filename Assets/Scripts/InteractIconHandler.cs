using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractIconHandler : MonoBehaviour
{
    public Transform child;
    public float distanceOffset;

    Interactable currentInteractable;

    void Start()
    {
        if (PlayerActor.player == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        var distance = distanceOffset;
        currentInteractable = PlayerActor.player.highlightedInteractable;
        if (currentInteractable != null && !PlayerActor.player.isMenuOpen)
        {
            child.gameObject.SetActive(true);
            if (currentInteractable.interactIconPositionOverride)
            {
                transform.position = currentInteractable.interactIconPositionOverride.position;
            }
            else
            {
                distance += currentInteractable.interactIconHeight;
                transform.position = currentInteractable.transform.position + Vector3.up * distance;
            }
        }
        else
        {
            child.gameObject.SetActive(false);
        }
    }
}
