// by OnatKorucu
// http://answers.unity.com/answers/1874177/view.html

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class LogRecordService : MonoBehaviour //WARNING: Do not Debug.Log() inside this class, it might create an endless loop.
{
    #region SingletonImplementation
    public static LogRecordService instance;

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
    #endregion
    const int INITIAL_CHARACTER_CAPACITY = 15000;
    const int MAXIMUM_NUMBER_OF_RECORDED_LOGS = 300;
    private List<LogEntry> _logEntries;
    private StringBuilder _stringBuilder;

    public void Start() //WARNING: If you won't use Singleton, you better unsubscribe from those events, for instance within OnDestroy()
    {
        _logEntries = new List<LogEntry>();

        Application.logMessageReceivedThreaded += HandleLogMessageReceived;
        SROptions.OnCopyAllLogsButtonPressed += HandleCopyAllLogsButtonPressed;
        SROptions.OnCopyAllDistinctLogsButtonPressed += HandleCopyAllDistinctLogsButtonPressed;
    }

    private void HandleCopyAllLogsButtonPressed()
    {
        CopyLogEntryListToClipboard(_logEntries);
    }

    private void HandleCopyAllDistinctLogsButtonPressed()
    {
        List<LogEntry> _logEntriesWithoutDuplicates = _logEntries.Distinct(new LogEntryComparer()).ToList();

        CopyLogEntryListToClipboard(_logEntriesWithoutDuplicates);
    }

    private void CopyLogEntryListToClipboard(List<LogEntry> logEntries)
    {
        _stringBuilder = new StringBuilder(INITIAL_CHARACTER_CAPACITY);

        foreach (LogEntry logEntry in logEntries)
        {
            _stringBuilder.Append(logEntry.LogString + "\n" + logEntry.StackTrace + "\n");
        }

        GUIUtility.systemCopyBuffer = _stringBuilder.ToString();
    }

    public void HandleLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        //#if !UNITY_EDITOR //You can make the functions only work outside UNITY_EDITOR to get a small performance boost while working on other stuff if you want.
        LogEntry newLogEntry = new LogEntry(logString, stackTrace, type);
        _logEntries.Add(newLogEntry);

        if (_logEntries.Count > MAXIMUM_NUMBER_OF_RECORDED_LOGS)
        {
            _logEntries.RemoveAt(0);
        }
        //#endif
    }

    public void GetLastNumberOfLogs(int amount, string[] logs)
    {
        if (_logEntries == null) return;
        int index = 0;
        for (int i = _logEntries.Count - 1; i >= 0 && index < amount; i--)
        {
            LogEntry entry = _logEntries[i];
            logs[index] = entry.Type.ToString() + ": " + entry.LogString;
            index++;
        }
    }
}

internal class LogEntry
{
    public string LogString { get; }
    public string StackTrace { get; }
    public LogType Type { get; }

    public LogEntry(string logString, string stackTrace, LogType type)
    {
        LogString = logString;
        StackTrace = stackTrace;
        Type = type;
    }
}

internal class LogEntryComparer : IEqualityComparer<LogEntry>
{
    public int GetHashCode(LogEntry logEntry)
    {
        return 0;
    }

    public bool Equals(LogEntry x, LogEntry y)
    {
        return x != null
               && y != null
               && x.LogString.Equals(y.LogString)
               && x.StackTrace.Equals(y.StackTrace)
               && x.Type.ToString().Equals(y.Type.ToString());
    }
}