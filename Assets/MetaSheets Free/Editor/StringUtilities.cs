namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	public class StringUtilities {
		public static string Format(string template, params object[] args) {
			string output = template;
			for (int i = 0; i < args.Length; i++) {
				output = output.Replace("{" + i.ToString() + "}", args[i].ToString());
			}
			return output;
		}
		public static string GetCamelCase(string inp) {
			inp = inp.Trim();
			if (string.IsNullOrEmpty(inp)) {
				return "";
			}
			string[] words = inp.Split(' ');
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < words.Length; i++) {
				string s = words[i];
				if (s.Length > 0) {
					string firstLetter = s.Substring(0, 1);
					string rest = s.Substring(1, s.Length - 1);
					if (i == 0) {
						sb.Append(firstLetter.ToLower() + rest);//DON'T MODIFY FIRST CHARACTER
					} else {
						sb.Append(firstLetter.ToUpper() + rest);
					}
					sb.Append(" ");
				}
			}
			return (sb.ToString().Substring(0, sb.ToString().Length - 1)).Replace(" ", "");
		}
		public static string GetPascalCase(string inp) {
			inp = inp.Trim();
			if (string.IsNullOrEmpty(inp)) {
				return "";
			}
			inp = GetCamelCase(inp);
			return inp.Substring(0, 1).ToUpper() + inp.Substring(1);
		}
	}
}
