using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtagTalkboxHandler : MonoBehaviour
{
    Animator animator;
    public Transform targetPoint;
    public Canvas parentCanvas;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool("HasDialogue", ProtagDialogueController.HasDialogue());
    }
    // Update is called once per frame
    private void OnGUI()
    {
        Vector2 viewPointPos = Camera.main.WorldToViewportPoint(targetPoint.position);
        float width = ((RectTransform)parentCanvas.transform).rect.width;
        float height = ((RectTransform)parentCanvas.transform).rect.height;
        Vector2 screenPos = new Vector2(viewPointPos.x * width, viewPointPos.y * height);
        ((RectTransform)this.transform).anchoredPosition = screenPos;
    }
}
