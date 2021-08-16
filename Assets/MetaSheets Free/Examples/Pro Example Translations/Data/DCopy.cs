using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DCopy {
	private static eLanguage language = eLanguage.en;//Default internal language
	//Public Variables to Extend (Matching in the Spreadsheet)
	public string scenePath;
	public string en;
	public string fr;
	public string de;
	public string es;
	//Enum to determine the current Langauge
	public enum eLanguage {
		en,
		de,
		fr,
		es
	}
	/// <summary>
	/// Set the new language
	/// </summary>
	public static void SetLanguage(eLanguage language) {
		DCopy.language = language;
	}
	/// <summary>
	/// Return the copy in the current language
	/// </summary>
	public string Copy {
		get {
			if(language == eLanguage.en) {
				return en;
			}else if(language == eLanguage.fr) {
				return fr;
			}else if(language == eLanguage.de) {
				return de;
			} else if (language == eLanguage.es) {
				return es;
			}
			return en;//Default value
		}
	}
	/// <summary>
	/// Find the Text component that matches the path from the spreadsheet, e.g. "btnStart/Text"
	/// </summary>
	public Text GetText(Transform container) {
		Transform target = container.Find(this.scenePath);
		if (target != null) {
			return target.GetComponent<Text>();
		}
		return null;
	}
}

