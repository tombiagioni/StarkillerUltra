// ITEM MONEY SCRIPT CS 20150119_1055 //

using UnityEngine;
using System;
using System.Collections;


public class ItemMoneyScript:MonoBehaviour
{
	
	// Object values :
	float randomX;
	float randomY;
	float randomSmoothLerp = 0.01f;
	
	Vector3 basePosition;
	Vector3 newPosition;
	
	public float playerMoneyAttractionRange = 0.1f;
	bool ready = false; 
	bool attracted = false; 
	float moveToPlayerDelay = 0.0f;
	float moveToPlayerDelayMax = 2.0f;
	bool earned = false;
	float distanceToPlayer;
	
	// Object's cached components
	Transform myTr;
	GameObject myGo;
	SpriteRenderer mySpriteRdr;
	Animator myAnimator;
	
	// Player :
	GameObject player;
	Transform playerTr;
	PlayerScript playerScript;
	
	
	public void OnBecameInvisible()
	{
		
		// On became invisible, destroy object
		if (gameObject.activeInHierarchy == true) StartCoroutine(DestroyObject());
	
	}
	
	
	public IEnumerator DestroyObject()
	{
	
		myAnimator.SetBool("Coin_Grab_Bool", false);
		
		yield return null; // yield function is needed because an animation is playing
		
		// Destroy (gameObject); // REGULAR METHOD : // Kills the game object
			
		// pooling method : We set the object to inactive so it can be retreived from object pool list (in "ObjectPoolerScript")
		if (gameObject.activeInHierarchy == true)
		{
			myGo.SetActive(false);
			earned = attracted = false;
			moveToPlayerDelay = 0.0f;
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
		playerScript = player.GetComponent<PlayerScript>();
		playerTr = player.transform;
		
		StartCoroutine(Prepare ());
		
	}
	
	public IEnumerator Prepare()
	{
	
		// Compute random values we will use for movement
		randomX = UnityEngine.Random.Range(-0.25f, 0.10f);
		randomY = UnityEngine.Random.Range(-0.05f, 0.05f);
		randomSmoothLerp = UnityEngine.Random.Range(3.0f, 5.0f);
		
		yield return null;
		
		ready = true;
		
	}
	
	public void Update()
	{
		
		// If script isn't ready or coin is already earned, abort the function
		if (ready == false || earned == true) return;
		
		// Delay the movement of the coins - increment "moveToPlayerDelay"
		// this delay the movement when coin as just appeared
		if (moveToPlayerDelay < moveToPlayerDelayMax) moveToPlayerDelay = moveToPlayerDelay + 0.1f;
		
		basePosition = myTr.position;
		
		newPosition = new Vector3(myTr.position.x+randomX-0.05f,myTr.position.y+randomY,myTr.position.z);
		
		distanceToPlayer = Vector2.Distance(new Vector2(playerTr.position.x,playerTr.position.y), new Vector2(basePosition.x, basePosition.y));
		//Debug.Log ("distanceToPlayer" + distanceToPlayer + "Attracted = " + attracted +"   playerMoneyAttractionRange " + playerMoneyAttractionRange, gameObject);
		
		
		// If money is in player range, give it the "attracted" attribute
		if (attracted == false && distanceToPlayer <= playerMoneyAttractionRange && playerScript.canMove == true) attracted = true;
		
		// If money is attracted and very close to player, then player earn it
		else if (attracted == true && distanceToPlayer < 0.1f && playerScript.canMove == true)
		{
			earned = true;
			//playerScript.money = playerScript.money + 1; //TODO - not implemented
			
			// Set the animator bool to play the grab animation	
			myAnimator.SetBool("Coin_Grab_Bool", true);
			
			audio.Play();		  
		}	
	
		// If attracted, move towards player position
		else if (attracted == true && moveToPlayerDelay >= moveToPlayerDelayMax && playerScript.canMove == true) 
		{
			myTr.position = Vector3.MoveTowards(basePosition, playerTr.position, Time.deltaTime);
		}
		
	
		// Else, move towards new position ("newPosition")
		else
		{
			myTr.position = Vector3.Lerp(basePosition, newPosition, randomSmoothLerp * Time.deltaTime);
		
		}	
	
	}

}




