// ENEMY 5 SCRIPT CS 20151402_0647 //

// Unlike 'Enemy 4' (turret), with Enemy 5 we directly change the gameObject direction rather than create child sprite and script.
// It gives another point of view on how to deal with directions in Unity 2D.
// We also use inversed gravity, to give 'Enemy 5' the possibility to walk on ceiling.
// To do this, simply multiply the 2d rigidbody gravity by the up direction of the tranform (rigidbody2D.gravityScale * transform.up.y)
// Another difference compared to previous enemies, this time we do not use trigger in the collider

using UnityEngine;
using System;
using System.Collections;


public class Enemy5Script:MonoBehaviour
{
	
	// Is the game paused ? (sent by "MainScript()")
	[HideInInspector]
	public bool gamePause;	
	
	// Object values :
	public int hp = 1;
	public int scoreValue = 100;

	[HideInInspector]
	public int damageValue = 1;

	public float speed = 0.1f;
	
	public bool randomFire = true;
	public float actionDelayMax = 20.0f;
	public float actionDelayTimer = 0.0f;
	
	bool asleep = true;
	
	bool onAir = false;
	
	public LayerMask groundLayerMask;
	
	// Object references :
	Transform myTr;
	Rigidbody2D myRb;
	SpriteRenderer mySpriteRdr;
	Animator myAnimator;
	
	// Player references :
	GameObject player;
	Transform playerTr;
	Vector3 playerTrRelative;
	PlayerScript playerScript;
	
	// Weapon :
	public AudioClip bulletSound;
	public bool canFire = true;
	ObjectPoolerScript weaponPool; // This targets the enemy bullets' ObjectPool
	bool firing = false; // is enemy actually firing ?
	EnemyWeaponScript enemyWeaponScript;
	GameObject bulletClone;
	
	// Explosion :
	public AudioClip deathSound;
	ObjectPoolerScript explosionPool;
	//var explosion : GameObject;
	GameObject explosionClone;
	
	// Coins :
	public bool giveCoins = true;
	ObjectPoolerScript coinPool;
	//var coin : GameObject;
	GameObject coinClone;
	Vector3 coinPos;
	int i;
	
	
	
	public void OnBecameVisible()
	{
	
		// Wake up object
		WakeUp();
		myTr.collider2D.enabled = true;
		
	}
	
	public void WakeUp()
	{
	
		// Wake up object
		asleep = false;
		myRb.isKinematic = false;
		
	}
	
	public void OnBecameInvisible()
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
		
		// We cache components, this is usefull for performance optimisation !
		myTr = transform;	// We will now use "myTr" instead of "transform"
		myRb = rigidbody2D;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		myAnimator = myTr.GetComponent<Animator>();
		
		// Player cached components
		player = GameObject.FindWithTag ("Player");
		playerTr = player.transform;
		playerScript = player.GetComponent<PlayerScript>();
		
		// If enemy is upside-down (transform Z rotation = 180), inverse gravity !
		myRb.gravityScale = myRb.gravityScale * myTr.up.y;
			
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
		
		// If game is paused abort the function  (sent by "MainScript()")
		if (gamePause == true) return;
		
		// If our object is sleeping then abort the function
		if (asleep == true) return;	
		
		// If our object is not allowed to fire, or isn't grounded, then abort the function
		if (canFire == false || onAir == true) return;	
		
		// Increment action delay timer
		actionDelayTimer ++;
		
		// If action delay timer doesn't reach the max delay value, return the function
		if (actionDelayTimer < actionDelayMax) return;
		
		// Random fire option
		if (randomFire == true && UnityEngine.Random.Range (1,5) < 4)
		{
			actionDelayTimer = 0.0f;
			return; // abort
		}
		
		// Else, fire
		
		playerTrRelative = myTr.InverseTransformPoint(playerTr.position);
		
		// distance between object and player
		Vector3 playerDist = player.transform.position - transform.position;
		
		// if player is above object and in a given range and position, then fire - note that "LaunchProjectile()" is called by animation event ('Enemy 5 fire Animation')
		if (playerTrRelative.x > 0.32f && playerTrRelative.x < 1 && playerTrRelative.y > 0.32f && playerDist.sqrMagnitude > 0.2f && playerDist.sqrMagnitude < 1.5f && hp>0)
		{
			firing = true;
			myAnimator.SetBool("Firing_Bool", true); // "LaunchProjectile()" is called by 'Enemy 5 fire Animation' !
			myRb.isKinematic = true;
			actionDelayTimer = 0.0f;
		}	
		
	}
	
	public void FixedUpdate()
	{	
		
		// If game is paused abort the function  (sent by "MainScript()")
		if (gamePause == true) return;
		
		// If our object is sleeping then abort the function
		if (asleep == true) return;
		
		// if our object is firing, abort the function
		if (firing == true) return;
		
		// Cast a ray under the object
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(myTr.position.x - 0.12f * myTr.right.x, myTr.position.y), -Vector2.up * myTr.up.y,  Mathf.Infinity, groundLayerMask.value);
	
		// Mathf.Abs Returns the absolute value of value.
		float groundDist = Mathf.Abs(hit.point.y - transform.position.y);
		
		// If it hits ground...
		if ( hit /*!= null*/ && groundDist < 0.16f )
		{
			Debug.DrawRay (new Vector2(myTr.position.x - 0.12f * myTr.right.x, myTr.position.y), -Vector3.up * myTr.up.y, Color.green);	
			  
			if (onAir == true) onAir = false;
		}
		
		else // if (hit == null)
		
		{
			Debug.DrawRay (new Vector2(myTr.position.x - 0.12f * myTr.right.x, myTr.position.y), -Vector3.up * myTr.up.y, Color.red);	  
			
			if (onAir == false) onAir = true;
		}
		
		if (onAir == false) myRb.velocity = new Vector2 (myTr.right.x * speed, myRb.velocity.y);

		else myRb.velocity = new Vector2 (0.0f, myRb.velocity.y);
		
	}
	
	// "LaunchProjectile()" is called by animation Event in 'Enemy 5 Fire Animation'
	public IEnumerator LaunchProjectile()
	{	
		
		// check if the object is already active - this is because this function is called by animation event !
		if (gameObject.activeInHierarchy == false) yield break; // FIX CS return null;
		
		canFire = false; // the object is no more allowed to fire (this is a unique shot)
		
		bulletClone = weaponPool.Spawn();
		bulletClone.transform.position = new Vector3 (myTr.position.x + 0.06f * myTr.right.x, myTr.position.y + 0.16f * myTr.up.y, myTr.position.z);
		
		enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
		enemyWeaponScript.direction = new Vector2 (myTr.right.x, myTr.up.y);
		
		audio.clip = bulletSound;
		audio.Play();
		
		yield return null;
		
		myAnimator.SetBool("Firing_Bool", false);
		
		yield return new WaitForSeconds(1.3f);	
		
		firing = false;
		myRb.isKinematic = false;
		
	}
	
	public IEnumerator ApplyDamage(int damage)
	{	
	
		// Ensure that object receiving damage is not sleeping (and therefore out of screen)
			if (asleep == true) yield break;// FIX CSreturn null;
		
		// Substract 1 health point
		hp = hp-1;
		
		// If health > 0, then launch "Damageblink()" function
		if (hp > 0) StartCoroutine(DamageBlink ());
	
		// Else, object is destroyed
		else
		{	
			audio.clip = deathSound;
			audio.Play();
			
			myTr.collider2D.enabled = false;
			myTr.renderer.enabled = false;
					
			//REGULAR METHOD : INSTANTIATE (unoptimised)
			//explosionClone = Instantiate(explosion, Vector3 (myTr.position.x, myTr.position.y, myTr.position.z), Quaternion.identity);
				
			// Pooling Method : grab object from "ObjectPooler"'s gameObjects list
			explosionClone = explosionPool.Spawn();
			explosionClone.transform.position = myTr.position;
	
			// search for "GiveCoins" value in player preferences
			if (PlayerPrefs.HasKey("Give coins") == false || PlayerPrefs.GetInt("Give coins") == 1) giveCoins = false; // we use the value 2 for true, and one for false
			else if (PlayerPrefs.GetInt("Give coins") == 2) giveCoins = true;
			
			// If "GiveCoins" is true, spawn coins
			if (giveCoins == true)
			{
				coinPos = myTr.position;
				for(i = 0; i<scoreValue*0.1f; i++)
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

	public void OnCollisionEnter2D(Collision2D other)
	{
	
		if (other.gameObject.CompareTag("Player"))
		{	
			// Apply damage to player
			other.gameObject.SendMessageUpwards ("ApplyDamage", damageValue, SendMessageOptions.DontRequireReceiver);
		}
		
	}

}














