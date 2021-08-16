using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DataBasics{
	//Sheet SheetItems
	public static DataBasicsTypes.SheetItems items = new DataBasicsTypes.SheetItems();
	static DataBasics(){
		//Static constructor that init each sheet data
		items.Init(); 
	}
}
namespace DataBasicsTypes{
	public class Items{
		public string id;
		public string name;
		public int cost;
		public bool isWeapon;
		public string description;
		public Items(){}
		public Items(string id, string name, int cost, bool isWeapon, string description){
			this.id = id;
			this.name = name;
			this.cost = cost;
			this.isWeapon = isWeapon;
			this.description = description;
		}
	}
	public class SheetItems: IEnumerable{
		//Owner:	
		public System.DateTime updated = new System.DateTime(2016,8,16,12,58,14);
		public string[] labels = new string[]{"id","name","cost","isWeapon","description"};
		private Items[] _rows = new Items[7];
		public void Init() {
			_rows = new Items[]{
					new Items("item_Gun","Gun",700,true,"A gun from the 60's, holds 7 rounds."),
					new Items("item_Knife","Knife",60,true,"A rusty old knife."),
					new Items("item_Apple","Apple",5,false,"Green apples from the farmer."),
					new Items("item_Candy","Candy",8,false,"Chewing candy with a purple color."),
					new Items("item_Magazine","Magazine",12,false,"A car magazine from last year."),
					new Items("item_Carrot","Carrot",4,false,"A dutch carrot that provides vitamins."),
					new Items("item_Taser","Taser",90,true,"A taser weapon to stun targets.")
				};
		}
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private SheetItems t;
			public SheetEnumerator(SheetItems t){
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
		public Items this[int index]{
			get{
				return _rows[index];
			}
		}
		/// <summary>
		/// Access row item by first culumn string identifier
		/// </summary>
		public Items this[string id]{
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
		public Items[] ToArray(){
			return _rows;
		}
		/// <summary>
		/// Random item
		/// </summary>
		/// <returns>Returns a random item.</returns>
		public Items Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}
		//Specific Items
		public Items item_Gun{	get{ return _rows[0]; } }
		public Items item_Knife{	get{ return _rows[1]; } }
		public Items item_Apple{	get{ return _rows[2]; } }
		public Items item_Candy{	get{ return _rows[3]; } }
		public Items item_Magazine{	get{ return _rows[4]; } }
		public Items item_Carrot{	get{ return _rows[5]; } }
		public Items item_Taser{	get{ return _rows[6]; } }
	}
}
