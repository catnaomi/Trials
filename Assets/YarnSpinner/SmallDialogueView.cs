using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;
public class SmallDialogueView : DialogueViewBase
{
    public float characterInDuration = 1f;
    public float nextCharacterDelay = 1f;
    public float whitespaceDelay = 1f;
    public float defaultContinueTime = 5f;
    public Vector2 characterScaleRange = Vector2.one;
    public Vector2 characterAlphaRange = Vector2.one;
    public float uiFadeInTime = 1f;
    [Header("References")]
    public TMP_Text activeTextBox;
    public Image portrait;
    public GameObject progressBarParent;
    public Image progressBar;
    public CanvasGroup group;

    [SerializeField, ReadOnly] float[] timingArray;
    bool showing;
    int currentIndex;
    Color32[] vertexColors;
    bool interrupt;
    bool lineFinished;
    Coroutine FadeRoutine;
    float continueDuration;
    float continueRemaining;
    // Stores a reference to the method to call when the user wants to advance
    // the line.
    Action advanceHandler = null;
    Action naturalFinishCallback;
    private void Start()
    {
        GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>().onDialogueComplete.AddListener(Close);
    }

    private void OnGUI()
    {
        if (showing && group.alpha < 1f)
        {
            if (uiFadeInTime <= 0)
            {
                group.alpha = 1f;
            }
            else
            {
                group.alpha += Mathf.Clamp01(Time.deltaTime / uiFadeInTime);
            }
        }
        else if (!showing && group.alpha > 0)
        {
            if (uiFadeInTime <= 0)
            {
                group.alpha = 0f;
            }
            else
            {
                group.alpha -= Mathf.Clamp01(Time.deltaTime / uiFadeInTime);
            }
        }
    }

    private void Update()
    {
        if (continueDuration > 0)
        {
            if (!progressBarParent.activeSelf)
            {
                progressBarParent.SetActive(true);
            }
            progressBar.fillAmount = Mathf.Clamp01(continueRemaining / continueDuration);
            if (continueRemaining < 0)
            {
                naturalFinishCallback?.Invoke();
            }
            else if (TimeTravelController.time == null || !TimeTravelController.time.IsFreezing())
            {
                continueRemaining -= Time.deltaTime;
            }
        }
        else
        {
            if (progressBarParent.activeSelf)
            {
                progressBarParent.SetActive(false);
            }
        }
    }
    public void Close()
    {
        showing = false;
    }
    // RunLine receives a localized line, and is in charge of displaying it to
    // the user. When the view is done with the line, it should call
    // onDialogueLineFinished.
    //
    // Unless the line gets interrupted, the Dialogue Runner will wait until all
    // views have called their onDialogueLineFinished, before telling them to
    // dismiss the line and proceeding on to the next one. This means that if
    // you want to keep a line on screen for a while, simply don't call
    // onDialogueLineFinished until you're ready.
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        // We shouldn't do anything if we're not active.
        if (gameObject.activeInHierarchy == false)
        {
            // This line view isn't active; it should immediately report that
            // it's finished presenting.
            onDialogueLineFinished();
            return;
        }

        bool isSmall = false;
        string mood = "default";

        string[] meta = dialogueLine.Metadata;

        continueDuration = -1f;
        if (meta?.Length > 0)
        {
            for (int i = 0; i < meta.Length; i++)
            {
                if (meta[i].Contains("mood"))
                {
                    string[] split = meta[i].Split(":");
                    mood = split[1].Trim();
                }
                if (meta[i].Contains("small"))
                {
                    isSmall = true;
                }
                if (meta[i].Contains("duration"))
                {
                    string[] split = meta[i].Split(":");
                    continueDuration = float.Parse(split[1].Trim());
                }
            }
        }
        if (!isSmall)
        {
            showing = false;
            onDialogueLineFinished();
            return;
        }

        Debug.Log($"{this.name} running line {dialogueLine.TextID}");
        
        if (continueDuration < 0)
        {
            continueDuration = defaultContinueTime;
        }
        continueRemaining = continueDuration;

        advanceHandler = requestInterrupt;

        activeTextBox.text = dialogueLine.TextWithoutCharacterName.Text;

        showing = true;

        
        Sprite portraitSprite = CharacterPortraitProvider.GetPortrait(dialogueLine.CharacterName, mood);

        if (portraitSprite != null)
        {
            portrait.sprite = portraitSprite;
            portrait.color = Color.white;
        }
        else
        {
            portrait.sprite = null;
            portrait.color = Color.clear;
        }

        Action OnFinish = () =>
        {
            if (interrupt)
            {
                naturalFinishCallback = null;
                continueDuration = -1f;
                //onDialogueLineFinished();
            }
        };
        StartCoroutine(FadeInText(dialogueLine.TextWithoutCharacterName.Text.Length, OnFinish));
        naturalFinishCallback = onDialogueLineFinished;
    }

    // InterruptLine is called when the dialogue runner indicates that the
    // line's presentation should be interrupted. This is a 'hurry up' signal -
    // the view should finish whatever presentation it needs to do as quickly as
    // possible.
    //
    // In the case of this view, we'll stop the scaling animation, go to full
    // scale, and then report that the presentation is complete.
    public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        if (gameObject.activeInHierarchy == false)
        {
            // This line view isn't active; it should immediately report that
            // it's finished presenting.
            onDialogueLineFinished();
            return;
        }

        // If we get an interrupt, we need to skip to the end of our
        // presentation as quickly as possible, so that we can be ready to
        // dismiss.

        advanceHandler = () =>
        {
            if (lineFinished)
            {
                onDialogueLineFinished();
            }
        };
        
        Debug.Log($"{this.name} was interrupted while presenting {dialogueLine.TextID}");

        // If we're in the middle of an animation, stop it.
        if (!lineFinished)
        {
            interrupt = true;
            continueDuration = -1f;
            onDialogueLineFinished();
        }
        else
        {
            onDialogueLineFinished();
        }
        
    }

    // DismissLine is called when the dialogue runner has instructed us to get
    // rid of the line. This is our view's opportunity to do whatever animations
    // we need to to get rid of the line. When we're done, we call
    // onDismissalComplete. When all line views have called their
    // onDismissalComplete, the dialogue runner moves on to the next line.
    public override void DismissLine(Action onDismissalComplete)
    {
        if (gameObject.activeInHierarchy == false)
        {
            // This line view isn't active; it should immediately report that
            // it's finished dismissing.
            onDismissalComplete();
            return;
        }

        Debug.Log($"{this.name} dismissing line");
        if (FadeRoutine != null)
        {
            StopCoroutine(FadeRoutine);
        }
        interrupt = false;
        activeTextBox.text = "";
        continueDuration = -1f;

        
        onDismissalComplete();
    }


    public override void UserRequestedViewAdvancement()
    {
        // We have received a signal that the user wants to proceed to the next
        // line.

        // Invoke our 'advance line' handler, which (depending on what we're
        // currently doing) will be a signal to interrupt the line, stop the
        // current animation, or do nothing.
        advanceHandler?.Invoke();
    }

    IEnumerator FadeInText(int characterCount, Action finishCallBack)
    {
        lineFinished = false;
        
        int completedIndices = -1;
        int lastRunIndices = -1;
        bool done = false;
        timingArray = new float[characterCount];
        currentIndex = 0;
        activeTextBox.ForceMeshUpdate();
        while (!lineFinished)
        {
            activeTextBox.maxVisibleCharacters = currentIndex + 1;
            for (int i = 0; i < timingArray.Length; i++)
            {

                if (i <= currentIndex)
                {
                    timingArray[i] += Time.deltaTime;
                    if (interrupt)
                    {
                        timingArray[i] = characterInDuration;
                    }
                    if (timingArray[i] >= characterInDuration)
                    {
                        timingArray[i] = characterInDuration;
                        if (completedIndices == i - 1)
                        {
                            completedIndices++;
                            if (completedIndices >= timingArray.Length - 1)
                            {
                                done = true;
                            }
                        }
                    }
                    if (timingArray[i] >= nextCharacterDelay && currentIndex == i)
                    {
                        currentIndex++;
                        if (currentIndex < activeTextBox.textInfo.characterCount && activeTextBox.textInfo.characterInfo[currentIndex].character == ' ')
                        {
                            timingArray[currentIndex] = -whitespaceDelay;
                        }
                    }
                }
                else
                {
                    timingArray[i] = 0f;
                }
                if (done && lastRunIndices == i - 1)
                {
                    lastRunIndices++;
                    if (lastRunIndices >= timingArray.Length - 1)
                    {
                        lineFinished = true;
                    }
                }
                if (!activeTextBox.textInfo.characterInfo[i].isVisible)
                {
                    continue;
                }
                float t = Mathf.Clamp01(timingArray[i] / characterInDuration);
                // Get the index of the material used by the current character.
                int materialIndex = activeTextBox.textInfo.characterInfo[i].materialReferenceIndex;
                // Copy the vertex colors to an array
                vertexColors = activeTextBox.textInfo.meshInfo[materialIndex].colors32;
                // get index of first vertex for character
                int vertexIndex = activeTextBox.textInfo.characterInfo[i].vertexIndex;

                vertexColors[vertexIndex + 0].a = (byte)(255f * Mathf.Lerp(characterAlphaRange.x, characterAlphaRange.y, t));
                vertexColors[vertexIndex + 1].a = (byte)(255f * Mathf.Lerp(characterAlphaRange.x, characterAlphaRange.y, t));
                vertexColors[vertexIndex + 2].a = (byte)(255f * Mathf.Lerp(characterAlphaRange.x, characterAlphaRange.y, t));
                vertexColors[vertexIndex + 3].a = (byte)(255f * Mathf.Lerp(characterAlphaRange.x, characterAlphaRange.y, t));

                //activeTextBox.textInfo.characterInfo[i].scale = Mathf.Lerp(characterScaleRange.x, characterScaleRange.y, t);


                
            }
            activeTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
        interrupt = false;
        finishCallBack();
    }
}
