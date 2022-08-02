using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
public class AnimatedDialogueView : DialogueViewBase
{
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
    bool animating;
    int currentIndex;
    Color32[] vertexColors;
    private void Start()
    {
        timingArray = new float[activeTextBox.text.Length];
        //animating = true;
        currentIndex = 0;
        activeTextBox.ForceMeshUpdate();
    }

    private void Update()
    {
        if (animating)
        {
            //activeTextBox.ForceMeshUpdate();
            activeTextBox.maxVisibleCharacters = currentIndex + 1;

            for (int i = 0; i < timingArray.Length; i++)
            {
                
                if (i <= currentIndex)
                {
                    timingArray[i] += Time.deltaTime;
                    if (timingArray[i] > characterInDuration)
                    {
                        timingArray[i] = characterInDuration;
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
        }
        if (!animating)
        {
            animating = true;
        }
    }
}
