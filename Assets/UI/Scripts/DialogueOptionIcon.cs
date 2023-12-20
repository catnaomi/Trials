using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionIcon : MonoBehaviour
{
    [SerializeField, ReadOnly] State state;
    public Image[] icons;
    public enum State
    {
        NotSelected,
        Selected,
        Cancellable
    }

    private void OnEnable()
    {
        state = State.NotSelected;
        UpdateIcons();
    }
    public void SetState(State newState)
    {
        this.state = newState;
        UpdateIcons();
    }

    void UpdateIcons()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].enabled = (i == (int)state);
        }
    }
}
