namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using System.Collections;
	using UnityEditor;
	[CustomEditor(typeof(Settings))]
	public class EditorSettings : Editor {
		private Settings Component {
			get {
				return (Settings)target;
			}
		}
		public override void OnInspectorGUI() {
			if (UserInterface.OnGUI_ScriptableObject(null, Component.configuration)) {
				EditorUtility.SetDirty(Component);
			}
		}
	}
}

