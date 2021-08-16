using UnityEngine;
using System.Collections;
using UnityEditor;
///		Meta Sheets Free
///		Copyright © 2017 renderhjs
///		Version 2.00.0
///		Website www.metasheets.com
public class DMetaSheetsCopy {
	public string copy;
	public string title;
	public void ShowAlert(params object[] arguments) {
		EditorUtility.DisplayDialog(Title(arguments), Copy(arguments), "Ok");
	}
	public string Copy(params object[] arguments) {
		return Format(this.copy, arguments);
	}
	public string Title(params object[] arguments) {
		return Format(this.title, arguments);
	}
	public void ShowHelp(params object[] arguments) {
		ShowHelp(MetaSheets.Help.eImage.none, arguments);
	}
	public void ShowHelp(MetaSheets.Help.eImage image, params object[] arguments) {
		MetaSheets.Help.ShowHelp(Title(arguments), Copy(arguments), image);
	}
	public static implicit operator string(DMetaSheetsCopy item) {
		return item.copy;
	}
	private string Format(string template, params object[] arguments) {
		string output = template;
		for (int i = 0; i < arguments.Length; i++) {
			output = output.Replace("{" + i.ToString() + "}", arguments[i].ToString());
		}
		return output;
	}
}
