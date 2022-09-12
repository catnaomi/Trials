using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalController : MonoBehaviour
{
    public static JournalController journal;
    public JournalEntryView journalEntryView;
    public JournalEntry currentEntry;

    public List<string> knownEntries;
    public int currentIndex;

    public float maxAutoJournalDistance = 5f;
    public bool showing;
    PlayerTargetManager targetManager;
    [SerializeField, ReadOnly] JournalEntry hoveredEntry;
    [SerializeField, ReadOnly] bool hoveredIsNew;
    [SerializeField, ReadOnly] JournalEntry nextEntry;
    [SerializeField, ReadOnly] JournalEntry prevEntry;
    public UnityEngine.InputSystem.InputActionReference nextEntryAction;
    public UnityEngine.InputSystem.InputActionReference previousEntryAction;
    public UnityEngine.InputSystem.InputActionReference showDialogueAction;
    private void Awake()
    {
        journal = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //knownEntries = new List<string>(); // TODO: load this from save file
        targetManager = FindObjectOfType<PlayerTargetManager>();
        if (targetManager != null)
        {
            targetManager.OnTargetUpdate.AddListener(GetHoveredEntryFromTarget);
        }
        journalEntryView.Hide();

        nextEntryAction.action.performed += (c) =>
        {
            ShowNextEntry();
        };

        previousEntryAction.action.performed += (c) =>
        {
            ShowPrevEntry();
        };

        showDialogueAction.action.performed += (c) =>
        {
            ShowDialogue();
        };
     }

    public void GetHoveredEntryFromTarget()
    {
        if (targetManager.currentTarget != null && Vector3.Distance(targetManager.currentTarget.transform.position, PlayerActor.player.transform.position) < maxAutoJournalDistance && targetManager.currentTarget.transform.root.TryGetComponent<ActorAttributes>(out ActorAttributes attributes))
        {
            hoveredEntry = attributes.journalEntry;
            if (!knownEntries.Contains(hoveredEntry.name))
            {
                hoveredIsNew = true;
            }
            else
            {
                hoveredIsNew = false;
            }
        }
    }
    public void OpenJournal()
    {
        showing = true;
        journalEntryView.Show();
        if (hoveredEntry != null)
        {
            OpenEntry(hoveredEntry);
        }
        else if (knownEntries.Count > 0)
        {
            OpenEntry(knownEntries[currentIndex]);
        }
    }

    public void CloseJournal()
    {
        showing = false;
        journalEntryView.Hide();
    }

    public void OpenEntry(string entryName)
    {
        var entry = GetEntryFromName(entryName);
        if (entry != null)
        {
            OpenEntry(entry);
        }
    }

    public JournalEntry GetEntryFromName(string entryName)
    {
        return Resources.Load<JournalEntry>("JournalEntries/" + entryName);
    }
    public void OpenEntry(JournalEntry journalEntry)
    {
        int index = knownEntries.FindIndex((s) => s.Equals(journalEntry.name));
        bool isNew = false;
        if (index == -1)
        {
            index = knownEntries.Count;
            knownEntries.Add(journalEntry.name);
            isNew = true;
        }
        currentIndex = index;
        currentEntry = journalEntry;
        
        journalEntryView.SetJournalEntry(journalEntry);
        

        if (isNew)
        {
            knownEntries.Sort();
            journalEntryView.OnNewEntry();
        }
        else
        {
            journalEntryView.StopNewEntry();
        }

        UpdateAdjacentEntries();
    }

    public void ShowNextEntry()
    {
        if (showing && nextEntry != null)
        {
            OpenEntry(nextEntry);
        }
    }

    public void ShowPrevEntry()
    {
        if (showing && prevEntry != null)
        {
            OpenEntry(prevEntry);
        }
    }

    public void ShowDialogue()
    {
        if (showing)
        {
            var runner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<Yarn.Unity.DialogueRunner>();
            if (runner != null && !runner.IsDialogueRunning && currentEntry != null && currentEntry.yarnNode != "")
            {
                runner.StartDialogue(currentEntry.yarnNode);
            }
        }
    }
    public void UpdateAdjacentEntries()
    {
        string next = "";
        string prev = "";
        if (true)
        {
            if (knownEntries.Count > 1)
            {
                int nextIndex = currentIndex + 1;
                nextIndex %= knownEntries.Count;
                nextEntry = GetEntryFromName(knownEntries[nextIndex]);
                next = nextEntry.displayName;
            }
            else
            {
                nextEntry = null;
            }
        }
        
        if (true)
        {
            if (knownEntries.Count > 1)
            {
                int prevIndex = currentIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = knownEntries.Count - 1;
                }
                prevEntry = GetEntryFromName(knownEntries[prevIndex]);
                prev = prevEntry.displayName;
            }
            else
            {
                prevEntry = null;
            }
        }
        journalEntryView.UpdateButtons(next, prev);
        
    }

    public bool ShouldOpenToJournal()
    {
        return hoveredEntry != null && hoveredIsNew;
    }
}
