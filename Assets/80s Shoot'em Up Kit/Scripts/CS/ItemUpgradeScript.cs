// ITEM UPGRADE SCRIPT CS 20151402_0718 //

using UnityEngine;
using System;
using System.Collections;


public class ItemUpgradeScript:MonoBehaviour
{
	
	// Object values :
	int type; // type 1 = Speed upgrade, type 2 = Weapon upgrade, type 3 = score bonus, type 4 = bomb, type 5 = blast
	float randomX;
	float randomY;
	float smoothLerp = 0.01f;
	Vector3 basePosition;
	Vector3 newPosition;
	
	public float playerAttractionRange = 0.1f;
	
	bool asleep = true;
		
	bool ready = false; 
	bool attracted = false; 
	bool earned = false;
	float distanceFromPlayer;
	
	public float boundToCamDelay;
	public float boundToCamDelayMax = 600.0f;
	
	// Object references :
	Transform myTr;
	GameObject myGo;
	SpriteRenderer mySpriteRdr;
	Animator myAnimator;
	
	// Sprite references :
	public Sprite notificationSpeedUp;
	public Sprite notificationWeaponUp;
	public Sprite notification1000Pts;
	public Sprite notificationBomb;
	public Sprite notificationBlast;
	
	// Player :
	GameObject player;
	Transform playerTr;
	PlayerScript playerScript;
	
	// Camera :
	Camera cam;
	Transform camTr;
	
	public void OnEnable()
	{
		
		// Check if the script has initialized
		if (ready == true)
		{
			SetItemType(); // Setup item upgrade type
		}
		
	}
	
	public void OnBecameVisible()
	{
		
		// Wake up object
		asleep = false;
		
	}
	
	
	public void OnBecameInvisible()
	{
		
		// Destroy object
		if (gameObject.activeInHierarchy == true && ready == true) StartCoroutine(DestroyObject());
	
	}
	
	public IEnumerator DestroyObject()
	{	
		
		if (audio.isPlaying) yield return new WaitForSeconds (audio.clip.length);  // Wait for the end of explosion audio clip
	
		yield return null; // yield function is needed because an animation is playing
	
		if (gameObject.activeInHierarchy == true)
		{	
			// Restore the Animator and the sorting order of the sprite, that were possibly changed to display upgrade notification (in "Update()")
			myAnimator.enabled = true;
			mySpriteRdr.sortingOrder = 2;
			
			myGo.SetActive(false);
			earned = attracted = false;
			asleep = true;
			boundToCamDelay = 0.0f;
			mySpriteRdr.color = Color.white;
		}	
	
	}
	
	public void Start()
	{
	
		myGo = gameObject;
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		myAnimator = myTr.GetComponent<Animator>();
		
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		playerTr = player.transform;
		
		cam = Camera.main;
		camTr = cam.transform;
		
		StartCoroutine(Prepare ());
		
	}
	
	public IEnumerator Prepare()
	{
	
		float rand = UnityEngine.Random.Range(-1.0f, 1.0f);
		float randAdd = UnityEngine.Random.Range(-0.25f, 0.25f);
		
		if (rand <= 0)	randomX = -1 + randAdd;
		else randomX = 1 + randAdd;
		
		
		rand = UnityEngine.Random.Range(-1.0f, 1.0f);
		randAdd = UnityEngine.Random.Range(-0.25f, 0.25f);
		if (rand <= 0)	randomY = -1 + randAdd;
		else randomY = 1 + randAdd;
		
		smoothLerp = .4f;//smoothLerp = Random.Range(1.0, 2.0);
	
		yield return null;
		
		ready = true;
		
		SetItemType(); // Determine wich type of item is created
		
	}
	
	// Determine wich type of item is created
	public void SetItemType()
	{
		
		myAnimator.SetBool("Item_Spawned_Bool", true);
		myAnimator.SetBool("Item_Grabbed_Bool", false);
		myAnimator.SetInteger("Item_Type_Int", 0);
		
		bool playerSpeedUpgradable = false;
		bool playerWeaponUpgradable = false;
		bool playerHasBomb = false; // bomb and blast are considered as 'special' upgrade
		float randomUpgrade = 0.0f;
		
		// Check if player is eligible for speed upgrade
		if (playerScript.speedLevel < playerScript.speedLevelMax) playerSpeedUpgradable = true;
		else playerSpeedUpgradable = false;
		
		// Check if player is eligible for weapon upgrade
		if (playerScript.weaponLevel < playerScript.weaponLevelMax) playerWeaponUpgradable = true;
		else playerWeaponUpgradable = false;
		
		// Check if player has bomb special upgrade
		playerHasBomb = playerScript.hasBomb;
		
		randomUpgrade = (float)UnityEngine.Random.Range (1,4);
		
		if (randomUpgrade <= 2)
		{
			
			// If speed and weapon are both upgradable
			if ( playerSpeedUpgradable == true && playerWeaponUpgradable == true )
			{
				randomUpgrade = (float)UnityEngine.Random.Range (1,3);
				
				if (randomUpgrade == 1) 				{ type = 1; myAnimator.SetInteger("Item_Type_Int", 1); }	// type = speed upgrade
				else if (randomUpgrade == 2)			{ type = 2; myAnimator.SetInteger("Item_Type_Int", 2); }	// type = weapon upgrade
			}
			
			else if (playerSpeedUpgradable == true)		{ type = 1; myAnimator.SetInteger("Item_Type_Int", 1); } 	// type = speed upgrade
			else if (playerWeaponUpgradable == true)	{ type = 2; myAnimator.SetInteger("Item_Type_Int", 2); }	// type = weapon upgrade
			else										{ type = 3; myAnimator.SetInteger("Item_Type_Int", 3); }	// type = bonusScore upgrade
		}
		
		else
		{
			// Special upgrade : bomb, blast, or score bonus
			randomUpgrade = (float)UnityEngine.Random.Range (1,3);
				
			if (randomUpgrade == 1 && playerHasBomb == false) 	{ type = 4; myAnimator.SetInteger("Item_Type_Int", 4); }	// type = bomb
			else if (randomUpgrade == 1)						{ type = 5; myAnimator.SetInteger("Item_Type_Int", 5); }	// type = blast
			else												{ type = 3; myAnimator.SetInteger("Item_Type_Int", 3); }	// type = score bonus upgrade
		}
		
	}
	
	public void Update()
	{
		
		// If isn't ready or is sleeping, abort the function
		if (ready == false || asleep == true) return;
		
		// Else
		
		// For some time, upgrade will bump when it hits the camera boundaries
		if (boundToCamDelay < boundToCamDelayMax && earned == false)
		{	
			//increment "boundToCamDelay"
			boundToCamDelay = boundToCamDelay + 1 * Time.deltaTime;
			
			if (myTr.position.y >= camTr.position.y + 0.95f)
			{	
				// Bound to cam limits
				myTr.position = new Vector3 (myTr.position.x, camTr.position.y + 0.95f, myTr.position.z);

				// Inverse Y direction
				randomY = -randomY;
			}
			
			else
			
			if (myTr.position.y <= camTr.position.y - 0.95f)
			{	
				myTr.position = new Vector3 (myTr.position.x, camTr.position.y - 0.95f, myTr.position.z);

				randomY = -randomY;
			}
			
			if (myTr.position.x >= camTr.position.x + 1.5f)
			{	
				myTr.position = new Vector3 (camTr.position.x + 1.5f, myTr.position.y, myTr.position.z);

				randomX = -randomX;
			}
			
			else
			
			if (myTr.position.x <= camTr.position.x - 1.5f)
			{	
				myTr.position = new Vector3 (camTr.position.x - 1.5f, myTr.position.y, myTr.position.z);

				randomX = -randomX;
			}
		}
		
		// Reference actual position
		basePosition = myTr.position;
		
		// Wanted position
		newPosition = new Vector3(myTr.position.x+randomX, myTr.position.y+randomY, myTr.position.z);
		
		// Get the distance between upgrade and player
		distanceFromPlayer = Vector2.Distance(new Vector2(playerTr.position.x,playerTr.position.y), new Vector2(basePosition.x, basePosition.y));
		
		//Debug.Log ("distanceFromPlayer" + distanceFromPlayer + "Attracted = " + attracted +"   playerAttractionRange " + playerAttractionRange, gameObject);
		
		// If earned, sprite has been set to 'notification' sprite ('speed up !', 'weapon up !', etc)
		// make the notification go up and fade out 
		if (earned == true)
		{
			myTr.position = new Vector3 (myTr.position.x, myTr.position.y + 0.25f * Time.deltaTime, myTr.position.z);

			mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, mySpriteRdr.color.a -0.6f * Time.deltaTime);

		}
		
		// If close enough to the player (based on "playerAttractionRange" value), set it "attracted"
		// (by default attraction is disabled for upgrade, by setting "playerAttractionRange" to 0)	
		else if (earned == false && attracted == false && distanceFromPlayer <= playerAttractionRange && playerScript.canMove == true)
		{
			// Attracted by player	
			attracted = true;
		}
		
		// If not earned by player yet, and is close enough, then set it "earned" (this act like a collision with the player)		
		else if (earned == false && distanceFromPlayer < 0.16f && playerScript.canMove == true) {
				  
			earned = true;
	
			// Set the animator boolean "Item_Grabbed_Bool" to true, to let it be aware that upgrade as been earned
			myAnimator.SetBool("Item_Grabbed_Bool", true);
			myAnimator.SetInteger("Item_Type_Int", 0);
			audio.Play();
			
			// Disable the Animator and change the sorting order of the sprite, in order to display the upgrade notification sprite
			myAnimator.enabled = false;
			
			// the notification will be drawn behind player sprite
			mySpriteRdr.sortingOrder = -2;
			
			// If type = Speed Up
			if (type == 1)
			{
				if (playerScript.speedLevel < playerScript.speedLevelMax)
				{
					mySpriteRdr.sprite = notificationSpeedUp;
					playerScript.speedLevel = playerScript.speedLevel + 1;
					playerScript.UpgradeSpeed();
				}
				
				else
				
				{
					mySpriteRdr.sprite = notification1000Pts;
					playerScript.UpdateScore (1000);
				} 
			}
			
			// If type = Weapon Up
			else if (type == 2)
			{
				if (playerScript.weaponLevel < playerScript.weaponLevelMax)
				{
					mySpriteRdr.sprite = notificationWeaponUp;
					playerScript.weaponLevel = playerScript.weaponLevel + 1;
					StartCoroutine(playerScript.UpgradeWeapon());
				}
				
				else
				
				{
					mySpriteRdr.sprite = notification1000Pts;
					playerScript.UpdateScore (1000);
				} 
			}		
			
			// If type = bonus score
			else if (type == 3)
			{
				mySpriteRdr.sprite = notification1000Pts; 
				playerScript.UpdateScore (1000);
			}
			
			// If type = bomb
			else if (type == 4)
			{
				if (playerScript.hasBomb == false)
				{
					mySpriteRdr.sprite = notificationBomb;
					playerScript.hasBomb = true;
					playerScript.UpdateScore (1000);
				}
				
				else
				
				{
					mySpriteRdr.sprite = notification1000Pts;
					playerScript.UpdateScore (1000);
				}  
				
			}
			
			// If type = blast
			else if (type == 5)
			{
				mySpriteRdr.sprite = notificationBlast;
				StartCoroutine(playerScript.Blast());
				//...	
			}
			
			//Invokes "DestroyObject()" in 2 seconds, so the notification sprite has some time to display
			Invoke("DestroyObject", 2.0f);
			
		}	
		
		// Else if upgrade is attracted and not close enough to be grabbed, move it towards player transform
		else if (attracted == true && playerScript.canMove == true) myTr.position = Vector3.MoveTowards(basePosition, playerTr.position, Time.deltaTime);
	
		// Else if it is not attracted, simply update is position
		else myTr.position = Vector3.Lerp(basePosition, newPosition, smoothLerp * Time.deltaTime);	
	
	}

}




