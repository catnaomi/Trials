namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using System.IO;
	using System.Collections.Generic;
	using UnityEditor;
	public class Settings : ScriptableObject {
		public Configuration configuration = new Configuration("","");
		private const string fileName = "{0} Settings";
		#region Set and Get
		public static void Save(Configuration configuration) {
			if(configuration.className != "") {
				Settings file = Get(configuration.className);
				if (file != null) {
					file.configuration.Apply(configuration);
					EditorUtility.SetDirty(file);
				}else if (file == null) {
					file = ScriptableObject.CreateInstance<Settings>();
					file.configuration.Apply(configuration);
					string name = string.Format( Settings.fileName, configuration.className);
                    string path = Path.Combine("Assets", name + ".asset");
					AssetDatabase.CreateAsset(file, path);
					files.Add(file);
					Copy.copy.popup_newSettingsFile.ShowAlert(name);
					EditorGUIUtility.PingObject(file);
				}
            }
		}
		public static Settings Get(string className) {
			for (int i = 0; i < All.Length; i++) {
				if(All[i] != null && All[i].configuration.className == className) {
					return All[i];
				}
			}
			return null;
		}
#endregion
#region All Settings
		private static List<Settings> files = new List<Settings>();
		private static bool isFilesRead = false;
		public static Settings[] All {
			get {
				if (files.Count == 0 && !isFilesRead) {
					string[] guids = AssetDatabase.FindAssets("t:" + typeof(Settings), null);
					if (guids.Length > 0) {
						files = new List<Settings>();
						for (int i = 0; i < guids.Length; i++) {
							files.Add(AssetDatabase.LoadAssetAtPath<Settings>(AssetDatabase.GUIDToAssetPath(guids[i])));
						}
					}
					isFilesRead = true;
				}
				return files.ToArray();
			}
		}
#endregion
	}
}
