using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwordAmbushYarnConditions : MonoBehaviour
{
    bool didHit;
    bool didTimestop;

    public string hitNode = "ashanti_hit";
    public string missNode = "ashanti_missnormal";
    public string timeHitNode = "ashanti_timemiss";
    public string timeMissNode = "ashanti_timehit";

    public YarnPlayer target;
    public void Hit()
    {
        didHit = true; 
    }

    public void TimeStop()
    {
        didTimestop = true;
    }

    public void Pass()
    {
        if (didTimestop && didHit)
        {
            target.nodes = new string[] { timeHitNode };
        }
        else if (didTimestop && !didHit)
        {
            target.nodes = new string[] { timeMissNode };
        }
        else if (!didTimestop && didHit)
        {
            target.nodes = new string[] { hitNode };
        }
        else if (!didTimestop && !didHit)
        {
            target.nodes = new string[] { missNode };
        }
        target.Play();
    }
}
