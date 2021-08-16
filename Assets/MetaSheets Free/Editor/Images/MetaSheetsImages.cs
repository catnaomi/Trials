namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using System.IO;
	public class MetaSheetsImages {
		private static string folder = "";
		private static bool folderSearched = false;
		private static string Folder {
			get {
				if(folder.Length == 0 && !folderSearched) {
					folder = EditorPathHelper.GetClassFolder<MetaSheetsImages>();
					folderSearched = true;
				}
				return folder;
			}
		}
		private static Dictionary<string, Texture2D> cachedValues = new Dictionary<string, Texture2D>();
		public static Texture2D Get(string fileName) {
			if (cachedValues.ContainsKey(fileName) == false) {
				Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>( Path.Combine(Folder,fileName + ".png"));
				cachedValues.Add(fileName, tex);
			}
			return cachedValues[fileName];
		}
#region Common Textures
		public static Texture2D Logo { get { return Get("ui_logo");} }
		public static Texture2D White { get { return Get("ui_white"); } }
		public static Texture2D IconDocument { get { return Get("ui_iconDocument"); } }
		public static Texture2D IconGenerate { get { return Get("ui_iconGenerate"); } }
		public static Texture2D IconTabHelp { get { return Get("ui_iconHelp"); } }
		public static Texture2D IconTabSettings { get { return Get("ui_iconTabSettings"); } }
		public static Texture2D IconClass { get { return Get("ui_iconClass"); } }
		public static Texture2D IconOpen { get { return Get("ui_iconOpen"); } }
		public static Texture2D IconCreate { get { return Get("ui_iconCreate"); } }
#endregion
	}
}
