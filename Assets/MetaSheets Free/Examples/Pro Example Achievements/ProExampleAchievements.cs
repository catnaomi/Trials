using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class ProExampleAchievements : MonoBehaviour {
	void Start() {
		transform.Find("btnViewSpreadsheet").GetComponent<Button>().onClick.AddListener(OnClickOpenSpreadsheet);
		transform.Find("groupAchievement").gameObject.SetActive(false);
	}
	void Update() {
		//Achievement for pressing 30x anywhere on the screen
		if (Input.GetMouseButtonDown(0)) {
			DataAchievements.achievements.click30.Increment();
		}
		//Achievement for pressing the U key
		if (Input.GetKeyDown(KeyCode.U)) {
			DataAchievements.achievements.pressKeyU.Increment();
		}
		//Check achievement against the time this app is running already
		DataAchievements.achievements.wait10Sec.Validate(Mathf.FloorToInt(Time.time));
	}
	#region Button Actions
	/// <summary>
	/// When we clicked on the open spreadsheet button
	/// </summary>
	private void OnClickOpenSpreadsheet() {
		Application.OpenURL("https://docs.google.com/spreadsheets/d/1Dgv7XxbY3zQ3wwrheekS_iTzarxnWGzDznetWZD6b0s/edit#gid=0");
	}
	private void OnClickRedButton() {
		DataAchievements.achievements.clickRed.Increment();
	}
	#endregion
	/// <summary>
	/// Displays the achievement in the UI. Calls internally a caroutine
	/// </summary>
	/// <param name="achievement"></param>
	public void DisplayAchievement(DAchievement achievement) {
		StartCoroutine(IDisplayAchievement(achievement));
	}
	/// <summary>
	/// Caroutine to display the achievement and then after a break to close it again
	/// </summary>
	/// <param name="achievement"></param>
	/// <returns></returns>
	IEnumerator IDisplayAchievement(DAchievement achievement) {
		//Show the groupAchievement container
		transform.Find("groupAchievement").gameObject.SetActive(true);
		//Display the message and the id's for Android and IOS
		transform.Find("groupAchievement/txtMessage").GetComponent<Text>().text = achievement.message;
		transform.Find("groupAchievement/txtAndroid").GetComponent<Text>().text = "Android ID\n" + achievement.idAndroid;
		transform.Find("groupAchievement/txtIOS").GetComponent<Text>().text = "IOS ID\n" + achievement.idIOS;
		yield return new WaitForSeconds(5f);
		//Close the groupAchievement container
		transform.Find("groupAchievement").gameObject.SetActive(false);
	}
}

