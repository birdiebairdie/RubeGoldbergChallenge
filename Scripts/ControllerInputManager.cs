using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//This class manages all the control triggers for both the left-hand and right-hand controllers
public class ControllerInputManager : MonoBehaviour {

	//TO CHECK
	// * where does the teleport laser originate from? do we need to use the leftHand bool in top-level update to see which controller is being used? would necessitate changing control triggers throughout.
	// * similar question with swipe menu - if not anchored to the right hand, might break immersion

	//General------------------------------------------------------
	//public SteamVR_TrackedObject trackedObj;
	//public SteamVR_Controller.Device device;
	public OVRInput.Controller thisController;
	public bool leftHand; //set in the Inspector window in Unity
	//public GameObject handAnchor;
	public float triggerSensitivity = 0.1f;

	//Teleporting---------------------------------------------------
	private LineRenderer laser; //the line drawn between the left controller and the location we will teleport to
	public GameObject teleportAimerObject; //the object showing where we will teleport to
	public Vector3 teleportLocation; //the point to which we draw the laser, display the aimer object, and then teleport
	public GameObject player; //us!
	public LayerMask laserMask; //objects/terrain where we can teleport
	public float yNudgeAmount = 0; //specific to teleportAimerObject height
	private int laserOutRange = 15; //distance to send laser straight out
	private int laserDownRange = 17; //distance to send laser down if no hit straight out

	//Swiping-------------------------------------------------------
	private ObjectMenuManager objectMenuManager;
	private bool menuOn = false;
	private float menuStickX;
	private float menuStickY;
	private bool menuIsSwipeable;
	private bool objectsAreSpawnable;
	public float thumbstickSensitivity = 0.45f;

	//Grabbing/Throwing---------------------------------------------
	//public AudioClip hapticsAudioClip; // 341029_projectsu012_short-buzz (freesound.org)
	//private OVRHapticsClip hapticsClip;
	//private int hapticsChannel;
	public float throwForce = 1.5f;
	public bool ballInHand = false;

	void Start() {
		print ("Starting ControllerInputManager");
		//hapticsClip = new OVRHapticsClip(hapticsAudioClip);
	}

	void Update() {
		//check if the controller is LTouch or RTouch
		//if(leftHand) { //if LTouch
			//thisController = OVRInput.Controller.LTouch; //set the controller to L
			//hapticsChannel = 0; //sets the haptics channel to the left hand
			ManageTeleportation(); //check on teleportation controls being triggered and respond appropriately
		//}
		//else { //if RTouch
			//thisController = OVRInput.Controller.RTouch; //set the controller to R
			//hapticsChannel = 1; //sets the haptics channel to the right hand
			ManageSwiping(); //check on swiping controls and respond appropriately
		//}
	}
		
	void ManageTeleportation() { //top-level management for the different components of teleportation
		if(OVRInput.Get(OVRInput.RawButton.LThumbstick)) { //if the left-hand thumbstick is pressed and held down
			TeleportAim(); //display laser aimer and teleport location object
			//print("Aiming the teleport laser");
		}
		if(OVRInput.GetUp(OVRInput.RawButton.LThumbstick)) { //on the frame that the left-hand thumbstick is released
			Teleport();	//teleport the player to the chosen location
			//print("Teleported!");
		}
	}
		
	void TeleportAim() { //teleport location display system
		laser.gameObject.SetActive(true); //show the laser
		teleportAimerObject.gameObject.SetActive(true); //show the teleport location object at the correct location
		laser.SetPosition(0, gameObject.transform.position); //laser originates from this controller, i.e. the left-hand one
		RaycastHit hit; //declare the variable
		if (Physics.Raycast (transform.position, transform.forward, out hit, laserOutRange, laserMask)) { //if the laser hits an allowable surface by going straight out a set distance (laserOutRange, set in initial variables)
			teleportLocation = hit.point; //set the location for teleportation
			laser.SetPosition (1, teleportLocation); //set the end of the laser to the teleport location
			teleportAimerObject.transform.position = new Vector3 (teleportLocation.x, teleportLocation.y + yNudgeAmount, teleportLocation.z); //sets the position of the teleport aimer object to the teleportation location, raised by the amount set in the initial variables (yNudgeAmount) so the object does not hang through the floor
		}
		else { //if the laser doesn't hit anything straight out, go straight down a set amount (laserDownRange in initial variables) from the end to look for an allowable surface
			teleportLocation = transform.position + transform.forward * laserOutRange; //first setting the teleport location to be straight out from the controller
			RaycastHit groundRay; //declare the variable
			if(Physics.Raycast(teleportLocation, Vector3.down, out groundRay, laserDownRange, laserMask)) { //then casting another ray straight down from that to check for allowable surfaces
				teleportLocation = new Vector3 (transform.forward.x * laserOutRange + transform.position.x, groundRay.point.y, transform.forward.z * laserOutRange + transform.position.z);
			}
			laser.SetPosition (1, transform.forward * laserOutRange + transform.position); //set the end of the laser to the new teleport location
			teleportAimerObject.transform.position = teleportLocation + new Vector3 (0, yNudgeAmount, 0); //set the position of the teleport aimer object, taking care to raise it up if needed
		}
	}
		
	void Teleport() { //instantly move the player to the chosen location
		laser.gameObject.SetActive(false); //turn off the teleport indicator laser
		teleportAimerObject.gameObject.SetActive(false); //turn off the teleport aimer object
		player.transform.position = teleportLocation; //move the player object to the set teleport location
	}
		
	void ManageSwiping() { //top-level management for the object menu
		if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick)){ //if the right-hand thumbstick is pressed
			menuOn = !menuOn; //toggle the menu boolean
			objectMenuManager.ToggleMenu (menuOn); //toggle the menu (i.e. activate the current object if the menu was off, and deactivate it if the menu was on)
		}
		menuStickX = OVRInput.Get (OVRInput.RawAxis2D.RThumbstick).x; //sets the variable for the x position of the right-hand thumbstick
		menuStickY = OVRInput.Get (OVRInput.RawAxis2D.RThumbstick).y; //sets the variable for the y position of the right-hand thumbstick
		if (menuStickX < thumbstickSensitivity && menuStickX > -thumbstickSensitivity) { //if the thumbstick is centered in the x direction
			menuIsSwipeable = true; //we can swipe the menu
		}
		if (menuStickY < thumbstickSensitivity && menuStickY > -thumbstickSensitivity) { //if the thumbstick is centered in the y direction
			objectsAreSpawnable = true; //we can spawn objects
		}
		if (menuOn && menuIsSwipeable && objectsAreSpawnable) { //if the object menu is on and the thumbstick is centered in both the x and y directions
			if(menuStickX <= -thumbstickSensitivity) { //if the right thumbstick is pushed left past a certain point
				objectMenuManager.MenuLeft(); //swipe left
				menuIsSwipeable = false; //reset the swipeability of the menu until the thumbstick is centered
			}

			if(menuStickX > thumbstickSensitivity) {//if the right thumbstick is pushed right past a certain point
				objectMenuManager.MenuRight(); //swipe right
				menuIsSwipeable = false; //reset the swipeability of the menu until the thumbstick is centered
			}

			if (menuStickY > thumbstickSensitivity) { //if the right thumbstick is pushed up
				objectMenuManager.SpawnCurrentObject(); //spawn the current object
				objectsAreSpawnable = false; //reset spawning ability until the thumbstick is centered
			}
		}
	}
	
		
	void OnTriggerStay(Collider collider)
	{
		if(collider.gameObject.CompareTag("Ball")) { // if there is a collision with the controller's collider and an object tagged "Ball" (this object will be thrown/roll/fall to the ground realistically)
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, thisController) > triggerSensitivity)  { // if the object is grabbed via hand trigger
				//&& (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, thisController) > triggerSensitivity) // AND index trigger
				//&& (OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, thisController))) { // AND thumb rest is touched
				GrabBall(collider); //pick up the ball
				ballInHand = true; //player is holding the ball
				//print("Player is holding the Ball");
			}
				
				if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, thisController) < triggerSensitivity) { // if the object is released via hand trigger
				//|| (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, thisController) < triggerSensitivity)) { // OR index trigger (ignoring thumb rest)
				ThrowBall(collider); //throw the ball
				ballInHand = false; //we are no longer holding the ball
				//print("Ball has been thrown");
			}
		}
			
		if(collider.gameObject.CompareTag("RGObject")) { //if there is a collision with the controller's collider and an object tagged "RGObject" (these objects will hang in the air)
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, thisController) > triggerSensitivity) { // if the object is grabbed via hand trigger
			//&& (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, thisController) > triggerSensitivity) // AND index trigger
			//&& (OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, thisController))) { // AND thumb rest is touched
			GrabObject(collider); // pick up the object
			//print ("Player is holding an Object");
			}
		}

		if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, thisController) < triggerSensitivity) { // if the object is released via hand trigger
			//|| (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, thisController) < triggerSensitivity)) { // OR index trigger
			ReleaseObject(collider); // release the object
		}

	}
		
	void GrabBall (Collider ball) { //picks up an object tagged "Throwable"
		ball.transform.SetParent(gameObject.transform); //set the controller as the parent of the throwable object so it moves with the controller
		ball.GetComponent<Rigidbody>().isKinematic = true; //turn off physics
		//OVRHaptics.Channels[hapticsChannel].Preempt(hapticsClip); //sends a buzz to the correct controller

	}
		
	void ThrowBall(Collider ball) { //throws an object tagged "Throwable"
		ball.transform.SetParent(null); //unparents the object so it has free movement
		Rigidbody rigidbody = ball.GetComponent<Rigidbody>(); //declare the variable
		rigidbody.isKinematic = false; //turn on physics
		rigidbody.velocity = OVRInput.GetLocalControllerVelocity(thisController) * throwForce; //give the object the velocity of the controller, tempered by the variable throwForce
		rigidbody.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(thisController); //give the object the angilar velocity of the controller
		//OVRHaptics.Channels[hapticsChannel].Clear(); //turn off all haptic feedback for this controller
	}
		
	void GrabObject (Collider rgo) { //picks up an object tagged "RGObject"
		rgo.transform.SetParent(gameObject.transform); //sets the controller as the parent of the moveable object
		//note: not changing isKinematic because these objects ignore physics to hang in the air
		//OVRHaptics.Channels[hapticsChannel].Preempt(hapticsClip); //sends a buzz to the correct controller
	}
		
	void ReleaseObject(Collider rgo) { //releases an object tagged "RGObject"
		rgo.transform.SetParent(null); //unparents the object
		//note: not changing isKinematic because these object ignore physics to hang in the air
		//OVRHaptics.Channels[hapticsChannel].Clear(); //turn off all haptic feedback for this controller
	}
}