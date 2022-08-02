using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
public class AnimatedDialogueView : DialogueViewBase
{
    public bool waitForInput = true;
    public float characterInDuration = 1f;
    public float nextCharacterDelay = 1f;
    public float whitespaceDelay = 1f;
    public Vector2 characterScaleRange = Vector2.one;
    public Vector2 characterAlphaRange = Vector2.one;
    [Header("References")]
    public RectTransform textContainersParent;
    public RectTransform activeTextContainer;
    public TMP_Text activeTextBox;
    public TMP_Text characterNameText;
    public Image portrait;
    public Button continueButton;

    [SerializeField, ReadOnly] float[] timingArray;
    int currentIndex;
    Color32[] vertexColors;
    bool interrupt;
    bool lineFinished;
    private void Start()
    {
        
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

        Debug.Log($"{this.name} running line {dialogueLine.TextID}");

        activeTextBox.text = dialogueLine.TextWithoutCharacterName.Text;

        characterNameText.text = dialogueLine.CharacterName;

        Action OnFinish = () =>
        {
            if (waitForInput)
            {
                // do nothing
            }
            else
            {
                onDialogueLineFinished();
            }
        };
        StartCoroutine(FadeInText(dialogueLine.TextWithoutCharacterName.Text.Length, OnFinish));
    }

    // InterruptLine is called when the dialogue runner indicates that the
    // line's presentation should be interrupted. This is a 'hurry up' signal -
    // the view should finish whatever presentation it needs to do as quickly as
    // possible.
    //
    // In the case of this view, we'll stop the scaling animation, go to full
    // scale, and then report that the presentation is complete.
    public void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
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

        Debug.Log($"{this.name} was interrupted while presenting {dialogueLine.TextID}");

        // If we're in the middle of an animation, stop it.
        interrupt = true;

        // Indicate that we've finished presenting the line.
        onDialogueLineFinished();
    }


    IEnumerator FadeInText(int characterCount, Action finishCallBack)
    {
        lineFinished = false;

        int completedIndices = -1;

        timingArray = new float[characterCount];
        currentIndex = 0;
        activeTextBox.ForceMeshUpdate();
        while (!lineFinished)
        {
            activeTextBox.maxVisibleCharacters = currentIndex + 1;
            for (int i = completedIndices + 1; i < timingArray.Length; i++)
            {

                if (i <= currentIndex)
                {
                    timingArray[i] += Time.deltaTime;
                    if (interrupt || timingArray[i] >= characterInDuration)
                    {
                        timingArray[i] = characterInDuration;
                        if (completedIndices == i - 1)
                        {
                            completedIndices++;
                            if (completedIndices >= timingArray.Length)
                            {
                                lineFinished = true;
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

                if (t > 0 && t < 1) Debug.Log((byte)(255f * Mathf.Lerp(characterAlphaRange.x, characterAlphaRange.y, t)));
                //activeTextBox.textInfo.characterInfo[i].scale = Mathf.Lerp(characterScaleRange.x, characterScaleRange.y, t);



            }
            activeTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
        interrupt = false;
        finishCallBack();
    }
}
