using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractIconHandler : MonoBehaviour
{
    public Transform child;
    Interactable currentInteractable;
    float distance;
    public float distanceOffset;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerActor.player == null)
        {
            this.enabled = false;
            return;
        }
    }

    private void Update()
    {
        distance = distanceOffset;
        Vector3 center = Vector3.zero;
        currentInteractable = PlayerActor.player.highlightedInteractable;
        if (currentInteractable != null && !PlayerActor.player.isMenuOpen) 
        {
            child.gameObject.SetActive(true);
            distance += currentInteractable.interactIconHeight;
            center = currentInteractable.transform.position;
            this.transform.position = center + Vector3.up * distance;
        }
        else
        {
            child.gameObject.SetActive(false);
        }
    }
}
