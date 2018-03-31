using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanObject : MonoBehaviour {

	//needs to send out a "push" that only affects the ball object
	//raycast?
	//varying force outwards depending on ball's distance from fan when it triggers a raycast hit (which will only happen straight out from fan)
	public float windForce = -12f;
	private Rigidbody ball;

	void OnTriggerStay (Collider collider) { //as long as the Ball remains in the capsule collider in front of the fan
		if (collider.gameObject.CompareTag ("Ball")) { // if the object in the trigger is the Ball
			collider.GetComponent<Rigidbody>().AddForce(transform.forward * windForce, ForceMode.Acceleration); // add a force "blowing" away from the face of the fan
		}
	}
}
