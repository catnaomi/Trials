using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DataAudio{
	//Sheet SheetSounds
	public static DataAudioTypes.SheetSounds sounds = new DataAudioTypes.SheetSounds();
	static DataAudio(){
		//Static constructor that init each sheet data
		sounds.Init(); 
	}
}
namespace DataAudioTypes{
	public class Sounds:DAudio{
		public string id;
		public Sounds(){}
		public Sounds(string id, float volume, string[] clipNames){
			this.id = id;
			this.volume = volume;
			this.clipNames = clipNames;
		}
	}
	public class SheetSounds: IEnumerable{
		//Owner:	
		public System.DateTime updated = new System.DateTime(2016,8,16,10,28,6);
		public string[] labels = new string[]{"id","volume","clipNames"};
		private Sounds[] _rows = new Sounds[6];
		public void Init() {
			_rows = new Sounds[]{
					new Sounds("blib",1f,new string[]{"sfx_blib"}),
					new Sounds("plop",1f,new string[]{"sfx_plop"}),
					new Sounds("wush",1f,new string[]{"sfx_wush"}),
					new Sounds("steps",0.9f,new string[]{"step_03","step_04","step_05","step_06","step_07","step_01","step_02"}),
					new Sounds("metals",1f,new string[]{"metal_03","metal_04","metal_05","metal_01","metal_02"}),
					new Sounds("woods",1f,new string[]{"wood_03","wood_04","wood_05","wood_06","wood_01","wood_02"})
				};
		}
		public IEnumerator GetEnumerator(){
			return new SheetEnumerator(this);
		}
		private class SheetEnumerator : IEnumerator{
			private int idx = -1;
			private SheetSounds t;
			public SheetEnumerator(SheetSounds t){
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
		public Sounds this[int index]{
			get{
				return _rows[index];
			}
		}
		/// <summary>
		/// Access row item by first culumn string identifier
		/// </summary>
		public Sounds this[string id]{
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
		public Sounds[] ToArray(){
			return _rows;
		}
		/// <summary>
		/// Random item
		/// </summary>
		/// <returns>Returns a random item.</returns>
		public Sounds Random() {
			return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
		}
		//Specific Items
		public Sounds blib{	get{ return _rows[0]; } }
		public Sounds plop{	get{ return _rows[1]; } }
		public Sounds wush{	get{ return _rows[2]; } }
		public Sounds steps{	get{ return _rows[3]; } }
		public Sounds metals{	get{ return _rows[4]; } }
		public Sounds woods{	get{ return _rows[5]; } }
	}
}
