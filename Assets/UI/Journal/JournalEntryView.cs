using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalEntryView : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text epithet;
    public Image image;
    public GameObject weaknessesPanel;
    public GameObject weaknessesPrefab;
    public int weaknessesAmount = 3;
    List<StatElement> statElements;
    public Transform weaknessesParent;
    public TMP_Text notes;
    public CanvasGroup canvasGroup;
    public CanvasGroup newEntry;
    public Button showDescriptionButton;
    public Button nextButton;
    public Button prevButton;

    private void Start()
    {
        statElements = new List<StatElement>();
        for (int i = 0; i < weaknessesAmount; i++)
        {
            GameObject weaknessObject = Instantiate(weaknessesPrefab, weaknessesParent);
            statElements.Add(weaknessObject.GetComponent<StatElement>());
            weaknessObject.SetActive(false);
        }
        UpdateButtons("", "");
    }

    private void OnGUI()
    {
        if (newEntry.alpha > 0)
        {
            newEntry.alpha = Mathf.MoveTowards(newEntry.alpha, 0f, Time.deltaTime * 0.1f);
        }
    }
    public void SetJournalEntry(JournalEntry entry)
    {
        title.text = entry.displayName;
        epithet.text = entry.epithet;
        image.sprite = entry.image;
        notes.text = entry.notes;

        DamageType[] weaknesses = entry.weaknesses.ToArray();
        for (int i = 0; i < Mathf.Max(weaknesses.Length, statElements.Count); i++)
        {
            if (i < weaknesses.Length)
            {
                if (i >= weaknessesAmount)
                {
                    GameObject weaknessObject = Instantiate(weaknessesPrefab, weaknessesParent);
                    statElements[i] = weaknessObject.GetComponent<StatElement>();
                    weaknessObject.SetActive(false);
                }
                statElements[i].type = weaknesses[i];
                statElements[i].gameObject.SetActive(true);
                statElements[i].SetSprite();       
            }
            else
            {
                statElements[i].gameObject.SetActive(false);
            }
            
        }
        showDescriptionButton.gameObject.SetActive(entry.yarnNode != "");
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void UpdateButtons(string nextText, string prevText)
    {
        if (nextText != "")
        {
            nextButton.gameObject.SetActive(true);
            nextButton.GetComponentInChildren<TMP_Text>().text = nextText;
        }
        else
        {
            nextButton.gameObject.SetActive(false);
        }

        if (prevText != "")
        {
            prevButton.gameObject.SetActive(true);
            prevButton.GetComponentInChildren<TMP_Text>().text = prevText;
        }
        else
        {
            prevButton.gameObject.SetActive(false);
        }
    }

    public void NextEvent()
    {
        JournalController.journal.ShowNextEntry();
    }

    public void PrevEvent()
    {
        JournalController.journal.ShowPrevEntry();
    }

    public void ShowDescriptionEvent()
    {
        JournalController.journal.ShowDialogue();
    }
    public void OnNewEntry()
    {
        newEntry.alpha = 1f;
    }
    
    public void StopNewEntry()
    {
        newEntry.alpha = 0f;
    }
}
