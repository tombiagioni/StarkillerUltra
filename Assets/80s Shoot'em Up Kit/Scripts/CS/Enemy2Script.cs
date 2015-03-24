// ENEMY 2 SCRIPT CS 20151402_0647 //

using UnityEngine;
using System;
using System.Collections;


public class Enemy2Script:MonoBehaviour
{
	
	[HideInInspector] bool gamePause;	// Is the game paused ? (sent by "MainScript()")
	
	// Object values :
	public int hp = 1; // Health Points value
	public int scoreValue = 100;
	[HideInInspector] int damageValue = 1;
	public float speed = 0.1f;
	
	// Sprite references :
	public Sprite goForward;
	public Sprite goUp;
	public Sprite goDown;
	
	bool asleep = true;
	
	// Vector 2 direction of the object
	Vector2 seekDir = new Vector2(-1.0f,0.0f);
	
	// delay before changing direction ("seekDir")
	float seekDelay = 0.0f;
	float seekDelayMax = 2.0f;
	
	// Object cached components :
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	// Player cached components :
	GameObject player;
	Transform playerTr;
	PlayerScript playerScript;
	
	// Explosion :
	//var deathSound : AudioClip; 				// unused (already set up in audio source)
	//var explosion : GameObject;				// Regular - unoptimized way to instantiate object (we use object pooling instead)
	ObjectPoolerScript explosionPool;		// This targets the explosion ObjectPool
	GameObject explosionClone;	// value to cache spawned object
	
	// Coins :
	public bool giveCoins = true;
	//var coin : GameObject;
	ObjectPoolerScript coinPool;
	GameObject coinClone;
	Vector3 coinPos;
	int i;
	
	// Upgrade :
	public bool giveUpgrade = false;
	ObjectPoolerScript upgradePool;
	public Color upgradeHolderColor = Color.yellow;
	
	
	public void OnBecameVisible()
	{
		
		// On became visible, wake up the object
		asleep = false;
		myTr.collider2D.enabled = true;
		
	}
	
	public void OnBecameInvisible()
	{
		
		// On became invisible, destroy object
		if (gameObject.activeInHierarchy == true) StartCoroutine(DestroyObject());
	
	}
	
	public IEnumerator DestroyObject()
	{
		
		yield return new WaitForSeconds (audio.clip.length); // Wait for the end of explosion audio clip
		
		if (gameObject.activeInHierarchy == true) Destroy (gameObject); // Kills the game object
		
	}
	
	// Called by 'MainScript'
	public void OnPauseGame()
	{
	
		gamePause = true;
		
	}
	
	// Called by 'MainScript'
	public void OnResumeGame()
	{
	
		gamePause = false;
		
	}
	
	public void Start()
	{
		
		explosionPool = GameObject.Find("ObjectPool EnemyExplosions").GetComponent<ObjectPoolerScript>(); 
		coinPool = GameObject.Find("ObjectPool ItemCoins").GetComponent<ObjectPoolerScript>(); 
		upgradePool = GameObject.Find("ObjectPool ItemUpgrades").GetComponent<ObjectPoolerScript>(); 
	
		// cache object transform and renderer
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		playerTr = player.transform;
		
		// If the enemy hold an upgrade, give the user a visual hint !
		if (giveUpgrade == true) mySpriteRdr.color = upgradeHolderColor;
		
		StartCoroutine(DisableCollider());
		
	}
	
	// Disable collider some times after scene has initialised
	// Collider is disabled to be sure that nothing will reach an out of screen object
	// Disabling is delayed to let "MainScript"'s "Spawn()" function, wich use enemies colliders, operate first.
	public IEnumerator DisableCollider()
	{
		
		yield return new WaitForSeconds(0.3f);
		
		myTr.collider2D.enabled = false;
		
	}
	
	public void Update()
	{
	
		// If game is paused abort the function (sent by "MainScript()")
		if (gamePause == true) return;
		
		// If our object is sleeping then abort the function
		if (asleep == true || hp <=0) return;
		
		// Direction : Forward
		if (seekDir == new Vector2(-1.0f, 0.0f) || seekDir == new Vector2(0.0f, 0.0f))
		{	
			myTr.position = new Vector3 (myTr.position.x - speed * Time.deltaTime, myTr.position.y, myTr.position.z);
			if (mySpriteRdr.sprite != goForward) mySpriteRdr.sprite = goForward;
		}
		
		// Direction : Up
		else if (seekDir == new Vector2(-1.0f, 1.0f))
		{
			myTr.position = new Vector3 (myTr.position.x - speed * 0.5f * Time.deltaTime, myTr.position.y + speed * 0.5f * Time.deltaTime, myTr.position.z);

			if (mySpriteRdr.sprite != goUp) mySpriteRdr.sprite = goUp;	
		}
		
		// Direction : Down
		else if (seekDir == new Vector2(-1.0f, -1.0f))
		{
			myTr.position = new Vector3 (myTr.position.x - speed * 0.5f * Time.deltaTime, myTr.position.y - speed * 0.5f * Time.deltaTime, myTr.position.z);

			if (mySpriteRdr.sprite != goDown) mySpriteRdr.sprite = goDown;
		}
		
		// If seek delay doesn't reach max delay, increment it and abort the function
		if (seekDelay < seekDelayMax) {seekDelay = seekDelay + 0.1f; return;}
		
		// (else) Reset the delay counter
		seekDelay = 0.0f;
		
		// if gameObject is behind player, change direction to forward
		if (myTr.position.x < playerTr.position.x || mySpriteRdr.isVisible == false) seekDir = new Vector2(-1.0f, 0.0f);
	
		
		// Else if player is above gameObject, change direction to down
		else if (myTr.position.y < playerTr.position.y - 0.03f) seekDir = new Vector2(-1.0f, 1.0f);
	
		
		// else If player is under gameObject, change direction to up
		else if (myTr.position.y > playerTr.position.y + 0.03f) seekDir = new Vector2(-1.0f, -1.0f);
	
		
		// else player is in front of gameObject, change direction to forward
		else seekDir = new Vector2(-1.0f, 0.0f);
	
	}
	
	public IEnumerator ApplyDamage(int damage)
	{
		
		// Ensure that object receiving damage is not sleeping (and therefore out of screen)
		if (asleep == true) yield break;//return null; // FIX CS 13 01 2015 VERIFIER !!
		hp = hp-1;
		
		if (hp > 0)
		{
			StartCoroutine(DamageBlink ());
			// ... play an impact sound
		}
		
		else 
		{	
			audio.Play();
			myTr.collider2D.enabled = false;
			myTr.renderer.enabled = false;
			
			//REGULAR METHOD : INSTANTIATE (unoptimised)
			//explosionClone = Instantiate(explosion, Vector3 (myTr.position.x, myTr.position.y, myTr.position.z), Quaternion.identity);
			
			// Pooling Method : grab object from "ObjectPooler"'s gameObjects list
			explosionClone = explosionPool.Spawn();
			explosionClone.transform.position = myTr.position;
	
			// search for "giveCoins" value in player preferences
			if (PlayerPrefs.HasKey("Give coins") == false || PlayerPrefs.GetInt("Give coins") == 1) giveCoins = false; // we use the value 2 for true, and one for false
			else if (PlayerPrefs.GetInt("Give coins") == 2) giveCoins = true;
			
			if (giveCoins == true)
			{
				coinPos = myTr.position;
				
				for(i = 0; i<scoreValue/10; i++)
				{ 
					//REGULAR METHOD : INSTANTIATE (unoptimised)
					//coinClone = Instantiate(coin, coinPos, Quaternion.identity);
					
					// Pooling Method : grab object from "ObjectPooler"'s gameObjects list
					coinClone = coinPool.Spawn();
					coinClone.transform.position = coinPos;
					
					yield return null;
				}
			}
			
			if (giveUpgrade == true)
			{
				GameObject upgradeClone = upgradePool.Spawn();
				upgradeClone.transform.position = myTr.position;
			}
			
			playerScript.UpdateScore (scoreValue);
			StartCoroutine(DestroyObject());
		}
		
	}
	
	public IEnumerator DamageBlink()
	{
	
		mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, 0.0f);

		yield return new WaitForSeconds (0.05f);

		mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, 1.0f);
		
	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Player"))
		{	
			//Debug.Log("Enemy collided with player", gameObject);
			other.SendMessageUpwards ("ApplyDamage", damageValue, SendMessageOptions.DontRequireReceiver);
		}
	
	}

}






