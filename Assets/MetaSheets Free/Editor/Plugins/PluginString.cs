namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class PluginString : Plugin {
		public PluginString() : base(eType._string, "string") {
		}
		override public bool IsValid(string cell) {
			return true;
		}
		override public string GetValue(string cell, string className, string sheetName, string columnName) {
			return "\"" + cell.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\n") + "\"";
		}
	}
}
