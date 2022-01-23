using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class LineActorPositioningHandler : MonoBehaviour
{
    public GameObject speaker;
    public Transform dialogueMount;
    public DialogueRunner dialogueRunner;
    public RectTransform dialogueParent;
    public Transform lineMount;
    public LineRenderer lineRenderer;
    public float speed = 25f;
    public float width = 728f;
    public float height = 300f;
    public Vector2 linePointOffset = new Vector2(10f, 10f);
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, -1f, 1f, 1f);
    float t;
    float teval;
    bool isLeft;
    Vector3 linePointReal;
    float offset;
    Vector3 position;
    private void Start()
    {
        width = dialogueParent.rect.width;
        height = dialogueParent.rect.height;
    }
    public void SetSpeaker(GameObject speak, Transform mount)
    {
        speaker = speak;
        dialogueMount = mount;
        offset = 0f;
        linePointReal = linePointOffset;
        float x = Camera.main.WorldToViewportPoint(speaker.transform.position).x;
        isLeft = x > 0.5f;
        t = isLeft ? 0f : 1f;
    }
    private void OnGUI()
    {
        if (dialogueRunner.IsDialogueRunning)
        {
            if (speaker != null)
            {
                float x = Camera.main.WorldToViewportPoint(speaker.transform.position).x;
                if (!isLeft && x > 0.6f)
                {
                    isLeft = true;
                }
                else if (isLeft && x < 0.4f)
                {
                    isLeft = false;
                }
                t = Mathf.MoveTowards(t, isLeft ? 0f : 1f, speed * Time.deltaTime);
                teval = curve.Evaluate(t);
                //dialogueParent.position = Vector3.MoveTowards(dialogueParent.position, isLeft ? positionL.position : positionR.position, speed * Time.deltaTime);
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(dialogueMount.position);
                Rect prect = transform.parent.GetComponent<RectTransform>().rect;
                offset = width / 2f * teval;//, speed * Time.deltaTime);
                position = new Vector3(prect.width * screenPoint.x + offset, prect.height * screenPoint.y);
                Vector3 positionNoOffset = new Vector3(prect.width * screenPoint.x, prect.height * screenPoint.y);// new Vector3(screenPoint.x + offset, screenPoint.y, 0f);
                //position.x = Mathf.Clamp(position.x, width / 2f, Screen.width - (width / 2f));
                //position.y = Mathf.Clamp(position.y, height / 2f, Screen.height - (height / 2f));
                dialogueParent.position = transform.parent.TransformPoint(position);
                //lineRenderer.SetPosition(0, new Vector3(position.x, position.y, 0f));// - ((RectTransform)lineMount).rect.center);
                linePointReal = positionNoOffset + new Vector3(linePointOffset.x * teval, linePointOffset.y, 0f);
                lineRenderer.SetPosition(0, new Vector2(position.x,position.y + height * 0.75f));
                lineRenderer.SetPosition(1, linePointReal);// - ((RectTransform)lineMount).rect.center);
            }
        }
    }

    private void Update()
    {
        if (dialogueRunner.IsDialogueRunning)
        {
            if (speaker != null && dialogueMount != null && lineRenderer != null)
            {
                //lineRenderer.SetPosition(0, lineMount.transform.position);
                //lineRenderer.SetPosition(1, dialogueMount.position);
            }
        }
    }
}
