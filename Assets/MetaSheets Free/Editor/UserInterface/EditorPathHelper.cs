namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright � 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	public class EditorPathHelper {
		public static string GetClassFolder<T>() {
			System.Type type = typeof(T);
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
			FileInfo[] fileInfos = directory.GetFiles(string.Format("*{0}.cs",type.Name), SearchOption.AllDirectories);
			foreach (FileInfo fileInfo in fileInfos) {
				string dir = fileInfo.Directory.FullName;
				return "Assets" + dir.Substring(Application.dataPath.Length);
			}
			return "";
		}
	}
}
