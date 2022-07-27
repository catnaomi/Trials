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
    public bool open;
    float timeScale;
    CanvasGroup group;
    TMP_Text[] entries;
    LogEntry[] logs;
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
        UpdateLogs();
        Application.logMessageReceivedThreaded += UpdateLogs;
        input.onSubmit.AddListener(OnSubmit);
        group = this.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            ToggleOpen();
        }
        if (open && EventSystem.current.currentSelectedGameObject != input.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }
    }
    public void UpdateLogs(string logString, string stackTrace, LogType type)
    {
        LogRecordService.instance.GetLastNumberOfLogs(logEntriesToShow, logStrings);
        for (int i = 0; i < logEntriesToShow; i++)
        {
            entries[i].text = logStrings[i];
            entries[i].gameObject.SetActive(entries[i].text != null && entries[i].text.Length > 0);
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
        if (!open)
        {
            UpdateLogs();
            group.alpha = 1f;
            input.interactable = true;
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            open = true;
            timeScale = Time.timeScale;
            Time.timeScale = 0f;

        }
        else
        {
            group.alpha = 0f;
            input.interactable = false;
            Time.timeScale = timeScale;
            open = false;
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
}
