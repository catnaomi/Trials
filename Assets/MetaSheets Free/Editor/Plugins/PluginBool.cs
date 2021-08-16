namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class PluginBool : Plugin {
		public PluginBool(eType type= eType._bool, string columnKeyWord = "bool", bool isCommaValue = false) : base(type, columnKeyWord, isCommaValue) {
		}
		public override bool IsValid(string cell) {
			bool valBool;
			if (bool.TryParse(cell, out valBool)) {
				return true;
			}
			return false;
		}
		override public string GetValue(string cell, string className, string sheetName, string columnName) {
			cell = Trim(cell);
			bool parseTry;
			if (cell.Length == 0) {
				return "false";
			}else if (TryParse(cell, out parseTry)) {
				return parseTry.ToString().ToLower();
			} else {
				return cell;
			}
		}
		protected bool TryParse(string cell, out bool value) {
			cell = cell.Trim();
			if(cell == "1") {
				value = true;
				return true;
			}else if(cell == "0") {
				value = false;
				return true;
			}else if(bool.TryParse(cell, out value)) {
				return true;
			}
			value = false;
			return false;
		}
	}
}

