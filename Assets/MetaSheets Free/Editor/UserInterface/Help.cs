namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	public class Help {
		public enum eImage {
			none,
			publish,
			emptyId,
			noContentRow,
			copyUrl,
			example,
			overview,
			refError,
			cellWrongClass,
			cellWrongClassArray
		}
		public static bool isEnabled = false;
		private static string title = "";
		private static string body = "";
		private static eImage image = eImage.none;
		private static float height;
		private static Dictionary<eImage, Texture> dictionaryTextures = new Dictionary<eImage, Texture>();
		public static void ShowHelp(string title, string body) {
			ShowHelp(title, body, eImage.none);
		}
		public static void ShowHelp(string title, string body, eImage image) {
			isEnabled = true;
			Help.title = title;
			Help.body = body;
			Help.image = image;
		}
		public static void ShowOverview() {
			isEnabled = true;
			Help.title = "Help";
			Help.body = "";
			Help.image = eImage.overview;
		}
		#region Draw
		public static void OnGUI() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(GUILayout.MaxWidth(320));
			if (image == eImage.overview) {
				DrawPageOverview();
			} else {
				DrawPage();
			}
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Ok", GUILayout.Height(24f), GUILayout.Width(96f))) {
				isEnabled = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(8);
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		private static void DrawPage() {
			GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
			guiStyle.wordWrap = true;
			GUILayout.Space(8);
			EditorGUILayout.HelpBox(title, MessageType.None);
			EditorGUILayout.LabelField(body, guiStyle);
			OnDrawImage();
		}
		private static void DrawPageOverview() {
			GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
			guiStyle.wordWrap = true;
			EditorGUILayout.LabelField(Copy.copy.help_overviewIntroduction, guiStyle);
			OnDrawImage("help_overview");
			if (OnDrawButton("Documentation")) {
				Application.OpenURL("http://metasheets.com/#documentation");
			}
			if (OnDrawButton("Data Type Reference")) {
				Application.OpenURL("http://metasheets.com/#dataTypes");
			}
			EditorGUILayout.LabelField(Copy.copy.help_overviewDataTypes, guiStyle);
			GUILayout.BeginHorizontal();
			if (OnDrawButton("Support")) {
				Application.OpenURL("https://metasheets.helprace.com");
			}
			if (OnDrawButton("Blog")) {
				Application.OpenURL("http://www.metasheets.com/blog");
			}
			GUILayout.EndHorizontal();
		}
		private static bool OnDrawButton(string label) {
			bool isActive = false;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(label, GUILayout.Height(24f), GUILayout.ExpandWidth(true))) {
				isActive = true;
			}
			GUILayout.EndHorizontal();
			return isActive;
		}
		private static void OnDrawImage() {
			OnDrawImage("help_" + image.ToString());
		}
		private static void OnDrawImage(string fileName) {
			if (image != eImage.none) {
				Texture texture;
				if (!dictionaryTextures.ContainsKey(image)) {
					texture = MetaSheetsImages.Get(fileName);
					dictionaryTextures.Add(image, texture);
				} else {
					texture = dictionaryTextures[image];
				}
				float w = texture.width;
				float h = texture.height * w / texture.width;
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.border = new RectOffset(0, 0, 1, 1);
				guiStyle.padding = new RectOffset(0, 0, 1, 1);
				GUILayout.Box(texture, guiStyle, GUILayout.Width(w), GUILayout.Height(h));
			}
		}
		#endregion
	}
}
