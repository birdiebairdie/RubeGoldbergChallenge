using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineObject : MonoBehaviour {
	// on collision between the ball and the trampoline, needs to add force or just change ball's instantaneous velocity by (0, -1, 0) to go up instead of down
	//assign this to the trampoline
	//set the collider to NOT isTrigger

	public float bounceForce = 7f;
	private Rigidbody ball;

	void OnCollisionEnter (Collision collision) { //when the Ball hits the Trampoline
		if (collision.gameObject.CompareTag ("Ball")) {
			collision.collider.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * bounceForce, ForceMode.Impulse); // "bounce" the Ball up, not changing the other transforms
		}
	}
}
