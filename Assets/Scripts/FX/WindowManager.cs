using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// http://answers.unity.com/answers/1582777/view.html
// by alvmoral
public class WindowManager : MonoBehaviour
{

    public delegate void ScreenSizeChangeEventHandler(int Width, int Height);       //  Define a delgate for the event
    public event ScreenSizeChangeEventHandler ScreenSizeChangeEvent;                //  Define the Event
    public event ScreenSizeChangeEventHandler ScreenSizeChangeEventDelayed;                //  Define the Event
    public float delayTime = 1f;
    float clock;
    protected virtual void OnScreenSizeChange(int Width, int Height)
    {              //  Define Function trigger and protect the event for not null;
        if (ScreenSizeChangeEvent != null) ScreenSizeChangeEvent(Width, Height);
    }

    // delayed event to not update constantly while window is still being recharged
    protected virtual void OnScreenSizeChangeDelayed(int Width, int Height)
    {              //  Define Function trigger and protect the event for not null;
        if (ScreenSizeChangeEventDelayed != null) ScreenSizeChangeEventDelayed(Width, Height);
    }

    private Vector2 lastScreenSize;
    public static WindowManager instance = null;                                    //  Singleton for call just one instance

    void Awake()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        instance = this;                                                            // Singleton instance
        clock = delayTime + 1f;
    }

    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (this.lastScreenSize != screenSize) {
            this.lastScreenSize = screenSize;
            OnScreenSizeChange(Screen.width, Screen.height);                        //  Launch the event when the screen size change
            clock = 0f;
        }
        if (clock < delayTime)
        {
            clock += Time.deltaTime;
            if (clock >= delayTime)
            {
                OnScreenSizeChangeDelayed(Screen.width, Screen.height);
                Debug.Log("delayed screen size changed!");
            }
        }
    }

}
