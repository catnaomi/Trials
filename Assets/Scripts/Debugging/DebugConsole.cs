using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public int logEntriesToShow = 10;
    public GameObject entryPrefab;
    public Transform entryParent;
    public TMP_InputField input;
    public ScrollRect scroll;
    public bool open;

    CanvasGroup group;
    TMP_Text[] entries;
    LogEntry[] logs;
    LinkedList<string> commandHistory;
    LinkedListNode<string> currentCommand;
    string[] logStrings;

    private void Start()
    {
        entries = new TMP_Text[logEntriesToShow];
        for (int i = 0; i < logEntriesToShow; i++)
        {
            entries[i] = Instantiate(entryPrefab, entryParent).GetComponent<TMP_Text>();
        }
        logs = new LogEntry[logEntriesToShow];
        logStrings = new string[logEntriesToShow];
        commandHistory = new LinkedList<string>();
        UpdateLogs();
        Application.logMessageReceivedThreaded += UpdateLogs;
        input.onSubmit.AddListener(OnSubmit);
        group = this.GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    private void Update()
    {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            ToggleOpen();
        }
        if (Keyboard.current.pageUpKey.wasPressedThisFrame)
        {
            float increment = (Screen.height / 2) / (entryParent as RectTransform).rect.height;
            scroll.verticalNormalizedPosition = Mathf.Clamp(scroll.verticalNormalizedPosition + increment, 0f, 1f);
        }
        if (Keyboard.current.pageDownKey.wasPressedThisFrame)
        {
            float increment = (Screen.height / 2) / (entryParent as RectTransform).rect.height;
            scroll.verticalNormalizedPosition = Mathf.Clamp(scroll.verticalNormalizedPosition - increment, 0f, 1f);
        }
        if (Keyboard.current.homeKey.wasPressedThisFrame)
        {
            scroll.verticalNormalizedPosition = 0f;
        }
        if (Keyboard.current.endKey.wasPressedThisFrame)
        {
            scroll.verticalNormalizedPosition = 1f;
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (commandHistory.Count > 0 && currentCommand != null && currentCommand.Previous != null)
            {
                currentCommand = currentCommand.Previous;
                input.text = currentCommand.Value;
            }
            else if (commandHistory.Count > 0 && currentCommand == null)
            {
                currentCommand = commandHistory.Last;
                input.text = currentCommand.Value;
            }
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (commandHistory.Count > 0 && currentCommand != null && currentCommand.Next != null)
            {
                currentCommand = currentCommand.Next;
                input.text = currentCommand.Value;
            }
        }
        if (open && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != input.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }
    }
    public void UpdateLogs(string logString, string stackTrace, LogType type)
    {
        LogRecordService.instance.GetLastNumberOfLogs(logEntriesToShow, logStrings);

        string lastLog = "";
        int index = 0;
        int repeats = 0;
        for (int i = 0; i < logEntriesToShow; i++)
        {
            if (logStrings[i] == lastLog)
            {
                repeats++;
                entries[index - 1].text = lastLog + " (x" + (repeats + 1) + ")";
                entries[i].gameObject.SetActive(false);
                continue;
            }
            entries[index].text = logStrings[i];
            entries[index].gameObject.SetActive(entries[index].text != null && entries[index].text.Length > 0);
            lastLog = logStrings[i];
            index++;
            repeats = 0;
        }
    }

    public void UpdateLogs()
    {
        UpdateLogs("", "", LogType.Log);
    }


    public void OnSubmit(string eventData)
    {
        Debug.Log("<color=#00FFFF>>" + eventData + "</color>");
        input.text = "";
        RunMethod(eventData);
        UpdateLogs();
        EventSystem.current.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
        LayoutRebuilder.ForceRebuildLayoutImmediate(entryParent as RectTransform);
        scroll.verticalNormalizedPosition = 0f;
        commandHistory.AddLast(eventData);
        currentCommand = null;
    }

    public void RunMethod(string input)
    {
        if (input == "") return;
        try
        {
            //string cleanInput = CleanString(input);
            string[] split = Regex.Split(input, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            string command = split[0];

            string[] args = split.Length > 1 ? new ArraySegment<string>(split, 1, split.Length-1).ToArray() : new string[0];

            Type type = typeof(DebugReflectionMethods);

            if (command.ToLower() == "help" && args.Length == 1)
            {
                command = "helpm";
            }
            MethodInfo method = type.GetMethod(command, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("Command not found.");
            }
            else
            {
                method.Invoke(null, args);
            }
        }
        catch (Exception ex)
        {
            
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
        
    }

    public void ToggleOpen()
    {
        open = !open;

        if (open)
        {
            TimeScaleController.instance.paused = true;
            UpdateLogs();
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            input.interactable = true;
            EventSystem.current.SetSelectedGameObject(input.gameObject);
        }
        else
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            input.interactable = false;
            TimeScaleController.instance.paused = false;
        }
    }

    public string CleanString(string dirtyString)
    {
        HashSet<char> removeChars = new HashSet<char>(" ?&^$#@!()+-,:;<>’\'-_*");
        StringBuilder result = new StringBuilder(dirtyString.Length);
        foreach (char c in dirtyString)
            if (!removeChars.Contains(c)) // prevent dirty chars
                result.Append(c);
        return result.ToString();
    }

    private void OnApplicationQuit()
    {
        Application.logMessageReceivedThreaded -= UpdateLogs;
    }
}
