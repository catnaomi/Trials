namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;
	using SimpleJSONMetaSheets;
	public class MetaSheets : EditorWindow {
		#region VARS
		public const string VERSION = "2.00.0";
		public static Configuration configuration = new Configuration("","");
		private const float loadTimeout = 45f;//DEFAULT TIMEOUT WHEN LOADING THE INDEX 
		#endregion
		void OnEnable() {
			if(window != null && this != window) {
				window.Close();
			}
			ResetApplication();
		}
		private void OnDestroy() {
			window = null;
		}
		[MenuItem("Assets/MetaSheets Free", false, 251)]
		public static void InitMenuItem() {
			ResetApplication();
		}
		public static void Build(string className, string documentKey) {
			Build(new Configuration(className, documentKey), null);
		}
	public static void Build(Configuration configuration, EditorWindow window) {
			if (!Application.isPlaying) {
				LoadIndex(configuration, window);
			} else {
				Copy.copy.popup_cantBuildWhileRunning.ShowAlert();
			}
		}
		private static void ResetApplication() {
			Load();
			EditorUtility.ClearProgressBar();
			UserInterface.InitWindow(Window);
		}
		void OnGUI() {
			if (UserInterface.OnGUI_EditorWindow(Window, configuration)) {
				Configuration.Cache = configuration;
			}
		}
#region SAVE AND LOAD SETTINGS
		public static void Save() {
			Configuration.Cache = configuration;
			Settings.Save(configuration);
		}
		private static void Load() {
			configuration = Configuration.Cache;
		}
#endregion
#region LOAD JSON
		private static void LoadIndex(Configuration configuration, EditorWindow window) {
			EditorUtility.DisplayProgressBar("Loading", "Fetching sheets", 0.1f);
			string url = configuration.DocumentURL;
			float time = (float)EditorApplication.timeSinceStartup;
			WWW request = new WWW(url);
			while (request.isDone != true) {
				float t = (float)EditorApplication.timeSinceStartup - time;
				if (t >= loadTimeout) {
					Copy.copy.help_timedOutIndex.ShowHelp(loadTimeout);
					EditorUtility.ClearProgressBar();
					request.Dispose();
					return;
				}
			}
			if (request.error != null && request.error.Contains("Bad Request")) {
				Copy.copy.help_400BadRequest.ShowHelp(Help.eImage.copyUrl, request.error);
				EditorUtility.ClearProgressBar();
				return;
			}
			if (request.text.Contains("<!DOCTYPE html>") && request.text.IndexOf("<!DOCTYPE html>") <= 10) {
				Copy.copy.help_notAccessable.ShowHelp(Help.eImage.publish);
				EditorUtility.ClearProgressBar();
				return;
			}
			JSONNode json = JSON.Parse(request.text);
			configuration.documentTitle = json["feed"]["title"]["$t"].Value;
			if (window  != null && window == Window) {
				Save();
			}
			int numSheets = int.Parse(json["feed"]["openSearch$totalResults"]["$t"].Value);
			List<string> documentGIDs = new List<string>();
			List<string> documentTitles = new List<string>();
			for (int i = 0; i < numSheets; i++) {
				string title = json["feed"]["entry"][i]["title"]["$t"].Value;
				if (title.IndexOf("//") != 0) {
					string gid = json["feed"]["entry"][i]["id"]["$t"].Value;
					gid = gid.Substring(gid.LastIndexOf("/") + 1);
					documentGIDs.Add(gid);
					documentTitles.Add(title);
				}
			}
			EditorUtility.DisplayProgressBar("Loading", "Fetching amount of pages", 0.3f);
			LoadSheets(configuration, documentGIDs.ToArray(), documentTitles.ToArray(), window);
		}
		private static void LoadSheets(Configuration configuration, string[] documentGIDs, string[] documentTitles, EditorWindow window) {
			string[] jsonDocs = new string[documentGIDs.Length];
			for (int i = 0; i < documentGIDs.Length; i++) {
				string url = "https://spreadsheets.google.com/feeds/cells/" + configuration.documentKey + "/" + documentGIDs[i] + "/public/basic?alt=json-in-script";
				float time = (float)EditorApplication.timeSinceStartup;
				WWW request = new WWW(url);
				while (request.isDone != true) {
					float t = (float)EditorApplication.timeSinceStartup - time;
					if (t >= loadTimeout) {
						Copy.copy.help_timedOutSheet.ShowHelp(documentTitles[i], loadTimeout);
						EditorUtility.ClearProgressBar();
						request.Dispose();
						return;
					}
					float pA = (float)(i + request.progress) / (float)documentGIDs.Length;//% chunk of this page
					float pSub = Mathf.Min(1f, t / 2f) * 1f / (float)documentGIDs.Length;//Relative time offset of this page
					float pTotal = 0.3f + pA * 0.7f + pSub;
					pTotal = Mathf.Clamp(pTotal, 0f, 1f);
					EditorUtility.DisplayProgressBar("Loading " + Mathf.RoundToInt(pTotal * 100) + "%", "Sheet " + documentTitles[i] + "", pTotal);
				}
				jsonDocs[i] = GetJson(request.text);//Clean up JS commands and comments
			}
			EditorUtility.ClearProgressBar();
			ParseJsonPages(configuration, documentGIDs, jsonDocs, window);
		}
		private static string GetJson(string inp) {
			int A = inp.IndexOf("{");
			int B = inp.LastIndexOf("}");
			if (A != -1 && B != -1) {
				return inp.Substring(A, B - A + 1);
			}
			return inp;
		}
#endregion
#region PARSE JSON
		private static List<string[][]> all_Cells;
		private static string[] all_SheetNames;
		private static void ParseJsonPages(Configuration configuration, string[] documentGIDs, string[] jsonDocs, EditorWindow window) {
			int j;
			int k;
			string classes = "";
			all_SheetNames	= new string[jsonDocs.Length];
			all_Cells		= new List<string[][]>();
			bool errorExceededRowsColumns = false;
			for (int i = 0; i < jsonDocs.Length; i++) {
				JSONNode json = JSON.Parse(jsonDocs[i]);
				string title = json["feed"]["title"]["$t"].Value;
				JSONNode entries = json["feed"]["entry"];
				int maxCols = 1;
				int maxRows = 1;
				string[][] cells = new string[100][];
				for (j = 0; j < cells.Length; j++) {
					cells[j] = new string[columnNames.Length];
				}
				for (j = 0; j < entries.Count; j++) {
					string cellId = entries[j]["title"]["$t"].Value;        //A1, B1, A2,...
					string cellContent = entries[j]["content"]["$t"].Value; //The cell value
					int[] colRow = GetColumnRow(cellId);
					maxCols = Mathf.Max(maxCols, colRow[0] + 1);
					maxRows = Mathf.Max(maxRows, colRow[1] + 1);
					if (colRow[1] < cells.Length && colRow[0] < cells[0].Length) {//Fits inside an constructor array
						cells[colRow[1]][colRow[0]] = cellContent;//Assign value
					} else {
						errorExceededRowsColumns = true;
					}
				}
				maxCols = Mathf.Min(maxCols, cells[0].Length);
				maxRows = Mathf.Min(maxRows, cells.Length);
				string[][] cellsTrimmed = new string[maxRows][];
				for (j = 0; j < maxRows; j++) {
					cellsTrimmed[j] = new string[maxCols];
					System.Array.Copy(cells[j], cellsTrimmed[j], maxCols);
				}
				cells = cellsTrimmed;
				List<int> keepColumns = new List<int>();
				for (j = 0; j < cells[0].Length; j++) {
					if (cells[0][j] != null) {
						if (cells[0][j].IndexOf("//") != 0) {
							keepColumns.Add(j);
						}
					}
				}
				if (keepColumns.Count != cells[0].Length) {
					for (j = 0; j < cells.Length; j++) {
						List<string> row = new List<string>();
						for (k = 0; k < keepColumns.Count; k++) {
							row.Add(cells[j][keepColumns[k]]);
						}
						cells[j] = row.ToArray();
					}
				}
				List<int> keepRows = new List<int>();
				for (j = 0; j < cells.Length; j++) {
					if (cells[j][0] == null || cells[j][0] == "") {
						int count = 0;
						for (k = 0; k < cells[j].Length; k++) {
							if (cells[j][k] == null || cells[j][k] == "") {
								count++;
							}
						}
						if (count >= cells[j].Length) {
							continue;
						}
					}
					if (cells[j].Length == 0 || cells[j][0] == null) {
						Copy.copy.help_wrongSheetSetup.ShowHelp(j + 1, title, Help.eImage.emptyId);
					}
					if (cells[j].Length > 0 && cells[j][0] != null && cells[j][0].IndexOf("//") != 0) {
						keepRows.Add(j);
					}
				}
				if (keepRows.Count != cells.Length) {
					string[][] cellsCopy = new string[keepRows.Count][];
					for (j = 0; j < keepRows.Count; j++) {
						cellsCopy[j] = cells[keepRows[j]];
					}
					cells = cellsCopy;
				}
				for (j = 0; j < cells.Length; j++) {
					for (k = 0; k < cells[j].Length; k++) {
						if (cells[j][k] == null) {
							cells[j][k] = "";
						}
					}
				}
				all_Cells.Add(cells);
				all_SheetNames[i] = StringUtilities.GetCamelCase(title);
			}
			if (errorExceededRowsColumns) {
				Copy.copy.help_notAllParsedFree.ShowHelp();
			}
			for (int i = 0; i < all_SheetNames.Length; i++) {
				string[][] cells = all_Cells[i];
				if (cells.Length == 0) {
					Copy.copy.help_noContentCells.ShowHelp(Help.eImage.example, all_SheetNames[i]);
					return;
				} else if (cells.Length == 1) {
					Help.ShowHelp("Aborting", "", Help.eImage.noContentRow);
					Copy.copy.help_noContentRows.ShowHelp(Help.eImage.noContentRow, all_SheetNames[i]);
					return;
				} else {
					int maxCols = cells[0].Length;
					int maxRows = cells.Length;
					JSONNode json = JSON.Parse(jsonDocs[i]);
					string updated = json["feed"]["updated"]["$t"].Value;
					string title = all_SheetNames[i];
					eType[] typesCols = new eType[maxCols];
					for (j = 0; j < maxCols; j++) {
						string[] colArray = new string[maxRows - 1];
						for (k = 0; k < colArray.Length; k++) {
							colArray[k] = cells[k + 1][j];
						}
						typesCols[j] = GetTypeColumn(title, cells[0][j], colArray);
					}
					string code_classRow = GetCode_Row(configuration, title, typesCols, cells);
					string code_classSheet = GetCode_Sheet(configuration, i, title, typesCols, cells, updated);
					string template = @"
namespace {0}{
	{1}
	{2}
}";
					classes += StringUtilities.Format(template,
						 GetName_ClassSheetNameSpace(configuration.className),
						 code_classRow,
						 code_classSheet
					);
				}
			}
			classes = GetCode_Main(configuration, all_Cells) + classes;
			string header = @"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
";
			header+="\n";	
			classes = header+classes;
			classes = Regex.Replace(classes, @"\r\n|\n\r|\n|\r", "\r\n");
			StreamWriter writer;
			string path;
			string[] pathExisting = Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "/Assets/", configuration.className + ".cs", SearchOption.AllDirectories);
			if (pathExisting.Length >= 1) {
				path = pathExisting[0];
			} else {
				path = System.IO.Directory.GetCurrentDirectory() + "/Assets/" + configuration.className + ".cs";
			}
			FileInfo fileInfo = new FileInfo(path);
			if (!fileInfo.Exists) {
				writer = fileInfo.CreateText();
			} else {
				fileInfo.Delete();
				writer = fileInfo.CreateText();
			}
			writer.Write(classes);
			writer.Close();
			if (window != null) {
				window.ShowNotification(new GUIContent(Copy.copy.notification_codeGenerated.Copy(configuration.className)));
			} else {
				Debug.Log(Copy.copy.notification_codeGenerated.Copy(configuration.className));
			}
			AssetDatabase.Refresh();
			if (pathExisting.Length == 0) {
				Copy.copy.popup_newDataFile.ShowAlert(configuration.className);
			}
		}
#endregion
#region GENERATE CLASS
		private static string GetCode_Main(Configuration configuration, List<string[][]> all_Cells) {
			int i;
			string clsInitSheetsData = "";
			string cls = "public class " + configuration.className + "{";
			cls += "\n\t//Document URL: " + configuration.DocumentURL +"\n";
			for (i = 0; i < all_SheetNames.Length; i++) {
				string clsSheetName = GetName_ClassSheet(all_SheetNames[i]);
				cls += "\n\t//Sheet " + clsSheetName;//Comment of the 'sheet name'
				cls += "\n\tpublic static " + GetName_ClassSheetNameSpace(configuration.className) + "." + clsSheetName + " " + GetName_Variable(all_SheetNames[i]) + " = new " + GetName_ClassSheetNameSpace(configuration.className) + "." + clsSheetName + "();";
				clsInitSheetsData += StringUtilities.Format("{0}.Init(); ", GetName_Variable(all_SheetNames[i]) );
			}
			cls += StringUtilities.Format(@"
	static {0}(){
		{1}
	}",
				configuration.className, clsInitSheetsData
			);
			cls += "\n}\n\n";
			return cls;
		}
		private static string GetCode_Row(Configuration configuration, string sheetName, eType[] typesCols, string[][] cells) {
			int i;
			string clsRowName = GetName_ClassRow(sheetName);
			string code = "public class " + clsRowName + "{";
			for (i = 0; i < cells[0].Length; i++) {//For each column in the spreadsheet
				Plugin plugin = plugins[typesCols[i]];
				{
					string variableTypeName = plugin.GetTypeName(cells[1][i], configuration.className, sheetName, cells[0][i]);
						code += "\n\t\tpublic " + variableTypeName + " " + GetName_Variable(cells[0][i]) + ";";
				}
			}
			string[] varNames = new string[cells[0].Length];
			for (i = 0; i < cells[0].Length; i++) {
				varNames[i] = GetName_Variable(cells[0][i], varNames);
			}
			code += "\n\n\t\tpublic " + clsRowName + "(){}";
			code += "\n\n\t\tpublic " + clsRowName + "(";
			for (i = 0; i < varNames.Length; i++) {
				Plugin plugin = plugins[typesCols[i]];
				code += plugin.GetTypeName(cells[1][i], configuration.className, sheetName, cells[0][i]) + " " + varNames[i];
				if (i < varNames.Length - 1) {
					code += ", ";
				}
			}
			code += "){";
			for (i = 0; i < varNames.Length; i++) {
				code += "\n\t\t\tthis." + varNames[i] + " = " + varNames[i] + ";";
			}
			code += "\n\t\t}";
			code += "\n\t}";
			return code;
		}
		private static string GetCode_Sheet(Configuration configuration, int idxPage, string sheetName, eType[] typesCols, string[][] cells, string updated) {
			int i;
			int j;
			string clsSheetName = GetName_ClassSheet(sheetName);
			string clsRowName = GetName_ClassRow(sheetName);
			string code = "public class " + clsSheetName + ": IEnumerable{";
			string date_A = updated.Split('T')[0];//2014-04-11
			string date_B = updated.Split('T')[1];//01:59:14.682Z
			date_B = date_B.Substring(0, date_B.IndexOf("."));
			int date_year = int.Parse(date_A.Split('-')[0]);
			int date_month = int.Parse(date_A.Split('-')[1]);
			int date_day = int.Parse(date_A.Split('-')[2]);
			int date_hour = int.Parse(date_B.Split(':')[0]);
			int date_minute = int.Parse(date_B.Split(':')[1]);
			int date_sec = int.Parse(date_B.Split(':')[2]);
			code += "\n\t\tpublic System.DateTime updated = new System.DateTime(" + date_year + "," + date_month + "," + date_day + "," + date_hour + "," + date_minute + "," + date_sec + ");";
			for (i = 0; i < cells[0].Length; i++) {
				cells[0][i] = cells[0][i].Replace("\n", "").Replace("\r", "").Trim();
			}
			code += "\n\t\tpublic readonly string[] labels = new string[]{\"" + string.Join("\",\"", cells[0]) + "\"};";
			string codeInitArray = "new " + clsRowName + "[]{";
			for (i = 0; i < cells.Length - 1; i++) {
				string[] columns = cells[i + 1];
				codeInitArray += "\n\t\t\t\t\tnew " + clsRowName + "(";
				for (j = 0; j < columns.Length; j++) {
					if (columns[j] == "#REF!") {
						Copy.copy.help_refError.ShowHelp(Help.eImage.refError, sheetName);
					}
					codeInitArray += GetVariable_Value(configuration.className, columns[j], cells[0][j], sheetName, typesCols[j]);
					if (j < columns.Length - 1) {
						codeInitArray += ",";
					}
				}
				codeInitArray += ")";
				if (i < cells.Length - 2) {
					codeInitArray += ",";
				}
			}
			codeInitArray += "\n\t\t\t\t};";
			code += StringUtilities.Format(@"
		private {0}[] _rows = new {0}[{1}];
		public void Init() {
			_rows = {2}
		}
			", clsRowName, cells.Length - 1, codeInitArray);
			string templateEnum = @"
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private {0} t;
			public SheetEnumerator({0} t){
				this.t = t;
			}
			public bool MoveNext(){
				if (idx < t._rows.Length - 1){
					idx++;
					return true;
				}else{
					return false;
				}
			}
			public void Reset(){
				idx = -1;
			}
			public object Current{
				get{
					return t._rows[idx];
				}
			}
		}";
			code += StringUtilities.Format(templateEnum, clsSheetName);
			code += @"
		public int Length{ get{ return _rows.Length; } }";
			code += StringUtilities.Format(@"
		public {0} this[int index]{
			get{
				return _rows[index];
			}
		}", 
			clsRowName);
			if (typesCols[0] == eType._string) {
				code += StringUtilities.Format(@"
		public {0} this[string id]{
			get{
				for (int i = 0; i < _rows.Length; i++) {
					if( _rows[i].{1} == id){ return _rows[i]; }
				}
				return null;
			}
		}", 
				clsRowName, GetName_Variable(cells[0][0]));
			}
			if (typesCols[0] == eType._string) {
				code += StringUtilities.Format(@"
		public bool ContainsKey(string key){
			for (int i = 0; i < _rows.Length; i++) {
				if( _rows[i].{0} == key){ return true; }
			}
			return false;
		}",
				GetName_Variable(cells[0][0]));
			}
			code += StringUtilities.Format(@"
		public {0}[] ToArray(){
			return _rows;
		}", 
			clsRowName);
			code += @"
		public " + clsRowName + @" Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}";
			code += @"
";
			List<string> vars = new List<string>();
			for (i = 0; i < cells.Length - 1; i++) {
				string[] row = cells[i + 1];
				string varName = GetName_Variable(StringUtilities.GetCamelCase(row[0]), vars.ToArray(), "labels", "_rows", "updated", "Length", "ToArray", "HasKey", "Random", "Reload", "Length", "GetEnumerator", "iCaroutineLoad");
                vars.Add(varName);
			}
			if (typesCols[0] == eType._string) {
				for (i = 0; i < cells.Length - 1; i++) {
					string varName = vars[i];
					code += "\n\t\tpublic " + clsRowName + " " + varName + "{\tget{ return _rows[" + i.ToString() + "]; } }";
				}
			}
			code += "\n";
			code += "\n\t}";
			return code;
		}
#endregion
#region Get Variable Type
		private static Dictionary<eType, Plugin> plugins = new Dictionary<eType, Plugin>() {
			{eType._Vector2,    new PluginVector2() },
			{eType._Vector3,    new PluginVector3() },
			{eType._bool,		new PluginBool() },
			{eType._int,        new PluginInt() },
			{eType._float,      new PluginFloat() },
			{ eType._string,     new PluginString() },
		};
		public static eType GetPluginType(string cell) {
			foreach(KeyValuePair<eType, Plugin> plugin in plugins) {
				if (plugin.Value.IsValid(cell)) {
					return plugin.Key;
				}
			}
			return eType._string;
		}
		public static eType GetTypeColumn(string sheetName, string columnName, string[] cells) {
			string[] columnNameArray = columnName.Split(new char[] { ' ' });
			if (columnNameArray.Length >= 2) {
				string compareColumnType = StringUtilities.GetCamelCase(columnNameArray[0]).ToLower();
				foreach (KeyValuePair<eType, Plugin> pair in plugins) {
					if (compareColumnType == pair.Value.columnKeyWord.ToLower()) {
						return pair.Key;
					}
				}
			}
			Dictionary<eType, int> typeCount = new Dictionary<eType, int>();
			int countTypeEmpty= 0;
			foreach (eType type in plugins.Keys) {
				typeCount[type] = 0;
			}
			foreach (string cell in cells) {
				if(cell != "") {
					typeCount[GetPluginType(cell)]++;
				} else {
					countTypeEmpty++;
				}
			}
			List<string> output = new List<string>();
			foreach(KeyValuePair<eType, int> set in typeCount) { 
				if(set.Value > 0) {
					output.Add( set.Key+" = "+set.Value );
				}
			}
			int countMaxCount = 0;
			eType countMaxType = eType._string;
			foreach (KeyValuePair<eType, int> set in typeCount) {
				if (set.Value > countMaxCount) {
					countMaxCount = set.Value;
					countMaxType = set.Key;
				}
				if (set.Value == (cells.Length - countTypeEmpty) && set.Value > 0) {//COULD MAP CLEARLY
					return set.Key;
				}
			}
			if (countMaxType == eType._float) {
				if (typeCount[eType._string] == 0 && typeCount[eType._bool] == 0) {
					return eType._float;
				}
			} else if (countMaxType == eType._int) {
				if (typeCount[eType._string] == 0 && typeCount[eType._bool] == 0) {
					if (typeCount[eType._float] > 0) {//ONE OF THEM IS A FLOAT, CONVERT ALL INTEGERS TO FLOATS
						return eType._float;
					} else {
						return eType._int;
					}
				}
			}
			return eType._string;
		}
		public static string GetVariable_Value(string className, string cell, string columnName, string sheetName, eType type) {
			foreach(KeyValuePair<eType, Plugin> plugin in plugins) {
				if(type == plugin.Key) {
					return plugin.Value.GetValue(cell, className, sheetName, columnName);
				}
			}
			return null;
		}
#endregion
#region Helpers
		public static string GetName_ClassRow(string title) {
			string n = StringUtilities.GetCamelCase(title);
			if (n.Contains(":")) {
				n = n.Substring(0, n.IndexOf(":"));
			}
			n = n.Substring(0, 1).ToUpper() + "" + n.Substring(1);
			return n;
		}
		private static string GetName_ClassSheet(string title) {
			string n = StringUtilities.GetCamelCase(title);
			if (n.Contains(":")) {
				n = n.Substring(0, n.IndexOf(":"));
			}
			n = n.Substring(0, 1).ToUpper() + "" + n.Substring(1);
			return "Sheet" + n;
		}
		public static string GetName_ClassSheetNameSpace(string className) {
			return StringUtilities.GetPascalCase(className) + "Types";
		}
		public static string GetName_Variable(string inp) {
			return GetName_Variable(inp, new string[] { });
		}
		private static string GetName_Variable(string inp, string[] existingNames, params string[] additionalExistingVars) {
			int i;
			inp = System.Text.RegularExpressions.Regex.Replace(inp, @"\t|\n|\r", "").Trim();
			if (inp == "") {//Can't be empty
				inp = "unkown";
			}
			string[] columnNameArray = inp.Split(new char[] { ' ' });
			if (columnNameArray.Length >= 2) {
				string compareColumnType = StringUtilities.GetCamelCase(columnNameArray[0]).ToLower();
				foreach (KeyValuePair<eType, Plugin> pair in plugins) {
					if (compareColumnType == pair.Value.columnKeyWord.ToLower()) {
						inp = inp.Substring(inp.IndexOf(columnNameArray[0]) + columnNameArray[0].Length);
						inp = StringUtilities.GetCamelCase(inp);
						break;
					}
				}
			}
			inp = StringUtilities.GetCamelCase(inp);
			if (inp.Contains(":")) {
				inp = inp.Substring(0, inp.IndexOf(":"));
			}
			int parseTry;
			if (int.TryParse(inp.Substring(0, 1), out parseTry)) {
				inp = "_" + inp;
			}
			inp = Regex.Replace(inp, @"[^\u0000-\u007F]", string.Empty);
			char[] illegalChars = new char[] { '/', '\\', '#', '-', '.', '?', '\'', ',', '<', '>', ' ', '[', ']', '{', '}', '(', ')' };
			for (i = 0; i < illegalChars.Length; i++) {
				inp = inp.Replace(illegalChars[i].ToString(), "");
			}
			string[] protectedVars = new string[] { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "descending", "dynamic", "from", "get", "global", "group", "into", "join", "let", "orderby", "partial", "partial", "remove", "select", "value", "var", "where", "yield"};
			if (additionalExistingVars.Length > 0) {
				string[] protectedVarsCopy = new string[protectedVars.Length + additionalExistingVars.Length];
				protectedVars.CopyTo(protectedVarsCopy, 0);
				additionalExistingVars.CopyTo(protectedVarsCopy, protectedVars.Length);
				protectedVars = protectedVarsCopy;
			}
			for (i = 0; i < protectedVars.Length; i++) {
				if (inp == protectedVars[i]) {
					inp = "_" + inp;
					break;
				}
			}
			if (System.Array.IndexOf<string>(existingNames, inp) == -1) {
				return inp;
			} else {
				for (i = 0; i < existingNames.Length; i++) {
					int nr = (i + 1);
					string newName = inp + "" + nr.ToString("D2");
					if (System.Array.IndexOf<string>(existingNames, newName) == -1) {
						return newName;
					}
				}
			}
			return inp;
		}
		private static EditorWindow window;
		public static EditorWindow Window {
			get {
				if(window == null) {
					window = (EditorWindow)EditorWindow.GetWindow<MetaSheets>();
				}
				return window;
			}
		}
#endregion
		private static string[] columnNames = ("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,AA,AB,AC,AD,AE,AF,AG,AH,AI,AJ,AK,AL,AM,AN,AO,AP,AQ,AR,AS,AT,AU,AV,AW,AX,AY,AZ,BA,BB,BC,BD,BE,BF,BG,BH,BI,BJ,BK,BL,BM,BN,BO,BP,BQ,BR,BS,BT,BU,BV,BW,BX,BY,BZ,CA,CB,CC,CD,CE,CF,CG,CH,CI,CJ,CK,CL,CM,CN,CO,CP,CQ,CR,CS,CT,CU,CV,CW,CX,CY,CZ").Split(new char[] { ',' });
		private static int[] GetColumnRow(string id) {
			string A = Regex.Replace(id, @"[\d]", "");//Alphabetical part
			string B = Regex.Replace(id, @"[^\d]", "");//Row index
			int idxCol = System.Array.IndexOf<string>(columnNames, A);
			int idxRow = System.Convert.ToInt32(B) - 1;//Starts at 0
			return new int[2] { idxCol, idxRow };
		}
	}
}

