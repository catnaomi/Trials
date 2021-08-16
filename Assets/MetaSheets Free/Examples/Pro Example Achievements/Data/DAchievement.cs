using UnityEngine;
using System.Collections;
public class DAchievement {
	//Extending variables from spreadsheet
	public string id;
	public int count;
	public string message;
	public string idIOS;
	public string idAndroid;
	//Private variables
	/// <summary>
	/// Track an incremental progression for this achievement
	/// </summary>
	public void Increment() {
		SaveStep++;
		ValidateEvent();
    }
	/// <summary>
	/// Validate against the internal count for this achievement
	/// </summary>
	/// <param name="value">The value we compare against: if its greater or equal to the count</param>
	public void Validate(int value) {
		//Only continue if the value has changed
		if(value != SaveStep) {
			SaveStep = value;
			ValidateEvent();
		}
	}
	/// <summary>
	/// Validates the saved step (SaveStep) against the count to unlock the achievement
	/// </summary>
	private void ValidateEvent() {
		//Validate only if we haven't shown this achievement yet
		if(SaveIsShown == false) {
			//Validate that the saved steps are equal or exceed the count from the sheet
			if (SaveStep >= count) {
				//Display this achievement in the ProExampleAchievements component that contains the UI
				GameObject.FindObjectOfType<ProExampleAchievements>().DisplayAchievement(this);
				//Remember that we have shown this now
				SaveIsShown = true;
			} else {
				Debug.Log("You are " + (count - SaveStep) + " from unlocking " + this.id);
			}
		}
	}
	#region Helpers
	/// <summary>
	/// Stores the increment step that we progressed so far with
	/// </summary>
	private int SaveStep {
		get {
			return PlayerPrefs.GetInt("achievement_step_" + this.id, 0);
		}
		set {
			PlayerPrefs.SetInt("achievement_step_" + this.id, value);
        }
	}
	/// <summary>
	/// Stores in the Player prefs a boolean to remember if we have shown this achievement before or not
	/// </summary>
	private bool SaveIsShown {
		get {
			return PlayerPrefs.GetInt("achievement_isShown" + this.id, 0) == 1 ? true : false;
		}
		set {
			PlayerPrefs.SetInt("achievement_isShown" + this.id, value ? 1 : 0);
		}
	}
	#endregion
}

