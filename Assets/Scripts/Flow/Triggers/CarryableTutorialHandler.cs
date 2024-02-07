using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryableTutorialHandler : MonoBehaviour
{
    public Carryable carryable;
    public Interactable interact;
    public string pickupPrompt;
    public UnityEngine.InputSystem.InputActionReference pickupInput;
    public string throwPrompt;
    public UnityEngine.InputSystem.InputActionReference throwInput;
    public string putdownPrompt;
    public UnityEngine.InputSystem.InputActionReference putdownInput;
    [ReadOnly] public int carryState = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (carryable == null) this.enabled = false;
        carryable.OnThrow.AddListener(OnThrow);
    }

    enum CarryState
    {
        NotInRange,
        InRange,
        Carried,
        Thrown,
    }
    // Update is called once per frame
    void Update()
    {
        if (carryState == (int)CarryState.NotInRange)
        {
            if (ShouldShowInteract())
            {
                ShowTutorial(pickupPrompt, pickupInput);
                carryState = (int)CarryState.InRange;
            }
        }
        else if (carryState == (int)CarryState.InRange)
        {
            if (carryable.isBeingCarried)
            {
                TutorialHandler.HideTutorialStatic(pickupPrompt);
                ShowTutorial(throwPrompt, throwInput);
                ShowTutorial(putdownPrompt, putdownInput);
                carryState = (int)CarryState.Carried;
            }
            else if (!ShouldShowInteract())
            {
                carryState = (int)CarryState.NotInRange;
                TutorialHandler.HideTutorialStatic(pickupPrompt);
            }
        }
        else if (carryState == (int)CarryState.Carried)
        {
            if (!carryable.isBeingCarried)
            {
                TutorialHandler.HideTutorialStatic(pickupPrompt);
                TutorialHandler.HideTutorialStatic(throwPrompt);
                carryState = (int)CarryState.NotInRange;
            }
        }
    }

    void HideAll()
    {
        TutorialHandler.HideTutorialStatic(pickupPrompt);
        TutorialHandler.HideTutorialStatic(throwPrompt);
        TutorialHandler.HideTutorialStatic(putdownPrompt);
    }

    void ShowTutorial(string text, UnityEngine.InputSystem.InputActionReference input)
    {
        string inputString = TutorialHandler.GetInputString(input);
        TutorialHandler.ShowTutorialStatic(text, inputString);
    }

    void OnThrow()
    {
        carryState = (int)CarryState.Thrown;
        HideAll();
    }
    bool ShouldShowInteract()
    {
        return interact != null && interact.isPlayerInside;
    }

    private void OnDestroy()
    {
        HideAll();
    }
}
