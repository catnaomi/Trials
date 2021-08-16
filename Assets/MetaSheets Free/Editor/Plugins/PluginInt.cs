namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class PluginInt : Plugin {
		public PluginInt() : base(eType._int, "int") {
		}
		override public bool IsValid(string cell) {
			int valInt;
			if (int.TryParse(cell, out valInt))
				return true;
			return false;
		}
		override public string GetValue(string cell, string className, string sheetName, string columnName) {
			int parseTry;
			if(cell.Trim().Length == 0) {
				return "0";
			}else if (int.TryParse(cell, out parseTry)) {
				return parseTry.ToString();
			} else {
				return cell;
			}
		}
	}
}

