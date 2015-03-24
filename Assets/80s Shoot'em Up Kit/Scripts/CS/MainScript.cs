// MAIN SCRIPT CS 20151402_0718 //

using UnityEngine;
using System;
using System.Collections;


public class MainScript:MonoBehaviour
{
	
	// Handle multiple scenes loading ? - Else the game runs in one unique scene
	public bool multipleScenesHandling = false;
	
	//[HideInInspector var level:int=0]; // The current game level
	[HideInInspector]
	public int level = 0; 
	
	// Game's current state
	public string gameState = "menu"; // "menu" or "inGame"
	
	//[HideInInspector var gamePause:boolean]; // Is the game paused ?
	[HideInInspector]
	public bool gamePause; 
	

	public AudioClip soundPlayStart;
	public AudioClip soundSpawnIntro;
	public AudioClip soundGameOver;
	
	// If set to true, framerate will be locked to the target speed (frame per second)
	public bool lockFramerate = false;
	public int framerate = 60;
	
	// Star particle objects
	public GameObject particleStarsFar;
	public GameObject particleStarsClose;
	
	// UI :
	public GameObject uiMenu;
	public GameObject uiPause;
	public GameObject uiPause_Label;
	public GameObject uiInGame;
	public GameObject uiScore; // the score UI
	public GameObject uiLifes; // the lifes UI
	TextMesh uiLifesTextMesh; // lifes UI textMesh component (used to displays lifes count)
	
	public GameObject uiText; // the text UI used by Event System
	
	// Player :
	GameObject player;
	PlayerScript playerScript; // reference to player's script
	public UiScoreScript uiScoreScript; // reference to UI score's script
	
	// Last checkpoint position reached - used by "Spawn()"
	//[HideInInspector var checkpointPos:float=0]; // "HideInInspector" attribute hides the public variable in the editor inspector
	[HideInInspector]
	public float checkpointPos = 0; 


	// Camera's layer masks
	public LayerMask regularLayerMask; // Regular camera's layer mask
	public LayerMask excludeEnemiesLayerMask; // This one is used when spawning to a checkpoint to hide 'Enemy Layer'
	
	// Used by "Spawn()" and "OnTriggerStay2D()" to destroy displayed enemies when spawning to a saved checkpoint

	//[HideInInspector var disableEnemies:boolean]; // destroy enemies without apply damage to them
	[HideInInspector]
	public bool disableEnemies; 
	//[HideInInspector var damageEnemies:boolean]; // apply damage to enemies
	[HideInInspector]
	public bool damageEnemies; 

	// Delete all stored Player Preferences at start ?
	public bool debugDeleteAllPlayerPrefs;
	
	// Show debug GUI ? (display debug option in Unity native GUI)
	public bool showDebugGUI = true;
	
	// Debug GUI toggles
	bool toggleGiveCoins;
	bool toggleGiveCoinsPrevious ;
	bool toggleShowDebug;
	bool toggleShowDebugPrevious;
	bool toggleInvincible;
	bool toggleInvinciblePrevious;
	
	public IEnumerator OnApplicationQuit()
	{
	
		StartCoroutine(GameDataReset ()); // This function take care of reseting all game data to start a fresh new game when level is reloaded
		
		yield return null;
		
	}
	
	public IEnumerator Start()
	{	
		
		if (debugDeleteAllPlayerPrefs == true) Debug.LogWarning ("WARNING : 'debugDeleteAllPlayerPrefs' value should be set to false !", gameObject);
		
		Debug.LogWarning ("REMEMBER TO SET THE BUILD SETTINGS ACCORDINGLY TO SCENES HANDLING !", gameObject);
		if (multipleScenesHandling == true) Debug.LogWarning ("(you are using 'multipleScenesHandling' attribute)", gameObject);
		else Debug.LogWarning ("(you are NOT using 'multipleScenesHandling' attribute)", gameObject);
		
		// Check if we have to delete Player Prefs for debugging purpose
		if (debugDeleteAllPlayerPrefs == true) PlayerPrefs.DeleteAll();
		
		// Find player gameObject and his script
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		
		yield return null;
		
		
		if (lockFramerate == true)
		{
			// Lock the framerate of the game
			Application.targetFrameRate = framerate;
		}
		
		// Define sorting layer and sorting order for background star particles and UI
		
		particleStarsFar.particleSystem.renderer.sortingLayerName = "Default"; // The layer used to define this sprite's overlay priority during rendering.
	    particleStarsFar.particleSystem.renderer.sortingOrder = -3; // The overlay priority of this sprite within its layer. Lower numbers are rendered first and subsequent numbers overlay those below.
		particleStarsClose.particleSystem.renderer.sortingLayerName = "Default";
	    particleStarsClose.particleSystem.renderer.sortingOrder = -2;
	    uiScore.renderer.sortingLayerName = uiLifes.renderer.sortingLayerName = uiText.renderer.sortingLayerName = "UI"; // The layer used to define this sprite's overlay priority during rendering.
		uiPause_Label.renderer.sortingLayerName = "UI";  // The layer used to define this sprite's overlay priority during rendering.
		
		// Retrieve textMesh component of "uiLifes" (the lifes UI)
		uiLifesTextMesh = uiLifes.GetComponent<TextMesh>();
		
		
		
		// If Player Preference "Level" doesn't exist, create it and set it to 0 - "Level" is used to load additively corresponding game scene (if 'multipleScenesHandling' value is set to 'true')
		if (PlayerPrefs.HasKey("Level") == false) { PlayerPrefs.SetInt("Level", 0);}
		
		// If Player Preference "Lifes" doesn't exist, create it and set it to "lifesBase" value - "LifesBase" is the number of lifes at game start
		if (PlayerPrefs.HasKey("Player lifes") == false) { PlayerPrefs.SetInt("Player lifes", playerScript.lifesBase);}
	
		// If Player Preference "Give coins" doesn't exist, create it and set it to false. Set the GUI toggle "toggleGiveCoins" accordingly
		if (PlayerPrefs.HasKey("Give coins") == false) { PlayerPrefs.SetInt("Give coins", 1); toggleGiveCoins = false; }
		
		// Else if Player Preference "Give coins" is true (2), set toggle "toggleGiveCoins" to true
		else if (PlayerPrefs.GetInt("Give coins") == 2) toggleGiveCoins = true;
		
		// Else if Player Preference "Give coins" is false (1), set toggle "toggleGiveCoins" to false
		else if (PlayerPrefs.GetInt("Give coins") == 1) toggleGiveCoins = false;
		
		if (PlayerPrefs.HasKey("Spawn position")) checkpointPos = PlayerPrefs.GetFloat("Spawn position");
		
		// If we want the project to handle differents scenes for game levels, load current level stored data
		if (multipleScenesHandling == true)
		{
			level = PlayerPrefs.GetInt("Level");
		}
		
		if (PlayerPrefs.HasKey("Game state")) // if player preference "Game state" already exist
		{
			gameState = PlayerPrefs.GetString("Game state");
		}
		
		else // Else (this is game's first run) create it and set it (and script's value) to "menu"
		{
			PlayerPrefs.SetString("Game state", "menu");
	
			gameState = "menu";
		}
		
		// If 'gameState' is "menu"
		if (gameState == "menu") StartCoroutine(InitMenu()); // Launch menu (new game)
		
		// Else
		else if (gameState == "inGame") StartCoroutine(InitGame ()); // We are already in a game session ! Launch game
		
	}
	
	// Pause the game - called by "Update()" on pause key press if "gamePause" is false
	public IEnumerator GamePauseEnable()
	{
	
		gamePause = true;
		
		Time.timeScale = 0.0f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
		
		yield return null;
		
		// Activate pause UI elements
		uiPause.SetActive(true);
		
		// Send pause message to all objects in the scene
		GameObject[] objects = (GameObject[])FindObjectsOfType (typeof(GameObject));
		
			foreach(GameObject go in objects)
			{
				go.SendMessage ("OnPauseGame", SendMessageOptions.DontRequireReceiver);
			}
		
	}
	
	// Resume the game - called by "Update()" on pause key press if "gamePause" is true
	public IEnumerator GamePauseResume()
	{
	
		gamePause = false;
		
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
		
		yield return null;
		
		//UnPause Objects (send unpause message to all objects in the scene)
		GameObject[] objects = (GameObject[])FindObjectsOfType (typeof(GameObject));
		foreach(GameObject go in objects)
		{
			go.SendMessage ("OnResumeGame", SendMessageOptions.DontRequireReceiver);
		}	
	
		// Deactivate pause UI elements
		uiPause.SetActive(false);
	
	}
	
	// Called by "Start()" when we launch a fresh new game (remember that the choice is ruled by 'gameState' value)
	public IEnumerator InitMenu()
	{
		
		// Activate game UI elements
		uiInGame.SetActive(false);
		
		// Set the variable "Game State" to "menu" - used to retrieve the state of the game when it is needed
		PlayerPrefs.SetString("Game state", "menu");
		
		yield return null;
		
		gameState = "menu"; // Change the game state value
		
		// We are not in game, so reset the game-relatives data
		StartCoroutine(GameDataReset ()); // This function take care of reseting all game data to start a fresh new game when level is reloaded
		
		yield return null;
		
		player.SetActive(false);
		
		yield return new WaitForSeconds (1.0f);
		
		uiMenu.SetActive(true);
		
		// note : in "Update()" we now (now that gameState = "menu" and menu UI is active) wait for any key press to exit menu ("ExitMenu()") and then init the game
		
	}
	
	// Called by "Update()" on any key press when game is ready
	public IEnumerator ExitMenu()
	{
	
		gameState = "exitMenu";
		audio.clip = soundPlayStart;
		audio.Play();
		yield return new WaitForSeconds (audio.clip.length);
		StartCoroutine(InitGame ());
		
	}
	
	// Called by "ExitMenu()" if we come from menu, or by "Start()" if we are already in game
	// See 'MainScript working cycle diagram' in documentation for details
	public IEnumerator InitGame()
	{
		
		// Set the variable "Game State" to "inGame" - used to retrieve the state of the game when it is needed
		PlayerPrefs.SetString("Game state", "inGame");
		
		yield return null;
		
		gameState = "inGame"; // Change the game state value
		
		player.SetActive(true);
		player.renderer.enabled = false;
		GameObject.Find("Exhaust").renderer.enabled = false;
		
		// Do we have more than one 'scene' in our project ? if yes, run the current game level
		if (multipleScenesHandling == true)
		{
			if (level > 0) Application.LoadLevelAdditive (level);
			else Application.LoadLevelAdditive (1);
		}
		
		yield return null;
		
		// Switch UI elements
		uiMenu.SetActive(false);
		uiInGame.SetActive(true);
		uiLifes.SetActive(false);
		
		player.renderer.enabled = true;
		GameObject.Find("Exhaust").renderer.enabled = true;
		
		// We are in game, so retrieve game session data (they are already reseted in "InitMenu()" if we come from menu)
		playerScript.score = PlayerPrefs.GetInt("Player score"); // retrieve score
		
		// Display score
		uiScoreScript.scoreGet = playerScript.score;	// Send the current score to the score UI ('UI_Score')
		uiScoreScript.UpdateScoreEntryDirect();			// Displays directly the score (in the score UI) without visual effect
		
		playerScript.lifes = PlayerPrefs.GetInt("Player lifes"); // retrieve player lifes count
		checkpointPos = PlayerPrefs.GetFloat("Spawn position"); // retrieve checkpoint (spawn) position
		
		StartCoroutine(Spawn (checkpointPos)); // Spawn player at position
		
		// Display life count
		uiLifesTextMesh.text = "@ x " + playerScript.lifes; // "@" character has been replaced by a ship symbol in the font texture
		
		uiLifes.SetActive(true);
		
		audio.clip = soundSpawnIntro;
		audio.Play();
		
		yield return new WaitForSeconds (audio.clip.length + 1);
		
		uiLifes.SetActive(false);
	
	}
	
	public void Update()
	{
		
		// Exit application on key input if we are in menu
	   	if (Input.GetKeyDown ("escape") && gameState == "menu") Application.Quit();
		
		// Exit menu (and start game) on any key input when ready
		else if (Input.anyKey && gameState == "menu" && uiMenu.activeInHierarchy == true)
		{
			StartCoroutine(ExitMenu());
		}
		
		// Switch game pause on key input
		else if ( gameState == "inGame" && ( Input.GetKeyDown("p") || Input.GetKeyDown(KeyCode.Menu)  || Input.GetButtonDown ("Pause")) )
		{
			if (gamePause == false) StartCoroutine(GamePauseEnable());
			else StartCoroutine(GamePauseResume());
	    }
		
		// Reset game on key input
		else if (gameState == "inGame" && (Input.GetKeyUp ("escape") || Input.GetKeyUp ("m") || Input.GetKeyUp ("r")))
		{
			if (gamePause == true) StartCoroutine(GamePauseResume ()); // Ensure that game is not paused
	
			StartCoroutine(GameDataReset()); // Reset Player Prefs ! // This function take care of reseting all game data to start a fresh new game when level is reloaded
			Application.LoadLevel(0); // then reload Level
		}
		
	}
	
	
	// Called by the player ('PlayerScript') when loosing a life
	public IEnumerator Die()
	{
		
		// If player lifes > = 0 - We lost a life, restart at last checkpoint
		if (playerScript.lifes >= 0)
		{
			
			if (gamePause == true) StartCoroutine(GamePauseResume ()); // Ensure that game is not paused
			
			// We store the game data in PlayerPrefs before reloading the scene
			PlayerPrefs.SetInt("Player lifes", playerScript.lifes);
			PlayerPrefs.SetString("Game state", "inGame"); // The script will know that we are already in game, it will 'continue' game session rather than init menu ! (this is managed in "Start()" function)
			PlayerPrefs.SetInt("Player score", playerScript.score);
			PlayerPrefs.SetFloat("Spawn position", checkpointPos);
			PlayerPrefs.SetInt("Level", level);
			
			yield return null;
			
			Application.LoadLevel (0);
		}
		
		// Else life < 0 - Game Over
		else
		{
			StartCoroutine(GameDataReset ()); // This function take care of reseting all game data to start a fresh new game when level is reloaded
			yield return null;
			StartCoroutine(GameOver());
		}
		
	}
	
	// Called by the player ('PlayerScript') when loosing a life, or called by an event
	public IEnumerator SetLevel()
	{
			
			if (gamePause == true) StartCoroutine(GamePauseResume ()); // Ensure that game is not paused
			
			if (level == 0) // level loaded is 0 (menu) so reset all values
			StartCoroutine(GameDataReset ());	// This function take care of reseting all game data to start a fresh new game when level is reloaded
			
			
			else // level > 0, we are still in game
			{
				PlayerPrefs.SetString("Game state", "inGame");			// The script will know that we are already in game, it will 'continue' game session rather than init menu ! (this is managed in "Start()" function)
				PlayerPrefs.SetInt("Player lifes", playerScript.lifes);
				PlayerPrefs.SetInt("Player score", playerScript.score);
				PlayerPrefs.SetFloat("Spawn position", checkpointPos);
				PlayerPrefs.SetInt("Level", level);
			}
			yield return null;
			
			Application.LoadLevel (0);
			
	}

	// Reset Player Prefs - This function take care of reseting all game data to start a fresh new game when level is reloaded
	public IEnumerator GameDataReset()
	{
		
		PlayerPrefs.SetString("Game state", "menu"); // The script will know that we are no more in a game session, and will send us back to menu (this is managed in "Start()" function)
		PlayerPrefs.SetInt("Player score", 0);
		PlayerPrefs.SetInt("Player lifes", playerScript.lifesBase);//PlayerPrefs.SetInt("Player lifes", 0);
		PlayerPrefs.SetFloat("Spawn position", 0.0f);
		PlayerPrefs.SetInt("Level", 0);
		
		yield return null;
		
	}
	
	// Game Over - Displays "GAME OVER" text, plays sound and restart the application - Game data are already reseted by "Die()" function
	public IEnumerator GameOver()
	{
		
		if (gamePause == true) StartCoroutine(GamePauseResume ()); // Ensure that game is not paused
		
		audio.Stop();
		audio.clip = soundGameOver;
		audio.loop = false;
		audio.Play();
		
		uiLifesTextMesh.text = "GAME OVER";
		uiLifes.SetActive(true);
		
		yield return new WaitForSeconds (audio.clip.length + 1);
		
		Application.LoadLevel (0);	
		
	}
	
	// "Spawn()" place the camera at a given "camXpos" horizontal position,
	// and delete enemies in view (using "OnTriggerStay2D()")
	public IEnumerator Spawn(float camXpos)
	{	
		
		playerScript.invincible = true;
		playerScript.camScrollEnabled = false;
		
		// Limit the camra far clip plane to hide sprites
		camera.farClipPlane = 0.31f;
		
		// Set "disableEnemies" to true in order to enable the camera Collider to trigger the enemies in view
		disableEnemies = true;
		
		// Move the camera to the desired position
		transform.position = new Vector3 (camXpos, transform.position.y, transform.position.z);

		
		// exclude enemies from the camera view
		camera.cullingMask = excludeEnemiesLayerMask;
	
		yield return new WaitForSeconds (0.5f);
		
		disableEnemies = false;
		
		// Restore the camra far clip plane to show sprites
		camera.farClipPlane = 5.0f;
		
		playerScript.enabled = true;
		playerScript.camScrollEnabled = true;
		
		if (toggleInvincible /*!= null*/) playerScript.invincible = toggleInvincible;
		else playerScript.invincible = false;
		
		playerScript.canShoot = true;
		
		// Restore regular culling mask, enabling "Enemy Layer" in camera view
		camera.cullingMask = regularLayerMask;
		
		// check for show debug option and display accordingly
		if (toggleShowDebug == true)
		{
			// Switch on layer 15 'Event Layer', leave others as-is
			camera.cullingMask |= (1 << 15);
		}
		
		else // if (toggleShowDebug == false)
		{
			// Switch off layer 15 'Event Layer', leave others as-is
			camera.cullingMask = ~(1 << 15);
		}
		
	}
	
	// Play an AudioClip - Usually called by Event System
	public IEnumerator MusicPlay(AudioClip music,bool loop,bool waitForClipEnd,float delay)
	{
		
		// If music is already playing, abort the funtion
			if (audio.clip == music && audio.isPlaying == true) yield break;//return null; // FIX CS 13 01 2015 VERIFIER !!
		
		if (waitForClipEnd == true && audio.isPlaying == true)
		{
			// Wait for the audio to have finished
			yield return new WaitForSeconds (audio.clip.length + delay);
		}
		
		else if (delay != 0) yield return new WaitForSeconds (delay);
		
		audio.clip = music;
		audio.loop = loop;
		audio.Play();
		
	}
	
	// Stop an AudioClip - Usually called by Event System
	public IEnumerator MusicStop(bool fadeOut)
	{
	
		if (fadeOut == true)
		{
			for(int i = 0; audio.volume > 0; i++)
			{
				audio.volume = audio.volume - 0.125f * Time.deltaTime;
				yield return null;
			}
			
			audio.Stop();
			audio.volume = 1.0f;
		}
		
		else audio.Stop();
		
	}
	
	// We use this function to trigger enemies in view
	public void OnTriggerStay2D(Collider2D other)
	{
	
		if (other.CompareTag("Enemy"))
		{	
			//Debug.Log("Detected");
			if (disableEnemies == true)
			{
				//Debug.Log ("disableEnemies ! xxxxx", gameObject);
				other.SendMessageUpwards ("OnBecameInvisible", SendMessageOptions.DontRequireReceiver);
			}
		
			if (damageEnemies == true) other.SendMessageUpwards ("ApplyDamage", 1, SendMessageOptions.DontRequireReceiver);
		}
		
	}
	
	// THIS PART IS OPTIONAL AND USED TO DISPLAY DEBUG GUI WINDOW ////////////////////////////////////////////////////////////////
	
	public void OnGUI()
	{
		
		if (showDebugGUI == false || gameState == "menu") return;
		
		// Make a background box
		GUI.Box (new Rect ((float)(Screen.width + 10 - Screen.width), (float)(Screen.height-28), (float)(Screen.width - 20), 20.0f), " ");
		
		toggleGiveCoinsPrevious = toggleGiveCoins;
		
		// Make the first toggle. If it is pressed, "giveCoins" player preference value will be changed
		toggleGiveCoins = GUI.Toggle (new Rect (40.0f,(float)(Screen.height-30),80.0f,20.0f),toggleGiveCoins, "Give coins");
		{	
			if (toggleGiveCoins != toggleGiveCoinsPrevious)
			{
				if (toggleGiveCoins == true)
				{
					
					PlayerPrefs.SetInt("Give coins", 2); // we use the value 2 for true, and one for false
				}
				else
				{
					
					PlayerPrefs.SetInt("Give coins", 1); // we use the value 2 for true, and one for false
				}
				print ("Give coins = " + toggleGiveCoins);
			}
		}
		
		toggleShowDebugPrevious = toggleShowDebug;
		
		// Make the second toggle. If it is pressed, "Event Layer" visibility will be changed
		toggleShowDebug = GUI.Toggle (new Rect (140.0f,(float)(Screen.height-30),150.0f,20.0f),toggleShowDebug, "Show Debug elements");
		{
			if (toggleShowDebug != toggleShowDebugPrevious)
			{
				if (toggleShowDebug == true)
				{
					// Switch on layer 15 'Event Layer', leave others as-is
					camera.cullingMask |= (1 << 15);
				}
				
				else
				{
					// Switch off layer 15 'Event Layer', leave others as-is
					camera.cullingMask = ~(1 << 15);
				}
			}
		}
		
		toggleInvinciblePrevious = toggleInvincible;
		
		// Make the second toggle. If it is pressed, "Event Layer" visibility will be changed
		toggleInvincible = GUI.Toggle (new Rect (310.0f,(float)(Screen.height-30),100.0f,20.0f),toggleInvincible, "Invincible");
		{
			if (toggleInvincible != toggleInvinciblePrevious)
			{
				playerScript.invincible = toggleInvincible;
			}
		}
		
		
		// Player Stats labels
		GUI.Label (new Rect ((float)(Screen.width - 350), (float)(Screen.height-30), 300.0f, 20.0f), "Speed lvl : " + playerScript.speedLevel +  "   Weapon lvl : " + playerScript.weaponLevel  +  "   has Bomb : " + playerScript.hasBomb);
	
	}

}









