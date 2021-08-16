using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSONMetaSheets;
public class DataReload_load : MonoBehaviour{
	public static void Load(GameObject go, int idx, string label, IEnumerator ieNumerator){
		DataReload_load cmp = go.AddComponent<DataReload_load>();
		cmp.label = label;
		cmp.idx = idx;
		cmp.StartCoroutine( ieNumerator );
	}
	public string label;
	public int idx;
	public void OnGUI(){
		UnityEngine.GUIStyle style = new GUIStyle("label");
		style.fontSize = 42;
		UnityEngine.GUI.color = Color.yellow;
		UnityEngine.GUI.Label(new Rect( 0,idx*42f, Screen.width,Screen.height),"Loading "+label+"", style);
	}
}
public class DataReload{
	//Sheet SheetObjects
	public static DataReloadTypes.SheetObjects objects = new DataReloadTypes.SheetObjects();
	static DataReload(){
		//Static constructor that init each sheet data
		objects.Init(); 
	}
	public static void Reload(System.Action onCallLoaded){
		int countLoaded = 0;
		objects.Reload(()=>{countLoaded++;if(countLoaded==1 && onCallLoaded != null){onCallLoaded();}});
	}
}
namespace DataReloadTypes{
	public class Objects{
		public string id;
		public Color color;
		public Vector3 rotationVector;
		public float rotationSpeed;
		public Objects(){}
		public Objects(string id, Color color, Vector3 rotationVector, float rotationSpeed){
			this.id = id;
			this.color = color;
			this.rotationVector = rotationVector;
			this.rotationSpeed = rotationSpeed;
		}
	}
	public class SheetObjects: IEnumerable{
		//Owner:	
		public System.DateTime updated = new System.DateTime(2016,8,10,9,2,10);
		public string[] labels = new string[]{"id","Color color","Vector3 rotationVector","float rotationSpeed"};
		private Objects[] _rows = new Objects[3];
		public void Init() {
			_rows = new Objects[]{
					new Objects("cylinder",new Color(1f,0.1098039f,0.1098039f,1f),new Vector3(0f,0f,1f),40f),
					new Objects("cube",new Color(0.5803922f,0.9294118f,0.2745098f,1f),new Vector3(0f,1f,0f),30f),
					new Objects("sphere",new Color(0.2156863f,0.7254902f,0.9137255f,1f),new Vector3(1f,0f,0f),16f)
				};
		}
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private SheetObjects t;
			public SheetEnumerator(SheetObjects t){
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
		}
		/// <summary>
		/// Length of rows of this sheet
		/// </summary>
		public int Length{ get{ return _rows.Length; } }
		/// <summary>
		/// Access row item by index
		/// </summary>
		public Objects this[int index]{
			get{
				return _rows[index];
			}
		}
		/// <summary>
		/// Access row item by first culumn string identifier
		/// </summary>
		public Objects this[string id]{
			get{
				for (int i = 0; i < _rows.Length; i++) {
					if( _rows[i].id == id){ return _rows[i]; }
				}
				return null;
			}
		}
		/// <summary>
		/// List of items
		/// </summary>
		/// <returns>Returns the internal array of items.</returns>
		public Objects[] ToArray(){
			return _rows;
		}
		/// <summary>
		/// Random item
		/// </summary>
		/// <returns>Returns a random item.</returns>
		public Objects Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}
		//Specific Items
		public Objects cylinder{	get{ return _rows[0]; } }
		public Objects cube{	get{ return _rows[1]; } }
		public Objects sphere{	get{ return _rows[2]; } }
		/// <summary>
		/// Reload this sheet live from the server
		/// </summary>
		public void Reload(System.Action onCallLoaded){
			Debug.Log("Load Sheet \"objects\"");
			GameObject go = new GameObject("UnityReload");
			DataReload_load.Load(go, 0, "objects", iCaroutineLoad(go,"https://spreadsheets.google.com/feeds/cells/1XPvDNffDlmEywlRuRhTFSRuVVWZdoDNbxZ8JxUB1XcE/od6/public/basic?alt=json-in-script", onCallLoaded) );
		}
#region Reload
		private IEnumerator iCaroutineLoad(GameObject go, string url, System.Action onCallLoaded){
			WWW request = new WWW(url);
			yield return request;
			int i;int j;
			JSONNode entries = JSON.Parse( GetJson(request.text) )["feed"]["entry"];
			string[][] cells = new string[ entries.Count ][];
			for (i = 0; i < cells.Length; i++) {
				cells[i] = new string[ entries.Count ];
			}
			//LOAD TIME STAMP
			string updatedJson = JSON.Parse( GetJson(request.text) )["feed"]["updated"]["$t"].Value;
			string date_A 	= updatedJson.Split('T')[0];//2014-04-11
			string date_B 	= updatedJson.Split('T')[1];//01:59:14.682Z
			date_B = date_B.Substring(0, date_B.IndexOf("."));
			updated = new DateTime( int.Parse(date_A.Split('-')[0]), int.Parse(date_A.Split('-')[1]), int.Parse(date_A.Split('-')[2]), int.Parse(date_B.Split(':')[0]), int.Parse(date_B.Split(':')[1]), int.Parse(date_B.Split(':')[2]) );
			for (i = 0; i < entries.Count; i++) {
				string nTitle 	= entries[i]["title"]["$t"].Value;
				string nContent 	= entries[i]["content"]["$t"].Value;
				int[] colRow = GetColumnRow(nTitle);
				cells[ colRow[1] ][ colRow[0] ] = nContent;//ASSIGN VALUE
			}
			//STRIP EMPTY ROWS & COMMENTED ROWS
			List<string[]> keepRows = new List<string[]>();
			for (i = 0; i < cells.Length; i++) {
				if ( string.Join("",  cells[i] ) != "" && cells[i][0].IndexOf("//") != 0){//ALL COMBINED CELLS ARE NOT EMPTY
	                keepRows.Add( cells[i] );
				}
			}
			cells = new string[keepRows.Count][];
			for (i = 0; i < cells.Length; i++) {
				cells[i] = keepRows[i];
			}
			//STRIP COMMENTED COLUMNS
			List<int> keepColumns = new List<int>();
			for (i = 0; i < cells[0].Length; i++) {
				if ( cells[0][i] != null){
					if ( cells[0][i].IndexOf("//")!=0){
						keepColumns.Add( i );
					}
				}
			}
			if (keepColumns.Count != cells[0].Length){
				for (i = 0; i < cells.Length; i++) {
					List<string> row = new List<string>();
					for (j = 0; j < keepColumns.Count; j++) {
						row.Add( cells[i][ keepColumns[j] ] );
					}
					cells[i] = row.ToArray();
				}
			}
			_rows = new Objects[cells.Length-1];//DON'T INLCUDE FIRST ROW
			for (i = 0; i < _rows.Length; i++) {
				//NOW PARSE AND ASSIGN LOADED STRING VALUES
				_rows[i] = new DataReloadTypes.Objects();
				string[] columns = cells[(i+1)];
					for (j = 0; j < 4; j++) {
						if (columns[j] == null){
							columns[j] = "";
						}
					}
					//VARIABLE 'id' OF TYPE 'string'
					_rows[i].id = (columns[0] == null) ? "" : columns[0].Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\r","\n");
					//VARIABLE 'color' OF TYPE 'Color'
					Color tmp1;
					TryParseColor( CleanString(columns[1]) , out tmp1);
					_rows[i].color = tmp1;
					//VARIABLE 'rotationVector' OF TYPE 'Vector3'
					_rows[i].rotationVector = (columns[2].Contains(",") && columns[2].Split(',').Length ==3) ? new Vector3( float.Parse(columns[2].Split(',')[0]), float.Parse(columns[2].Split(',')[1]), float.Parse(columns[2].Split(',')[2]) ) : Vector3.zero;
					//VARIABLE 'rotationSpeed' OF TYPE 'float'
					float tmp3; _rows[i].rotationSpeed = float.TryParse( columns[3], out tmp3) ? tmp3 : 0f;
				}
				GameObject.Destroy(go);
				if (onCallLoaded!=null){
					onCallLoaded();
				}
			}
			//Converts a string to bool
			private bool ParseBool(string inp) {
				bool result;
				if (bool.TryParse(inp, out result)) {
					return result;
				}
				return false;
			}
			//Converts a string to float
			private float ParseFloat(string inp){
				float result;
				if (float.TryParse (inp, out result)) {
					return result;
				}
				return 0f;
			}
			//Converts a string to int
			private int ParseInt(string inp){
				int result;
				if (int.TryParse (inp, out result)) {
					return result;
				}
				return 0;
			}
				//SUPPORTS A-CZ = 104 COLUMNS
				private static string[] columnNames = ("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,AA,AB,AC,AD,AE,AF,AG,AH,AI,AJ,AK,AL,AM,AN,AO,AP,AQ,AR,AS,AT,AU,AV,AW,AX,AY,AZ,BA,BB,BC,BD,BE,BF,BG,BH,BI,BJ,BK,BL,BM,BN,BO,BP,BQ,BR,BS,BT,BU,BV,BW,BX,BY,BZ,CA,CB,CC,CD,CE,CF,CG,CH,CI,CJ,CK,CL,CM,CN,CO,CP,CQ,CR,CS,CT,CU,CV,CW,CX,CY,CZ").Split(new char[]{','});
				private int[] GetColumnRow(string id)
				{
					string A = System.Text.RegularExpressions.Regex.Replace(id, @"[\d]","");//THE ALHABETICAL PART
					string B = System.Text.RegularExpressions.Regex.Replace(id, @"[^\d]","");//THE ROW INDEX
					int idxCol = System.Array.IndexOf<string>( columnNames, A );
					int idxRow = System.Convert.ToInt32( B )-1;//STARTS AT 0
					return new int[2]{idxCol, idxRow};
				}
				private string GetJson(string inp){
					int A = inp.IndexOf("{");
					int B = inp.LastIndexOf("}");
					if (A != -1 && B != -1){
						return inp.Substring( A, B-A+1);
					}
					return inp;
				}
				private string GetCamelCase(string inp){
					if (inp == ""){
						return "";
					}
					string[] words = inp.Split(' ');
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					for (int i = 0; i < words.Length; i++) {
						string s = words[i];
						if (s.Length > 0){
							string firstLetter = s.Substring(0, 1);
							string rest = s.Substring(1, s.Length - 1);
							if (i==0){
								sb.Append(firstLetter + rest);//DON'T MODIFY FIRST CHARACTER
							}else{
								sb.Append(firstLetter.ToUpper() + rest);
							}
							sb.Append(" ");
						}
					}	
					return (sb.ToString().Substring(0, sb.ToString().Length - 1)).Replace(" ","");
				}
				private string CleanString(string inp){
					return CleanString(inp, new string[]{" ","\t","\n","\r"} );
				}
				private string CleanString(string inp, string[] remove){
					return System.Text.RegularExpressions.Regex.Replace( inp, string.Join("|", remove), "").Trim();
				}
				private bool TryParseColor(string input, out Color color){
					input = input.Trim();
					if (input.IndexOf("#") == 0){
						if (input.Length == 7 || input.Length == 9){
							string hex = input.Replace("#","");
							byte b0; 
							byte b1; 
							byte b2; 
							byte b3;
							if (byte.TryParse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber, null as System.IFormatProvider, out b0)){
								if (byte.TryParse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber, null as System.IFormatProvider, out b1)){
									if (byte.TryParse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber, null as System.IFormatProvider, out b2)){
										if (input.Length == 9){
											if (byte.TryParse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber, null as System.IFormatProvider, out b3)){
												color = new Color((float)b1/255f, (float)b2/255f, (float)b3/255f, (float)b0/255f);
												return true;
											}
										}else{
											color = new Color((float)b0/255f, (float)b1/255f, (float)b2/255f);
											return true;
										}
									}	
								}
							}
						}
					}
					color = Color.black;
					return false;
				}
	#endregion
	}
}
