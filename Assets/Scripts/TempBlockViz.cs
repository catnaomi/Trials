using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempBlockViz : MonoBehaviour
{
    public Image blocking;
    public Image cross;
    public Image circle;
    public Image none;
    private void OnGUI()
    {
        if (PlayerActor.player != null)
        {
            bool block = false;
            bool slash = false;
            bool thrust = false;
            if (PlayerActor.player.IsBlocking())
            {
                block = true;
            }
            if (PlayerActor.player.IsBlockingSlash())
            {
                block = false;
                slash = true;
            }
            if (PlayerActor.player.IsBlockingThrust())
            {
                block = false;
                thrust = true;
            }
            blocking.enabled = block;
            cross.enabled = slash;
            circle.enabled = thrust;
            none.enabled = !block && !slash && !thrust;
        }    
    }
}
