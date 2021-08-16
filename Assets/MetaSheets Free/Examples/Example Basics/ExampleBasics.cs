using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
public class ExampleBasics : MonoBehaviour {
	void Start() {
		transform.Find("btnViewSpreadsheet").GetComponent<Button>().onClick.AddListener(OnClickOpenSpreadsheet);
		//Example Buttons
		transform.Find("btnStaticExampleA").GetComponent<Button>().onClick.AddListener(OnClickStaticExampleA);
		transform.Find("btnStaticExampleB").GetComponent<Button>().onClick.AddListener(OnClickStaticExampleB);
		transform.Find("btnStaticExampleC").GetComponent<Button>().onClick.AddListener(OnClickStaticExampleC);
		//Hide Tooltip container
		transform.Find("toolTip").gameObject.SetActive(false);
		//Basic info 
		Debug.Log("Items length: " + DataBasics.items.Length);
		Debug.Log("Item types: " + DataBasics.items[0].GetType() );
		Debug.Log("ContainsKey('item_Candy')? " + DataBasics.items.ContainsKey("item_Candy"));
		//Accessing items
		Debug.Log("Random item: " + DataBasics.items.Random().name);
		Debug.Log("First item: " + DataBasics.items.ToArray().First().name);
		Debug.Log("Specific item: " + DataBasics.items.item_Apple.name);
		Debug.Log("Array accessed item: " + DataBasics.items[2].name);
		Debug.Log("Key accessed item: " + DataBasics.items["item_Candy"].name);
		//Foreach loop access
		foreach(DataBasicsTypes.Items item in DataBasics.items) {
			Debug.Log("Foreach: " + item.id);
		}
		//for i++ loop access
		for (int i = 0; i < DataBasics.items.Length; i++) {
			Debug.Log("for i++: ["+i+"] = "+DataBasics.items[i].id);
		}
		PopuplateThumbnails();
	}
	/// <summary>
	/// Populates the invenotry UI with thumbails each representing a data entry of the sheet
	/// </summary>
	private void PopuplateThumbnails() {
		//The reference gameObject to instanciate
		GameObject gameObjectTemplate = transform.Find("groupItems/item0").gameObject;
		//Create thumbnail objects for each data Item in DataBasics.items
		foreach (DataBasicsTypes.Items item in DataBasics.items) {
			GameObject go = GameObject.Instantiate(gameObjectTemplate);
			go.transform.SetParent(gameObjectTemplate.transform.parent);
			go.transform.localScale = Vector3.one;
			//Assign name and cost for the thumbnail
			go.transform.Find("txtName").GetComponent<Text>().text = item.name;
			go.transform.Find("txtCost").GetComponent<Text>().text = "$" + item.cost;
			//Assign icons based on weapon or food
			go.transform.Find("imgWeapon").GetComponent<Image>().enabled = item.isWeapon;
			go.transform.Find("imgFood").GetComponent<Image>().enabled = !item.isWeapon;
			//Setup button actions
			Button button = go.transform.Find("btn").GetComponent<Button>();
			DataBasicsTypes.Items parameter = item;//Create a variable with a pointer inside the {} loop
			button.onClick.AddListener(() => {
				//Assign a Lambda expression (Annomynous function) to call the OnClickThumbnail call with the parameter object
				OnClickItem(parameter, go.transform);
			});
		}
		//Disable template gameObject
		gameObjectTemplate.SetActive(false);
	}
	#region Button Actions
	/// <summary>
	/// Display a tooltip to a particular item above a transformItem
	/// </summary>
	/// <param name="item">The DataBasics data item</param>
	/// <param name="transformItem">the transform of the button or thumbnail</param>
	private void OnClickItem(DataBasicsTypes.Items item, Transform transformItem) {
		transform.Find("toolTip").gameObject.SetActive(true);
		transform.Find("toolTip").transform.position = transformItem.position;
		//Display the description text
		transform.Find("toolTip/txt").GetComponent<Text>().text = item.description;
	}
	/// <summary>
	/// When we clicked on the open spreadsheet button
	/// </summary>
	private void OnClickOpenSpreadsheet() {
		Application.OpenURL("https://docs.google.com/spreadsheets/d/1aV_DzbS24DuXCo1Wm1iUVncydE9k08-msWBizWAyUiU/edit#gid=0");
	}
	private void OnClickStaticExampleA() {
		OnClickItem(DataBasics.items.item_Candy, transform.Find("btnStaticExampleA") );
	}
	private void OnClickStaticExampleB() {
		OnClickItem(DataBasics.items.item_Taser, transform.Find("btnStaticExampleB"));
	}
	private void OnClickStaticExampleC() {
		OnClickItem(DataBasics.items.item_Knife, transform.Find("btnStaticExampleC"));
	}
	#endregion
}

