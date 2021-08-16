namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class PluginVector2 : Plugin {
		public PluginVector2() : base(eType._Vector2, "Vector2", true) {
		}
		public override bool IsValid(string cell) {
			if (cell.Contains(",")) {
				string[] array = cell.Split(',');
				int countFloat = 0;
				float result;
				foreach (string s in array) {
					if (float.TryParse(s, out result)) {
						countFloat++;
					}
				}
				if (countFloat == array.Length && countFloat == 2) {
					return true;
				}
			}
			return false;
		}
		public override string GetValue(string cell, string className, string sheetName, string columnName) {
			cell = Trim(cell);
			if (cell == "") {
				return "Vector2.zero";
			} else {
				List<string> values = new List<string>();
				foreach(string s in cell.Split(',')) {
					float parsed;
					if(s.Trim().Length == 0) {
						values.Add("0f");
					}else if(float.TryParse(s, out parsed)){
						values.Add(parsed.ToString() + "f");
					} else {
						values.Add(s.Trim());
					}
				}
				return "new Vector2(" + string.Join(",", values.ToArray()) + ")";
			}
		}
	}
}

