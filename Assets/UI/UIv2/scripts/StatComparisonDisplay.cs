using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatComparisonDisplay : MonoBehaviour
{
    public TMP_Text displayName;
    public TMP_Text displayValue;
    public TMP_Text displayChange;
    [Space(5)]
    public float statValue;
    public bool compare;
    public float comparisonValue;
    [Space(5)]
    public string stringFormat;
    [Space(5)]
    public Color positiveColor;
    public Color negativeColor;
    public Color neutralColor;
    [Space(5)]
    public ColorComparisonType comparisonType;
    public enum ColorComparisonType
    {
        HigherPreferred,
        LowerPreferred,
        CloseToZeroPreferred,
    }
    public void UpdateDisplay()
    {
        displayValue.text = statValue.ToString(stringFormat);

        if (compare) {
            float comparisonResult = comparisonValue - statValue;
            if (comparisonResult > 0)
            {
                displayChange.text = "+";
                if (comparisonType == ColorComparisonType.HigherPreferred)
                {
                    displayChange.color = positiveColor;
                }
                else if (comparisonType == ColorComparisonType.LowerPreferred)
                {
                    displayChange.color = negativeColor;
                }
            }
            else if (comparisonResult < 0)
            {
                displayChange.text = "";
                if (comparisonType == ColorComparisonType.HigherPreferred)
                {
                    displayChange.color = negativeColor;
                }
                else if (comparisonType == ColorComparisonType.LowerPreferred)
                {
                    displayChange.color = positiveColor;
                }
            }
            else {
                displayChange.text = "±";
                displayChange.color = neutralColor;
            }
            if (comparisonType == ColorComparisonType.CloseToZeroPreferred)
            {
                float comparisonAbs = Mathf.Abs(comparisonValue);
                float statAbs = Mathf.Abs(statValue);

                if (comparisonAbs < statAbs)
                {
                    displayChange.color = positiveColor;
                }
                else if (comparisonAbs > statAbs)
                {
                    displayChange.color = negativeColor;
                }
                else
                {
                    displayChange.color = neutralColor;
                }
            }
            displayChange.text += comparisonResult.ToString(stringFormat);
            displayChange.text = "-> " + comparisonValue.ToString(stringFormat);
        }
        else
        {
            displayChange.text = "";
        }
    }
}
