using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ProExampleTranslations : MonoBehaviour {
	void Start() {
		transform.Find("groupLanguages/btnEnglish").GetComponent<Button>().onClick.AddListener(OnClickEnglish);
		transform.Find("groupLanguages/btnFrench").GetComponent<Button>().onClick.AddListener(OnClickFrench);
		transform.Find("groupLanguages/btnGerman").GetComponent<Button>().onClick.AddListener(OnClickGerman);
		transform.Find("groupLanguages/btnSpanish").GetComponent<Button>().onClick.AddListener(OnClickSpanish);
		transform.Find("btnViewSpreadsheet").GetComponent<Button>().onClick.AddListener(OnClickOpenSpreadsheet);
		DCopy.SetLanguage(DCopy.eLanguage.en);
		OnLanguageChanged();
	}
	#region Button Listeners
	private void OnClickEnglish() {
		DCopy.SetLanguage(DCopy.eLanguage.en);
		OnLanguageChanged();
	}
	private void OnClickFrench() {
		DCopy.SetLanguage(DCopy.eLanguage.fr);
		OnLanguageChanged();
	}
	private void OnClickGerman() {
		DCopy.SetLanguage(DCopy.eLanguage.de);
		OnLanguageChanged();
	}
	private void OnClickSpanish() {
		DCopy.SetLanguage(DCopy.eLanguage.es);
		OnLanguageChanged();
	}
	private void OnClickOpenSpreadsheet() {
		Application.OpenURL("https://docs.google.com/spreadsheets/d/1MjFMcjygwJwFSpwB0kwdoh6m2NZBP0v1Iq1Wpq6ieoM/edit#gid=0");
	}
	#endregion
	/// <summary>
	/// Refreshes the UI list of elements to the latest Copy of the target Language
	/// </summary>
	private void OnLanguageChanged() {
		//Loop through each data sheet item
		foreach(DCopy copy in DataTranslations.copy) {
			//If we can rethrieve the Text component by defined path in the spreadsheet
			if(copy.GetText(this.transform) != null) {
				copy.GetText(this.transform).text = copy.Copy;
			}
		}
	}
}

