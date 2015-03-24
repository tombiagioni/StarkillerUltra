// ENEMY 4 SCRIPT CS 20151402_0647 //

// Enemy 4 Entity gives you an exemple of how to deal with sprite direction independently of object direction.
// As sprite scale is not accessible direectly, we create a child sprite contained in this gameObject, then we inverse is scale when needed to mirror it.
// Direction of parent gameObject never change.

using UnityEngine;
using System;
using System.Collections;


public class Enemy4Script:MonoBehaviour
{
	
	// Is the game paused ? (sent by "MainScript()")
	[HideInInspector]
	public bool gamePause;

	// Object values :
	public int hp = 1;
	public int scoreValue = 100;

	[HideInInspector]
	public int damageValue = 1;

	public float actionDelayMax	= 10.0f;
	public float actionDelayTimer = 0.0f;
	public float shootDelayMax = 100.0f;
	public float shootDelayTimer = 0.0f;
	
	// Object references :
	Transform myTr;
	public Transform mySpriteTr;
	SpriteRenderer mySpriteRdr;
	
	// Sprite references :
	public Sprite spriteHorizontal;
	public Sprite spriteHorizontalFire;
	public Sprite spriteDiagonal;
	public Sprite spriteDiagonalFire;
	public Sprite spriteVertical;
	public Sprite spriteVerticalFire;
	
	bool asleep = true;
	
	// Player :
	GameObject player;
	Transform playerTr;
	Vector3 playerTrRelative;
	PlayerScript playerScript;
	
	// Weapon :
	ObjectPoolerScript weaponPool; // This targets the enemy bullets' ObjectPool
	Vector3 bulletDir;
	Vector3 bulletPos;
	bool firing = false; // is enemy actually firing ?
	EnemyWeaponScript enemyWeaponScript;
	GameObject bulletClone;
	public AudioClip bulletSound;
	
	// Explosion :
	public AudioClip deathSound;
	ObjectPoolerScript explosionPool;
	GameObject explosionClone;
	
	// Coins :
	public bool giveCoins = true;
	ObjectPoolerScript coinPool;
	GameObject coinClone;
	Vector3 coinPos;
	int i;
	
	// "OnBecameVisible()" will not work by itself since there is no renderer on the gameObject.
	// Instead, "SharedOnBecameVisible()" is called by the child's script ("Enemy4SpriteScript")
	public void SharedOnBecameVisible()
	{
	
		// Wake up object
		asleep = false;
	
	}
	
	// "OnBecameInvisible()" will not work by itself since there is no renderer on the gameObject.
	// Instead, "SharedOnBecameInvisible()" is called by the child's script ("Enemy4SpriteScript")
	public void SharedOnBecameInvisible()
	{
	
		// Destroy object
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
		weaponPool = GameObject.Find("ObjectPool EnemyBullets").GetComponent<ObjectPoolerScript>(); 
		
		// cache object components
		myTr = transform;
		mySpriteRdr = mySpriteTr.GetComponent<SpriteRenderer>();
		
		// cache player components
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		playerTr = player.transform;
	
	}
	
	public void Update()
	{	
		
		// If game is paused abort the function (sent by "MainScript()")
		if (gamePause == true) return;
		
		// If our object is sleeping then abort the function
		if (asleep == true) return;
		
		// Increment timers
		actionDelayTimer ++;
		shootDelayTimer ++;
		
		// If action delay timer doesn't reach the max delay value, or if enemy is already firing, return the function
		if (actionDelayTimer<actionDelayMax || firing == true) return;
		
		// compare player position relative to object transform;
		playerTrRelative = myTr.InverseTransformPoint(playerTr.position); //Debug.Log ("playerTrRelative : " + playerTrRelative, gameObject);
		
		// Compare the player position relative to object, and set the turret sprite accordingly
		
		if (playerTrRelative.x > - 0.2f && playerTrRelative.x < 0.2f) mySpriteRdr.sprite = spriteVertical;	
	
		
		else if (playerTrRelative.x < 0.2f)
		{
			if (playerTrRelative.y <  0.2f) {mySpriteRdr.sprite = spriteHorizontal;}
			else {mySpriteRdr.sprite = spriteDiagonal;}
			mySpriteTr.localScale = new Vector3 (1.0f, mySpriteTr.localScale.y, mySpriteTr.localScale.z);
		}
		
		else // if (playerTrRelative.x > - 0.2)
		{
			if (playerTrRelative.y <  0.2f) {mySpriteRdr.sprite = spriteHorizontal;}
			else {mySpriteRdr.sprite = spriteDiagonal;}
			mySpriteTr.localScale = new Vector3 (-1.0f, mySpriteTr.localScale.y, mySpriteTr.localScale.z);
		}
		
		// Reset the action delay timer
		actionDelayTimer = 0.0f;
		
		// If shoot delay passed, launch projectile
		if (shootDelayTimer > shootDelayMax && hp>0) StartCoroutine(LaunchProjectile());
		
	}
	
	public IEnumerator LaunchProjectile()
	{	
	
		firing = true; // Let the script be aware that enemy is already firing
		
		if (shootDelayTimer > shootDelayMax) //shoot
		{			
			if (mySpriteRdr.sprite == spriteVertical)
			{
				// Align the bullet direction to the object UP direction
				bulletDir = myTr.up;
				
				// Prepare the bullet position
				bulletPos = myTr.TransformPoint(0.0f, 0.2f, 0.0f);
				
				//Debug.Log("Enemy 4 (turret) - myTr.up : " + myTr.up, gameObject);
				
				//REGULAR METHOD : INSTANTIATE bullet (unoptimised)
				//bulletClone = Instantiate(bullet, bulletPos, Quaternion.identity);
					
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = bulletPos;
	
				enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
				
				enemyWeaponScript.direction = (Vector2)bulletDir;
				
				mySpriteRdr.sprite = spriteVerticalFire;
			}
			
			else if (mySpriteRdr.sprite == spriteHorizontal)
			{
				// Align the bullet direction to the object right direction - taking into account the sprite horizontal direction. 
				bulletDir = myTr.right * - mySpriteTr.localScale.x;
				
				// Prepare the bullet position
				bulletPos = myTr.TransformPoint(- 0.1f * mySpriteTr.localScale.x, 0.11f, 0.0f);
	
				//REGULAR METHOD : INSTANTIATE bullet (unoptimised)
				//bulletClone = Instantiate(bullet, bulletPos, Quaternion.identity);
					
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = bulletPos;
	
				enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
				
				enemyWeaponScript.direction = (Vector2)bulletDir;
				
				mySpriteRdr.sprite = spriteHorizontalFire;
			}
			
			else if (mySpriteRdr.sprite == spriteDiagonal)
			{
				bulletDir = myTr.right * - mySpriteTr.localScale.x + myTr.up;
				
				bulletPos = myTr.TransformPoint(- 0.07f * mySpriteTr.localScale.x, 0.17f, 0.0f);
	
				//REGULAR METHOD : INSTANTIATE bullet (unoptimised)
				//bulletClone = Instantiate(bullet, bulletPos, Quaternion.identity);
					
				// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
				bulletClone = weaponPool.Spawn();
				bulletClone.transform.position = bulletPos;
	
				enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
				
				enemyWeaponScript.direction = (Vector2)bulletDir;
				
				mySpriteRdr.sprite = spriteDiagonalFire;	
			}
			
			audio.clip = bulletSound;
			audio.Play();
			
			yield return new WaitForSeconds (0.1f);
			
			shootDelayTimer = 0.0f; // Reset the shoot delay count
			firing = false; // Enemy has finished firing
		}
		
	}
	
	
	// "ApplyDamage()" will not work by itself since there is no collider on the gameObject.
	// Instead, "SharedApplyDamage()" is called by the child's script ("Enemy4SpriteScript"), whose gameObject has a collider.
	public IEnumerator SharedApplyDamage(int damage)
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
			audio.clip = deathSound;
			audio.Play();
			
			mySpriteTr.collider2D.enabled = false;
			mySpriteTr.renderer.enabled = false;
				
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






