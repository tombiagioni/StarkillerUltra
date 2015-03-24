// PLAYER SCRIPT CS 20150214_2125 //

using UnityEngine;
using System;
using System.Collections;


public class PlayerScript:MonoBehaviour
{
	
	// Sprite references :
	public Sprite playerStand;
	public Sprite playerUp;
	public Sprite playerDown;
	
	// Player cached components :
	Transform myTr;				// Target player's transform
	SpriteRenderer mySpriteRdr;	// Target player's sprite renderer
	
	// Player exhaust cached components :
	GameObject myExhaustGo;		// Target exhaust's gameObject
	Transform myExhaustTr;		// Target exhaust's transform
	
	
	// Limits player's position relative to camera position :
	public Vector2 screenLimitsMin;
	public Vector2 screenLimitsMax;
	
	// Is the game paused ? (sent by "MainScript()")
	[HideInInspector]
	public bool gamePause;


	// Is player allowed to move ? // "HideInInspector" attribute hides the public variable in the editor inspector
	[HideInInspector]
	public bool canMove = true;

	float speed = 0.5f;				// Player moves speed
	
	public bool canShoot;						// Is player allowed to shoot ?
	float shootDelay;				// Delay between two shots
	
	public bool invincible = false;
	
	// bullets vars :
	public AudioClip bulletSound;
	// var bullet : GameObject;								// Regular - unoptimized way to instantiate object (we use object pooling instead)
	public ObjectPoolerScript weaponPool;					// This targets the player bullets' ObjectPool											
	GameObject bulletClone;					// value to cache spawned object
	PlayerWeaponScript playerWeaponScript;	// reference to bulletClone's script
	public int bulletCountLimiter = 2;						// max bullets available at the same time
	public int bulletCount = 0;						// actual number of bullets used
	
	// Player speed and weapon level - increased by upgrade items :
	bool upgrading;
	public int speedLevel = 1;
	public int speedLevelMax = 6;
	public int weaponLevel = 1;
	public int weaponLevelMax = 3;
	
	// Special upgrade : Bomb (bomb is set appart from weapon level) :
	public bool hasBomb;
	GameObject bombClone;			// value to cache spawned object
	bool canDropBomb = true;	// Is player allowed to drop bomb ?
	public float bombDelay = 4.0f;
	
	// Player death/explosion :
	public AudioClip deathSound;
	//var explosion : GameObject;				// Regular - unoptimized way to instantiate object (we use object pooling instead)
	public ObjectPoolerScript explosionPool;		// This targets the explosion ObjectPool											
	GameObject explosionClone;	// value to cache spawned object
	
	// Score :
	public int score = 0;
	// "uiScoreScript" targets the script attached to "UI_Score"
	public UiScoreScript uiScoreScript;
	
	// lifes left
	public int lifes;
	
	// lifes Base = number of lifes at game start
	public int lifesBase = 3;
	
	// lifes Max (Unused) = max lifes allowed
	// var lifesMax : int = 9;
	
	// Input system :
	// Used to store input axis
	Vector2 InputAxis;
	// Can the user use mouse or touch screen inputs ?
	public bool pointerInputAllowed = true;
	// Were keys or joystick used ?
	bool axisMoved = false;
	// Were mouse or touch screen used ? (shared with "PointerInputScript")
	// "HideInInspector" attribute hides the public variable in the editor inspector
	[HideInInspector]
	public bool pointerInputMoved = false;

	bool pointerInputSmoothedMoves = true; // should be kept to true, unless for some reason you need player to strictly follow mouse position
	// Pointer reference's transform (located in "Main Camera"/"PointerInputTarget")
	public Transform pointerInputTr;
	public PointerInputTargetScript pointerInputScript;
	// Used to store the previous position of "pointerInputTr"
	
	// Touch Screen controls :
	public bool ScreenButtonsInputAllowed = false; // If true, pointer input method ('pointerInputAllowed') will be set to false (in 'Start()' function)
	public JoystickCustom ScreenButtonMove;
	public JoystickCustom ScreenButtonFire1;
	public JoystickCustom ScreenButtonFire2;
	//
	
	// Camera :
	Camera cam;
	MainScript mainScript; // Target "MainScript" component attached to camera
	public bool camScrollEnabled	= true;
	public float camScrollSpeedX		= 0.1f;
	Transform camTr; // Target camera's transform
	
	
	public void OnPauseGame()
	{
	
		gamePause = true;
		
	}
	
	public void OnResumeGame()
	{
	
		gamePause = false;
		
	}
	
	public IEnumerator Start()
	{
		
		if (explosionPool == null)		Debug.LogError("You must populate 'explosionPool' with the corresponding object pool located in 'ObjectPools Container'", gameObject);
		if (weaponPool == null)			Debug.LogError("You must populate 'weaponPool' with the corresponding object pool located in 'ObjectPools Container'", gameObject);
		if (pointerInputTr == null) 	Debug.LogError("You must populate 'pointerInputTr' with the corresponding gameObject", gameObject);
		if (pointerInputScript == null) Debug.LogError("You must populate 'pointerInputScript' with the corresponding script", gameObject);
		
		// We cache components, this is usefull for performance optimisation !
		myTr = transform;									// We will now use "myTr" instead of "transform"
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();	// Same as above, we will now use "mySpriteRdr"
		
		// Reference player's exhaust transform. 'GameObject.Find("PlayerExhaust")' will return the game object named 'Exhaust', which is a child of 'Player'
		myExhaustGo = GameObject.Find("PlayerExhaust"); // (cache component)
		myExhaustTr = myExhaustGo.transform; 			// (cache component)
		
		// Find MainScript
		cam = Camera.main;
		mainScript = cam.GetComponent<MainScript>();
		
		camTr = cam.transform; 							// (cache component)

		myTr.localPosition = new Vector3 (-1.0f, myTr.localPosition.y, myTr.localPosition.z);

		// Enable moves
		canMove = true;
		
		UpgradeSpeed();		// Set initial speed
		StartCoroutine(UpgradeWeapon()); 	// Set initial weapon
		
		yield return null;
		
		canShoot = true;
		
		// Find screen buttons container
		GameObject onScreenBts = ScreenButtonMove.transform.parent.gameObject;
		
		// If 
		if (ScreenButtonsInputAllowed == true)
		{
			pointerInputAllowed = false;
			onScreenBts.SetActive(true);
		}
		
		else
		{
			onScreenBts.SetActive(false);
		}
		
	}
	
	public void Update()
	{
		
		// If game is paused abort the function  (sent by "MainScript()")
		if (gamePause == true) return;
		
		// If "canMove" is set to false, then abort the function
		if (canMove == false) return;
		
		// Set the axis input detection to false
		axisMoved = false;
		
		// "bulletCountLimiter" can be negative when upgrading weapon while firing ! //
		// if (bulletCount < 0) bulletCount = 0; //
		
		// Declare our input axis
		InputAxis = new Vector2(Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		
		// If joystick or keyboard Input = UP
		if (InputAxis.y > 0 || (ScreenButtonsInputAllowed == true && ScreenButtonMove.position.y > 0.35f))
		{	
			// Key input detected
			axisMoved = true;
			
			// Update the sprite
			if (mySpriteRdr.sprite != playerUp) mySpriteRdr.sprite = playerUp;
			
			// Update position (constrained to limits)
			if (myTr.position.y < camTr.position.y + screenLimitsMax.y)
			/*
            {
				var tmpPos = myTr.localPosition;
				tmpPos.y = myTr.localPosition.y + speed * Time.deltaTime;
				myTr.localPosition = tmpPos;
            }
			*/
			myTr.localPosition = new Vector3 (myTr.localPosition.x, myTr.localPosition.y + speed * Time.deltaTime, myTr.localPosition.z);


		}
		
		// Else if joystick or keyboard Input = DOWN
		else if (InputAxis.y < 0 || (ScreenButtonsInputAllowed == true && ScreenButtonMove.position.y < -0.35f))
		{	
			// Key input detected
			axisMoved = true;
			
			// Update the sprite
			if (mySpriteRdr.sprite != playerDown) mySpriteRdr.sprite = playerDown;
			
			// Update position (constrained to limits)
			if (myTr.position.y > camTr.position.y + screenLimitsMin.y)
			/*
            {
				var tmpPos = myTr.localPosition;
				tmpPos.y = myTr.localPosition.y - speed * Time.deltaTime;
				myTr.localPosition = tmpPos;
            }
            */
			myTr.localPosition = new Vector3 (myTr.localPosition.x, myTr.localPosition.y - speed * Time.deltaTime, myTr.localPosition.z);
		}
		
		else if (mySpriteRdr.sprite != playerStand) mySpriteRdr.sprite = playerStand;
		
		// Else if joystick or keyboard Input = LEFT
		if (InputAxis.x < 0 || (ScreenButtonsInputAllowed == true && ScreenButtonMove.position.x < -0.35f))
		{			
			// Key input detected
			axisMoved = true;
			
			// Update position (constrained to limits)
			if (myTr.position.x > camTr.position.x+screenLimitsMin.x)
			
			{
				/*
				var tmpPos = myTr.localPosition;
				tmpPos.x = myTr.localPosition.x - speed * Time.deltaTime;
				myTr.localPosition = tmpPos;
				*/
				myTr.localPosition = new Vector3 (myTr.localPosition.x - speed * Time.deltaTime, myTr.localPosition.y, myTr.localPosition.z);

				// Take into account the camera horizontal scrolling !
				if (camScrollEnabled == true) 
                {

					var tmpPos = myTr.position;
					tmpPos.x = myTr.position.x - camScrollSpeedX * Time.deltaTime;
					myTr.position = tmpPos;
                }
			}

		}
		
		// Else if joystick or keyboard Input = RIGHT
		else if (InputAxis.x > 0 || (ScreenButtonsInputAllowed == true && ScreenButtonMove.position.x > 0.35f))
		{	
			// Key input detected
			axisMoved = true;
			
			// Update position (constrained to limits)
			if (myTr.position.x < camTr.position.x+screenLimitsMax.x)
			
            {
				/*
				var tmpPos = myTr.localPosition;
				tmpPos.x = myTr.localPosition.x + speed * Time.deltaTime;
				myTr.localPosition = tmpPos;
				*/
				myTr.localPosition = new Vector3 (myTr.localPosition.x + speed * Time.deltaTime, myTr.localPosition.y, myTr.localPosition.z);
            }	
		}
		
		// "Fire1" button = simple fire ('left ctrl' by default on InputManager settings), "Fire2" button = auto-fire ('left alt' by default on InputManager settings)
		
		// Simple fire ("Fire1" button) (Input.GetButtonDown : Returns true during the frame the user pressed down the virtual button identified by buttonName.)
		if ( ( Input.GetKeyDown ("space") && ScreenButtonsInputAllowed == false) || (Input.GetButtonDown ("Fire1") && ScreenButtonsInputAllowed == false) || (ScreenButtonsInputAllowed == true && ScreenButtonFire1.IsFingerDown())  )
		{	
			StartCoroutine("LaunchProjectile", false); // buttonKeptDown is false
		}
		
		// Simple fire ("Fire1" button) kept down (Input.GetButton : Returns true while the virtual button identified by buttonName is held down.)
		else if ( (Input.GetKey ("space") && ScreenButtonsInputAllowed == false) || (Input.GetButton ("Fire1") && ScreenButtonsInputAllowed == false) || (ScreenButtonsInputAllowed == true && ScreenButtonFire1.IsFinger()) )
		{
			StartCoroutine("LaunchProjectile", true); /// 'LaunchProjectile()' argument 'buttonKeptDown' is true
		}
		
		// Auto-fire ("Fire2" button) - We don't check for 'input down this frame' here.
		else if ( (Input.GetButton ("Fire2") && ScreenButtonsInputAllowed == false) || (ScreenButtonsInputAllowed == true && ScreenButtonFire2.IsFinger()) )
		{
			StartCoroutine("LaunchProjectile", false); // 'LaunchProjectile()' argument 'buttonKeptDown' is false
		}
		
		// If mouse or touch screen inputs are allowed
		if (pointerInputAllowed == true)
		{	
			// Calculate the distance between 'pointer input target' transform and 'player' transform	
			float dist = Vector3.Distance(pointerInputTr.localPosition, myTr.localPosition); //Debug.Log ("(dist = " + dist, gameObject);
			
			// If pointer input has not moved, and is distance from player is lower than 0.08 (8 pixels), or if axis input was used, then set is position values to player transform position
			if ( (pointerInputMoved == false && dist < 0.08f) || axisMoved == true)
			{
				pointerInputTr.localPosition = myTr.localPosition;
				
				// Desactivate the pointer input
				pointerInputScript.isActive = false;
			}
			
			// Else, process pointer input movement 
			else if (axisMoved == false)
			{	
				// Activate the pointer input
				pointerInputScript.isActive = true;
				
				// If movement is smoothed ("smoothedMoves" value is true), use a lerp to smooth the positioning of "targetTr"
				if (pointerInputSmoothedMoves==true)
				{	
					// Move from "myTr" position to "pointerInputTr" position at the given speed ("pointerInputTargetSpeed")								
					myTr.position = Vector3.Lerp( myTr.position, pointerInputTr.position, /*pointerInputTargetSpeed*/ speed * 3.3f * Time.deltaTime );
				}			
				else // if "pointerInputSmoothedMoves" is false, use direct positioning
				{
					myTr.position = pointerInputTr.position;																											
				}
				
				// Update the sprite
				
				if (myTr.position.y < pointerInputTr.position.y - 0.3f && mySpriteRdr.sprite != playerUp)
				mySpriteRdr.sprite = playerUp;
				
				else if (myTr.position.y > pointerInputTr.position.y + 0.3f && mySpriteRdr.sprite != playerDown)
				mySpriteRdr.sprite = playerDown;
				
				else if (mySpriteRdr.sprite != playerStand)
				mySpriteRdr.sprite = playerStand;
			}
		}
		
	}
	
	public void UpgradeSpeed()
	{
		
		speed = 0.5f + 0.1f * speedLevel;
		
	}
	
	
	public IEnumerator UpgradeWeapon()
	{
	
		upgrading = true;
		
		if (weaponLevel == 1) { bulletCountLimiter = 2; shootDelay = 0.23f; }	// horizontal bullet
		
		else
		if (weaponLevel == 2) { bulletCountLimiter = 4; shootDelay = 0.26f; }	// horizontal + vertical bullet
	
		else
		if (weaponLevel == 3) { bulletCountLimiter = 2; shootDelay = 0.13f; }	// laser	
		
		yield return null;
		
		upgrading = false;
		
	}
	
	public IEnumerator LaunchProjectile(bool buttonKeptDown)
	{
	
		// If canShoot is already false, then abort the function
		if (canShoot==false || upgrading == true || renderer.isVisible == false) yield break;//return null; // FIX CS 13 01 2015 VERIFIER !!
	
		// Set canShoot to false until we process the function and the 'shootDelay'
		canShoot=false;
			
		// If player has bomb call "DropBomb()" function
		if (hasBomb == true && canDropBomb == true) StartCoroutine(DropBomb());
		
		// If we don't use auto-fire ("Fire2") button and simple fire button ("space" or "Fire1") is kept pressed,
		// then slow down the fire rate by adding an additional delay !
		// This is meant to encourage intensive button press.
		if ( buttonKeptDown == true && (Input.GetKey ("space") || Input.GetButton ("Fire1")) ) yield return new WaitForSeconds (shootDelay);
		
		if (bulletCount < bulletCountLimiter)
		{
			
				//REGULAR METHOD : INSTANTIATE bullet (unoptimised)
				//bulletClone = Instantiate(bullet, Vector3 (myTr.position.x+0.2, myTr.position.y-0.005, myTr.position.z), Quaternion.identity); // SMARTPOOL SYSTEM 
			
			if (weaponLevel == 1)
			{
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = new Vector3 (myTr.position.x + 0.2f, myTr.position.y - 0.005f, myTr.position.z);
	
				playerWeaponScript = bulletClone.GetComponent<PlayerWeaponScript>() as PlayerWeaponScript;
				playerWeaponScript.type = 1; // horizontal bullet
				playerWeaponScript.direction = new Vector2 (1.0f, 0.0f); // Set the direction of the bullet
				
				bulletCount = bulletCount + 1;
			}
			
			else
			
			if (weaponLevel == 2)
			{
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = new Vector3 (myTr.position.x + 0.2f, myTr.position.y - 0.005f, myTr.position.z);
	
				playerWeaponScript = bulletClone.GetComponent<PlayerWeaponScript>() as PlayerWeaponScript;
				playerWeaponScript.type = 1; // horizontal bullet
				playerWeaponScript.direction = new Vector2 (1.0f, 0.0f);
				
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = new Vector3 (myTr.position.x + 0.2f, myTr.position.y - 0.005f, myTr.position.z);
	
				playerWeaponScript = bulletClone.GetComponent<PlayerWeaponScript>() as PlayerWeaponScript;
				playerWeaponScript.type = 2; // diagonal bullet
				playerWeaponScript.direction = new Vector2 (1.0f, 0.5f);
				
				bulletCount = bulletCount + 2;
			}
			
			else
			
			if (weaponLevel == 3)
			{
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = new Vector3 (myTr.position.x + 0.2f, myTr.position.y - 0.005f, myTr.position.z);
	
				playerWeaponScript = bulletClone.GetComponent<PlayerWeaponScript>() as PlayerWeaponScript;
				playerWeaponScript.type = 4; // horizontal bullet
				playerWeaponScript.direction = new Vector2 (1.0f, 0.0f);	
				
				bulletCount = bulletCount + 1;
			}
			
			// Play bullet sound
			audio.clip = bulletSound;
			audio.Play();						
		}
		
		// Wait for weapon shoot delay
		yield return new WaitForSeconds (shootDelay);
		
		// shoot delay is now finished, set back "canShoot" to true, so we can fire again 
		canShoot=true;
		
	}
	
	public IEnumerator DropBomb()
	{
	
		canDropBomb = false;
		
		// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
		bombClone = weaponPool.Spawn();
		bombClone.transform.position = new Vector3 (myTr.position.x, myTr.position.y, myTr.position.z);
	
		playerWeaponScript = bombClone.GetComponent<PlayerWeaponScript>() as PlayerWeaponScript;
		playerWeaponScript.type = 5; // bomb
		playerWeaponScript.direction = new Vector2 (0.55f, -1.0f);
		
		yield return new WaitForSeconds (bombDelay);
		
		canDropBomb = true;
	
	}
	
	// Destroy all enemies on screen
	public IEnumerator Blast()
	{
	
		// switch the value "damageEnemies" located in "MainScript" camera component
		// ("MainScript" will use camera's collider to trigger enemies and damage them)
		
		mainScript.damageEnemies = true;
		
		yield return new WaitForSeconds(1.0f);
		
		mainScript.damageEnemies = false;
	
	}
	
	// This function is called by killed enemies' scripts
	public void UpdateScore(int addScore)
	{
	
		// Update player's score
		score = score + addScore;
		
		// Send the upated score to the UI ("UI_Score")'s score script ("UiScoreScript")
		uiScoreScript.ProcessScoreEntry(score);
	
	}	
	
	// This function is called by enemies' scripts
	public IEnumerator ApplyDamage(int damage) // "damage" value (refers to int "hp", health) isn't used for player in the project
	{	
	
		// If the player is "invicible", ignore the damages by aborting the function
		if (invincible == true) yield break; //return null; // FIX CS 13 01 2015 VERIFIER !!
		
		Debug.Log("Player received damage !", gameObject);
	
		myTr.collider2D.enabled = false;
		myTr.renderer.enabled = false;
		myExhaustTr.renderer.enabled = false;
		
		canMove = false;
		camScrollEnabled = false;
		
			
		mainScript.showDebugGUI = false; // hide the debug GUI (if it is displayed)
		mainScript.audio.Stop();
		
		// Create a big explosion !
		for(int i = 0; i < 6; i++)
		{
			explosionClone = explosionPool.Spawn();
			float randomXPos = UnityEngine.Random.Range(-0.04f, 0.04f);
			float randomYPos = UnityEngine.Random.Range(-0.04f, 0.04f);
			explosionClone.transform.position = myTr.position;
			/*
			var tmpPos = myTr.position;
			tmpPos.x = myTr.position.x + randomXPos;
			tmpPos.y = myTr.position.y + randomYPos;
			myTr.position = tmpPos;
			*/
			myTr.position = new Vector3 (myTr.position.x + randomXPos, myTr.position.y + randomYPos, myTr.position.z);

			//explosionClone.SetActive(true);
			audio.clip = deathSound;
			audio.Play();
			yield return new WaitForSeconds (audio.clip.length);
		}
		
		lifes = lifes - 1;
		
		yield return new WaitForSeconds (2.0f);
		
		// Launch "Die()" function located in the main script
		StartCoroutine(mainScript.Die());
		
	}
	
	// We use "LateUpdate()" to move the camera - it is better than to do it in an Update function, as all movements relative to the camera are already processed
	public void LateUpdate()
	{
	
		// If enabled, scroll the camera
		if (camScrollEnabled == true)
		camTr.position = new Vector3 (camTr.position.x + camScrollSpeedX * Time.deltaTime, camTr.position.y, camTr.position.z);
		
	}
	
	public void OnCollisionEnter2D(Collision2D coll)
	{
	
		if (coll.transform.CompareTag("Ground"))
		{	
			//Debug.Log("Player collided with the ground !", gameObject);
			
			// Collision with ground should kill the player, so apply a big 'damage' value if you implement health for the player
			StartCoroutine(ApplyDamage (999)); // "damage" value (refers to int "hp", health) isn't used for player in the project	
		}
	
	}
}







