using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class LineActorPositioningHandler : MonoBehaviour
{
    public GameObject speaker;
    public DialogueRunner dialogueRunner;
    public RectTransform dialogueParent;
    public RectTransform positionL;
    public RectTransform positionR;
    public float speed = 25f;
    public void SetSpeaker(GameObject speak)
    {
        speaker = speak;
    }
    private void OnGUI()
    {
        if (dialogueRunner.IsDialogueRunning)
        {
            if (speaker != null)
            {
                bool isLeft = Camera.main.WorldToViewportPoint(speaker.transform.position).x > 0.5f;
                //dialogueParent.position = Vector3.MoveTowards(dialogueParent.position, isLeft ? positionL.position : positionR.position, speed * Time.deltaTime);
            }
        }
    }
}
