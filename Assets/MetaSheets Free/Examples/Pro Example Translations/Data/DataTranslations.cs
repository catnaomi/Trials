using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DataTranslations{
	//Sheet SheetCopy
	public static DataTranslationsTypes.SheetCopy copy = new DataTranslationsTypes.SheetCopy();
	static DataTranslations(){
		//Static constructor that init each sheet data
		copy.Init(); 
	}
}
namespace DataTranslationsTypes{
	public class Copy:DCopy{
		public Copy(){}
		public Copy(string scenePath, string en, string fr, string de, string es){
			this.scenePath = scenePath;
			this.en = en;
			this.fr = fr;
			this.de = de;
			this.es = es;
		}
	}
	public class SheetCopy: IEnumerable{
		//Owner:	
		public System.DateTime updated = new System.DateTime(2016,8,10,8,54,4);
		public string[] labels = new string[]{"string scenePath","en","fr","de","es"};
		private Copy[] _rows = new Copy[11];
		public void Init() {
			_rows = new Copy[]{
					new Copy("groupLanguages/btnEnglish/Text","English","Anglais","Englisch","Inglés"),
					new Copy("groupLanguages/btnFrench/Text","French","français","Französisch","francés"),
					new Copy("groupLanguages/btnGerman/Text","German","allemand","Deutsche","alemán"),
					new Copy("groupLanguages/btnSpanish/Text","Spanish","Espanol","Spanisch","Español"),
					new Copy("btnStart/Text","Start","Démarrer","Anfang","comienzo"),
					new Copy("btnOptions/Text","Options","options de","Optionen","opciones"),
					new Copy("btnMoreGames/Text","More Games","Plus de jeux","Mehr Spiele","Más juegos"),
					new Copy("sliderMusic/Text","Music Volume","Volume de la musique","Musiklautstärke","Volumen de la música"),
					new Copy("sliderSfx/Text","Sound Effects Volume","Volume des effets sonores","Lautstärke Toneffekte","Efectos de sonido Volumen"),
					new Copy("btnMute/Text","Mute","Muet","Stumm","Mudo"),
					new Copy("txtBody","This example demonstrates how to switch languages in an app using MetaSheets generated data with a base class \"DCopy\". By extending to a base additional logic such as switching the language can be tied to the data.\n\nLook inside the DCopy class to see how getters and additional methods were used.","Cet exemple montre comment changer de langue dans une application utilisant MetaSheets données générées avec une classe de base \"dcopy\". En étendant à une logique additionnelle de base tels que la commutation de la langue peut être liée aux données.\n\nRegardez à l'intérieur de la classe dcopy pour voir comment les accesseurs et d'autres méthodes ont été utilisées.","Dieses Beispiel zeigt, wie die Sprache zu wechseln in einer App MetaSheets erzeugten Daten mit einer Basisklasse \"Dcopy\" verwenden. Durch die Erweiterung der Sprache auf eine Basis zusätzliche Logik wie Schalten kann auf die Daten gebunden werden.\n\nSehen Sie in den Dcopy Klasse zu sehen, wie Getter und zusätzliche Methoden verwendet wurden.","Este ejemplo muestra cómo cambiar de idioma a una aplicación usando la MetaSheets datos generados con una clase base \"DCOPY\". Al extender a una base lógica adicional como cambiar el idioma puede ser atado a los datos.\n\nMire dentro de la clase DCOPY para ver cómo se utilizaron los captadores y métodos adicionales.")
				};
		}
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private SheetCopy t;
			public SheetEnumerator(SheetCopy t){
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
		public Copy this[int index]{
			get{
				return _rows[index];
			}
		}
		/// <summary>
		/// Access row item by first culumn string identifier
		/// </summary>
		public Copy this[string id]{
			get{
				for (int i = 0; i < _rows.Length; i++) {
					if( _rows[i].scenePath == id){ return _rows[i]; }
				}
				return null;
			}
		}
		/// <summary>
		/// List of items
		/// </summary>
		/// <returns>Returns the internal array of items.</returns>
		public Copy[] ToArray(){
			return _rows;
		}
		/// <summary>
		/// Random item
		/// </summary>
		/// <returns>Returns a random item.</returns>
		public Copy Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}
		//Specific Items
		public Copy groupLanguagesbtnEnglishText{	get{ return _rows[0]; } }
		public Copy groupLanguagesbtnFrenchText{	get{ return _rows[1]; } }
		public Copy groupLanguagesbtnGermanText{	get{ return _rows[2]; } }
		public Copy groupLanguagesbtnSpanishText{	get{ return _rows[3]; } }
		public Copy btnStartText{	get{ return _rows[4]; } }
		public Copy btnOptionsText{	get{ return _rows[5]; } }
		public Copy btnMoreGamesText{	get{ return _rows[6]; } }
		public Copy sliderMusicText{	get{ return _rows[7]; } }
		public Copy sliderSfxText{	get{ return _rows[8]; } }
		public Copy btnMuteText{	get{ return _rows[9]; } }
		public Copy txtBody{	get{ return _rows[10]; } }
	}
}
