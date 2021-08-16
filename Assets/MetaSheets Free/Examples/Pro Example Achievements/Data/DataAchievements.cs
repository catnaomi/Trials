using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DataAchievements{
	//Sheet SheetAchievements
	public static DataAchievementsTypes.SheetAchievements achievements = new DataAchievementsTypes.SheetAchievements();
	static DataAchievements(){
		//Static constructor that init each sheet data
		achievements.Init(); 
	}
}
namespace DataAchievementsTypes{
	public class Achievements:DAchievement{
		public Achievements(){}
		public Achievements(string id, int count, string message, string idIOS, string idAndroid){
			this.id = id;
			this.count = count;
			this.message = message;
			this.idIOS = idIOS;
			this.idAndroid = idAndroid;
		}
	}
	public class SheetAchievements: IEnumerable{
		//Owner:	
		public System.DateTime updated = new System.DateTime(2016,8,16,11,32,0);
		public string[] labels = new string[]{"id","count","message","id IOS","id Android"};
		private Achievements[] _rows = new Achievements[4];
		public void Init() {
			_rows = new Achievements[]{
					new Achievements("click30",30,"Congratulations, You have clicked 30x times anywhere","achievement_click30","fMUwhMiwPi79QYrb"),
					new Achievements("clickRed",1,"Congratulations, You have clicked the red button","achievement_clickRed","0slffGA4GCc9KH5c"),
					new Achievements("wait10Sec",10,"Congratulations, You have waited 10 seconds","achievement_wait10Sec","s25JALtKUF2aIQH7"),
					new Achievements("pressKeyU",1,"You have pressed the secret U key","achievement_pressKeyU","YN27j6r4jfbxEhAI")
				};
		}
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private SheetAchievements t;
			public SheetEnumerator(SheetAchievements t){
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
		public Achievements this[int index]{
			get{
				return _rows[index];
			}
		}
		/// <summary>
		/// Access row item by first culumn string identifier
		/// </summary>
		public Achievements this[string id]{
			get{
				for (int i = 0; i < _rows.Length; i++) {
					if( _rows[i].id == id){ return _rows[i]; }
				}
				return null;
			}
		}
		/// <summary>
		/// Does an item exist with the following key?
		/// </summary>
		public bool ContainsKey(string key){
			for (int i = 0; i < _rows.Length; i++) {
				if( _rows[i].id == key){ return true; }
			}
			return false;
		}
		/// <summary>
		/// List of items
		/// </summary>
		/// <returns>Returns the internal array of items.</returns>
		public Achievements[] ToArray(){
			return _rows;
		}
		/// <summary>
		/// Random item
		/// </summary>
		/// <returns>Returns a random item.</returns>
		public Achievements Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}
		//Specific Items
		public Achievements click30{	get{ return _rows[0]; } }
		public Achievements clickRed{	get{ return _rows[1]; } }
		public Achievements wait10Sec{	get{ return _rows[2]; } }
		public Achievements pressKeyU{	get{ return _rows[3]; } }
	}
}
