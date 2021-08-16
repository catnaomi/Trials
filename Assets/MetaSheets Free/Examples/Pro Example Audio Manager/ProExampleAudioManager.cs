using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ProExampleAudioManager : MonoBehaviour {
	public AudioClip[] audioClips;
	void Start() {
		transform.Find("btnViewSpreadsheet").GetComponent<Button>().onClick.AddListener(OnClickOpenSpreadsheet);
		transform.Find("group/btnWood").GetComponent<Button>().onClick.AddListener(OnClickPlayWood);
		transform.Find("group/btnMetal").GetComponent<Button>().onClick.AddListener(OnClickPlayMetal);
		transform.Find("group/btnStep").GetComponent<Button>().onClick.AddListener(OnClickPlayStep);
		//Link all data items with audioclip data
		foreach (DAudio item in DataAudio.sounds) {
			item.LinkAudioClips(audioClips);
		}
		SetupButtonHoverSounds("btnViewSpreadsheet");
		SetupButtonHoverSounds("group/btnWood");
		SetupButtonHoverSounds("group/btnMetal");
		SetupButtonHoverSounds("group/btnStep");
	}
	#region Button Actions
	/// <summary>
	/// When we press the Metal sounds button play a random metal sound
	/// </summary>
	private void OnClickPlayMetal() {
		DataAudio.sounds.metals.Play();
	}
	/// <summary>
	/// When we press the steps sounds button play a random step sound
	/// </summary>
	private void OnClickPlayStep() {
		DataAudio.sounds.steps.Play();
	}
	/// <summary>
	/// When we press the steps sounds button play a random step sound
	/// </summary>
	private void OnClickPlayWood() {
		DataAudio.sounds.woods.Play();
	}
	/// <summary>
	/// When we clicked on the open spreadsheet button
	/// </summary>
	private void OnClickOpenSpreadsheet() {
		Application.OpenURL("https://docs.google.com/spreadsheets/d/1V77KkLOCLC7qbDVm1B9l_IJigBfc6eNOn0xNGS9DZms/edit#gid=0");
	}
	#endregion
	private void SetupButtonHoverSounds(string path) {
		Button button = transform.Find(path).GetComponent<Button>();
		AddEventListener(button, EventTriggerType.PointerEnter, DataAudio.sounds.blib.Play);
		AddEventListener(button, EventTriggerType.PointerExit, DataAudio.sounds.plop.Play);
		AddEventListener(button, EventTriggerType.PointerClick, DataAudio.sounds.wush.Play);
	}
	private void AddEventListener(Button button, EventTriggerType eventTriggerType, System.Action action) {
		EventTrigger.Entry eventType = new EventTrigger.Entry();
		eventType.eventID = eventTriggerType;
		eventType.callback.AddListener((BaseEventData e) => {
			action();
		});
		//Add eventType to EventTrigger Component
		EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
		if (eventTrigger == null) {
			eventTrigger = button.gameObject.AddComponent<EventTrigger>();
		}
		eventTrigger.triggers.Add(eventType);
	}
}

