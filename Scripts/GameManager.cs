using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//should include:
//		* scorekeeping
//		* anti-cheating
//		* level loads
//		* ball/star reset on a floor hit
//assign this script to the Ball object in the Unity Inspector
public class GameManager : MonoBehaviour {

	public int levelNumber;
	public List<GameObject> starList = new List<GameObject>();
	public List<GameObject> uncollectedStars = new List<GameObject>();
	public int totalStars; // the number of star objects in this scene
	public int score; // the score for the current level
	public int totalScore; // the score for the game up to this point
	public GameObject ballStartPosition;
	public GameObject playerStartPosition;
	public GameObject player;
	public GameObject ball;
	public GameObject system;
	public GameObject goal;
	public GameObject sparks; // the particle system prefab that plays when the ball hits a star or the goal
	//public GameObject loadingOverlay; // make this the PREFAB LoadingOverlay
	public LoadingOverlay overlay; 
	private bool cheating;
	public Material cheatingBallMAT; // material for the ball when cheating == true (turns red)
	public Material regularBallMAT; // material for the ball when cheating == false
	public AudioClip sceneIntroSound; // 353206_rhodesmas_intro-01 (freesound.org)
	public AudioClip starCollectSound; // 171671_fins_success-1 (freesound.org)
	public AudioClip goalHitSound; // 171670_fins_success-2 (freesound.org)
	public AudioClip floorHitSound; // light_bulb_smash (freesfx.co.uk)
	public AudioClip cheatingSound; // 342260_mooncubedesign_robot-voice-does-not-compute (freesound.org)
	//public AudioClip levelWinSound; // Ta Da-SoundBible.com-1881470640 this is just the goal hit sound, since ball only hits goal if the score is complete
	public AudioClip totalWinSound; // audience_clapping_whistling_and_cheering (freesfx.co.uk)
	public AudioClip backgroundMusic; // 07 LA7 (moby)
	public AudioSource nonSpatialAudio;

	void Awake () {
		print (player);
		print (ball);
		//DontDestroyOnLoad (player);
		//DontDestroyOnLoad (ball);
		DontDestroyOnLoad(system);
		PlayerPrefs.SetInt ("levelNumber", 0);
		PlayerPrefs.SetInt ("totalScore", 0);
		//print ("Calling Awake(): PlayerPrefs levelNumber and totalScore set to 0");
	} 

	// Use this for initialization
	void Start () {
		//print ("Starting GameManager");
		//if (PlayerPrefs.GetInt("level") == null) {
		//	PlayerPrefs.SetInt ("level", 0);
		//}
		// background music assigned to AudioSource on the Player object, will start on Awake and loop over ~20 minutes
		overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>(); //get the loading overlay on the player object (does not change between scenes)
		SetScene (); //initial scene settings
	}

	// Update is called once per frame
	void Update () {
		//print ("Updating GameManager");
		if(ball.GetComponent<MeshRenderer>().enabled) { // if the Ball's mesh renderer is turned on i.e. don't check this during scene resets) 
			CheckIfCheating (); // check for cheating and respond
		}
	}

	void SetScene () { //initial scene settings
		//print("Setting Scene " + levelNumber);
		List<GameObject> starList = new List<GameObject>();
		starList.Clear ();
		//uncollectedStars.Clear ();
		foreach (GameObject star in GameObject.FindGameObjectsWithTag("Star")) { //find all the stars in the current level
			starList.Add(star); // add the current star to the list of total stars
			//print("Building the list of all stars");
			//uncollectedStars.Add(star); // add the current star to the list of uncollected stars
			//print("Building the list of uncollected stars");
			star.GetComponent<MeshRenderer> ().enabled = true; //set the current star active (this is for resetting the scene, setting stars that were collected and disabled back to active)
			//print("Setting all the stars to Active");
		}
		totalStars = starList.Count; // set int "totalStars" to the Count of that list
		score = 0; // reset the current level score
		//print("Score is now " + score);
		totalScore = PlayerPrefs.GetInt ("totalScore"); //set it to the previous sum

		//need to assign these to a new object with each level load!
		playerStartPosition = GameObject.Find ("PlayerStartPosition");
		ballStartPosition = GameObject.Find ("BallStartPosition");
		goal = GameObject.Find ("Goal");

		// reset the Player position to the center of the platform
		player.transform.position = playerStartPosition.transform.position; 

		// set the Ball's velocity and spin to 0, turn the Ball's collider back on, and reset the Ball's position to the pedestal
		ball.GetComponent<Rigidbody> ().velocity = new Vector3(0, 0, 0);
		ball.GetComponent<Rigidbody> ().angularVelocity = new Vector3(0, 0, 0);
		ball.GetComponent<Collider>().enabled = true;
		ball.transform.position = ballStartPosition.transform.position;
		//AudioSource.PlayClipAtPoint(sceneIntroSound, ball.transform.position);
		nonSpatialAudio.Play();
		//LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>(); //get the loading overlay
		overlay.FadeOut(); //fade from black to scene
		//loadingOverlay.gameObject.GetComponent<LoadingOverlay>().FadeOut();
		//print ("Scene is set!");
	}

	void LoadLevel () { // this function is just so we can use Invoke when the ball hits the goal and wait long enough to fade to black
		SceneManager.LoadScene ("Scene" + levelNumber); // load the correct scene
		Invoke("SetScene", 2.0f);
	}

	void CheckIfCheating () { // check if cheating
		if (player.transform.position.y < 0.5  // if the player's transform.position.y < 0.5 (i.e. they are standing on the ground instead of the player's platform)
			&& GameObject.Find ("RightHandAnchor").GetComponent<ControllerInputManager> ().ballInHand) { // AND the ball is being held
			cheating = true;
			ball.GetComponent<Renderer> ().material = cheatingBallMAT; // set the ball material to the "cheating" color
			AudioSource.PlayClipAtPoint(cheatingSound, player.transform.position); // play the cheating sound on the Player
			print ("You scoundrel!");
		} 
		else {
			cheating = false; // if they are not cheating
			ball.GetComponent<Renderer> ().material = regularBallMAT; //set the ball material to the "normal" color
			//print ("Not cheating; proceed.");
		}
		foreach (GameObject star in starList) { // for each of the stars that has not already been collected
			star.GetComponent<MeshRenderer> ().enabled = !cheating; // set the current star's mesh renderer to enabled if the player is NOT cheating and disabled if the player IS cheating
		}
	}

	void OnTriggerEnter (Collider collider) { // check for ball interactions with stars and goal
		if(collider.gameObject.CompareTag("Star")) {
			uncollectedStars.Remove(collider.gameObject); 
			//print("Removed " + collider.gameObject + " from the list of uncollected stars.");
			Instantiate(sparks, gameObject.transform.position, Quaternion.Euler(-90, -0, 0)); // instantiate the poof 
			AudioSource.PlayClipAtPoint(starCollectSound, collider.gameObject.transform.position); // play collection sound on the Star
			collider.gameObject.GetComponent<MeshRenderer> ().enabled = false; // set the mesh renderer of the star with which the ball collided to disabled
			score++;
			//print ("Score is now " + score);
		}
		if (collider.gameObject.CompareTag ("Goal") && (score == totalStars)) { // if the ball hits the goal AND if all the stars in this scene have been collected
			//print ("The Ball has hit the Goal");
			Instantiate(sparks, gameObject.transform.position, Quaternion.Euler(-90, -0, 0)); // instantiate the poof 
			ball.GetComponent<Collider>().enabled = false; // turn off the ball's collider
			levelNumber++; // increase the level count
			//print("Level number is now " + levelNumber);
			PlayerPrefs.SetInt("totalScore", totalScore + score); // adds the score from this completed level to the total score for the whole game
			//yield return new WaitForSeconds(5.0f);
			if (levelNumber <= 3){ // if we haven't finished all the levels yet
				AudioSource.PlayClipAtPoint(goalHitSound, collider.gameObject.transform.position); // play goal hit sound on the Goal
				//LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>(); //get the loading overlay
				overlay.FadeIn(); // fade from scene to black
				//print("Fading to black...");
				Invoke("LoadLevel", 5.0f); // load next level
			}
			else { // if we win and we're on the last level
				print("Winner winner chicken dinner!");
				AudioSource.PlayClipAtPoint(totalWinSound, player.transform.position);
				// win! with fireworks
				// display the score
				// display a reset UI
			}
		}
		// choosing not to add the cases for Ball colliding with Fan trigger or Trampoline because those are both tagged as RGObject so that they can be handled the same in ControllerInputManager
	}

	// this is here instead of in OnTriggerEnter because the Player needs to interact with the Floor's collider as a collider, and not a trigger (will fall through)
	void OnCollisionEnter (Collision collision) { // check for ball interactions with floor and walls
		if(collision.gameObject.CompareTag("Environment") || (gameObject.transform.position.y < -0.5)){ // if ball hits the floor or walls OR bounces out of bounds and falls down
			AudioSource.PlayClipAtPoint(floorHitSound, gameObject.transform.position); // play floor collision sound on the Ball
			ball.GetComponent<Collider>().enabled = false; // set the Ball's collider to inactive
			starList.Clear();
			uncollectedStars.Clear();
			//LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>(); //find the loading overlay and access the attached script also called LoadingOverlay
			overlay.FadeIn(); // fade from scene to black
			//print ("Fading to black...");
			Invoke("SetScene", 5.0f); // reset everything after 5 seconds to give time to the fade
			//print ("Resetting...");
		}
	}
}