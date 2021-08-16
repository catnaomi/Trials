namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public enum eType {
		_Vector2,
		_Vector3,
		_bool,
		_float,
		_int,
		_string,
	}
	public class Plugin {
		public readonly eType type;
		public readonly string columnKeyWord;
		public readonly bool isCommaValue;
		public Plugin(eType type, string columnKeyWord, bool isCommaValue = false) {
			this.type = type;
			this.columnKeyWord = columnKeyWord;
			this.isCommaValue = isCommaValue;
		}
		public virtual string GetTypeName(string cellValue, string className, string sheetName, string columnName) {
			return columnKeyWord;
		}
		public virtual bool IsValid(string cell) {
			return false;
		}
		public virtual string GetValue(string cell, string className, string sheetName, string columnName) {
			return "null" + String.Format("", type.ToString(), cell);
		}
		protected string Trim(string cell) {
			return cell.Replace(" ", "").Replace("\t", "").Trim();
		}
	}
}
