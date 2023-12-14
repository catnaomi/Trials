using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseController
{
    static float lastTimescale;
    public static void Pause()
    {
        lastTimescale = Time.timeScale;
        Time.timeScale = 0;
    }

    public static void Unpause()
    {
        Time.timeScale = lastTimescale;
    }
}
