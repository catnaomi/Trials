using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuView : MonoBehaviour
{
    public static MenuView currentlyFocused;
    public bool focusOnStart;
    [SerializeField, ReadOnly] protected bool focused;
    public bool IsFocused {
        get { return focused; }
    }
    public UnityEvent OnFocus;
    public UnityEvent OnUnfocus;
    

    // Start is called before the first frame update
    void Start()
    {
        MenuStart();
        if (focusOnStart)
        {
            Focus();
        }  
    }

    public virtual void MenuStart()
    {
        // use this for initialization
    }
    public virtual void Focus()
    {
        focused = true;
        if (currentlyFocused != null && currentlyFocused != this)
        {
            currentlyFocused.Unfocus();
        }  
        currentlyFocused = this;
        OnFocus.Invoke();
    }

    public virtual void Unfocus()
    {
        focused = false;
        OnUnfocus.Invoke();
    }
}
