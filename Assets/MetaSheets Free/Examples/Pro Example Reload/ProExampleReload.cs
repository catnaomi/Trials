using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ProExampleReload : MonoBehaviour {
	public Transform transformCube;
	public Transform transformSphere;
	public Transform transformCylinder;
	public Material material;
	void Start() {
		transform.Find("btnReload").GetComponent<Button>().onClick.AddListener(OnClickReload);
		transform.Find("btnViewSpreadsheet").GetComponent<Button>().onClick.AddListener(OnClickOpenSpreadsheet);
		//Assign individual Materials
		transformCube.GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
		transformSphere.GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
		transformCylinder.GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
	}
	#region Button Listeners
	/// <summary>
	/// When we clicked on the reload button
	/// </summary>
	private void OnClickReload() {
		DataReload.Reload(OnDataReloaded);
	}
	/// <summary>
	/// When we clicked on the open spreadsheet button
	/// </summary>
	private void OnClickOpenSpreadsheet() {
		Application.OpenURL("https://docs.google.com/spreadsheets/d/1XPvDNffDlmEywlRuRhTFSRuVVWZdoDNbxZ8JxUB1XcE/edit#gid=0");
	}
	#endregion
	/// <summary>
	/// Refreshes the UI list of elements to the latest Copy of the target Language
	/// </summary>
	private void OnDataReloaded() {
		Debug.Log("Data has been reloaded");
	}
	/// <summary>
	/// Called on every frame update
	/// </summary>
	void Update() {
		UpdateTransform(transformCube,		DataReload.objects.cube);
		UpdateTransform(transformSphere,	DataReload.objects.sphere);
		UpdateTransform(transformCylinder,	DataReload.objects.cylinder);
	}
	private void UpdateTransform(Transform transform, DataReloadTypes.Objects item) {
		//Rotate by sheet item property
		transform.Rotate(item.rotationVector * item.rotationSpeed * Time.deltaTime);
		//Material Color by sheet item property
		transform.GetComponent<MeshRenderer>().sharedMaterial.color = item.color;
	}
}

