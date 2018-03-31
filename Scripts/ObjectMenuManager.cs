using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMenuManager : MonoBehaviour {

	public List<GameObject> objectList;
	public List<GameObject> objectPrefabList;
	public int currentObject = 0;

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			objectList.Add (child.gameObject);
		}
	}

	public void ToggleMenu(bool menuOn){
		objectList [currentObject].SetActive (menuOn);
	}

	public void MenuLeft() {
		objectList [currentObject].SetActive (false); //disable whatever menu object is currently showing
		currentObject--; //change the number of the current list item appropriately
		if(currentObject < 0) { //if this means the number of the current list item doesn't exist because it's -1
			currentObject = objectList.Count - 1; //reset the number of the current list item to the last item in the list (remember, count starts at 0, not 1)
		}
		objectList [currentObject].SetActive (true); //enable the new menu object
	}

	public void MenuRight() {
		objectList [currentObject].SetActive (false); //disable whatever menu object is currently showing
		currentObject++; //change the number of the current list item appropriately
		if(currentObject > objectList.Count - 1) { //if this means the number of the current list item doesn't exist because it's greater than the largest list item number
			currentObject = 0; //reset the number of the current list item to the first item in the list (remember, count starts at 0, not 1)
		}
		objectList [currentObject].SetActive (true); //enable the new menu object
	}

	public void SpawnCurrentObject() {
		Vector3 spawnPosition = objectList [currentObject].transform.position;
		Quaternion spawnRotation = objectList [currentObject].transform.rotation;
		Vector3 spawnScale = new Vector3 (100, 100, 100);
		GameObject newObject = Instantiate (objectPrefabList [currentObject], spawnPosition, spawnRotation);
		newObject.transform.localScale = spawnScale;
	}
}
