// ENEMY 6 SCRIPT CS 20151402_0709 //

// Mini-boss example - This one stops the scrolling until being destroyed

using UnityEngine;
using System;
using System.Collections;


public class Enemy6Script:MonoBehaviour
{
	
	// Is the game paused ? (sent by "MainScript()")
	[HideInInspector]
	public bool gamePause;	


	// Object values :
	public int hp = 30;
	public int scoreValue = 1000;

	[HideInInspector]
	public int damageValue = 1;
	
	float actionDelayMax = 100.0f;
	float actionDelayTimer = 0.0f;
	float shootDelayMax = 100.0f;
	float shootDelayTimer = 0.0f;
	
	// targets the state of enemy's position : 1 = center, 2 = up, 3 = center, 4 = down
	public int posState = 1;
	// the target position used for Y movement
	float targetPosY;
	
	// Does the enemy must stop the camera scrolling ?
	public bool stopScrolling = true;
	public float stopScrollingDelay = 2.0f;
	
	// Movement smoothDamp values
	public float smoothTime = 0.1f;
	public float yVelocity = 0.0f;
	public float newPositionY;
	
	// Object cached components :
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	bool asleep = true;
	
	// Player values :
	//private var player : GameObject;
	PlayerScript playerScript;
	
	// Weapon :
	public ObjectPoolerScript weaponPool; // This targets the enemy bullets' ObjectPool
	Vector3 bulletDir;
	Vector3 bulletPos;
	EnemyWeaponScript enemyWeaponScript;
	GameObject bulletClone;
	public AudioClip bulletSound;
	
	// Play special theme when the enemy appears ?
	public bool playAudioBossTheme = true;
	// boss audio clips :
	public AudioClip bossMusic;
	public AudioClip victorySound;
	
	// Explosion :
	public AudioClip deathSound;
	public AudioClip armorSound;
	ObjectPoolerScript explosionPool;
	//var explosion : GameObject;
	GameObject explosionClone;
	
	Camera cam;
	MainScript mainScript; // "Main Script" component attached to camera
	
	// Coins :
	public bool giveCoins = true;
	ObjectPoolerScript coinPool;
	//var coin : GameObject;
	GameObject coinClone;
	Vector3 coinPos;
	int i;
	
	public IEnumerator OnBecameVisible()
	{
	
		// On became visible, wake up the object
		asleep = false;
		myTr.collider2D.enabled = true;
		
		// Play special music theme ?
		if (playAudioBossTheme == true) StartCoroutine(LaunchMusic());
		
		// Pause camera scrolling ?
		if (stopScrolling == true)
		{
			yield return new WaitForSeconds (stopScrollingDelay);
			playerScript.camScrollEnabled = false;
		}
		
	}
	
	public IEnumerator LaunchMusic()
	{
		
		// Find MainScript
		cam = Camera.main;
		mainScript = cam.GetComponent<MainScript>();
		
		// Stop all coroutines relative to the audio in "MainScript"
		mainScript.StopCoroutine("MusicStop");
		mainScript.StopCoroutine("MusicPlay");
		
		mainScript.audio.clip = bossMusic;
		mainScript.audio.Stop();
		
		yield return null;
	
		mainScript.audio.loop = true;
		mainScript.audio.Play();
	
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
		
		// Resume camera scrolling
		if (stopScrolling == true) playerScript.camScrollEnabled = true;
		
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
		
		// cache object transform and renderer
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		
		explosionPool = GameObject.Find("ObjectPool EnemyExplosions").GetComponent<ObjectPoolerScript>(); 
		coinPool = GameObject.Find("ObjectPool ItemCoins").GetComponent<ObjectPoolerScript>(); 
		weaponPool = GameObject.Find("ObjectPool EnemyBullets").GetComponent<ObjectPoolerScript>(); 
		
		playerScript = GameObject.FindWithTag("Player").GetComponent<PlayerScript>();
	
		// Set "targetPosY" value to object Y position
		targetPosY = 0.0f;
	
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
	
		smoothTime = 0.1f;
		yVelocity = 0.0f;
		
		newPositionY = Mathf.SmoothDamp(transform.position.y, targetPosY, ref yVelocity, smoothTime);

		transform.position = new Vector3 (transform.position.x, newPositionY, transform.position.z);

		
		actionDelayTimer++;
		shootDelayTimer++;
		
		
		if (actionDelayTimer > actionDelayMax)
		{
			
			if (posState < 4) posState++;
			else posState = 1;
			
			if (posState == 1 || posState == 3) targetPosY = 0.0f;
			else if (posState == 2) targetPosY = 0.6f;
			else if (posState == 4) targetPosY = -0.6f;
			
			actionDelayTimer = 0.0f;		
		}
		
		if (shootDelayTimer > shootDelayMax && hp>0) //shoot
		{
			LaunchProjectile();
			shootDelayTimer = 0.0f;
		}
		
	}
	
	public void LaunchProjectile()
	{
	
		//REGULAR METHOD : INSTANTIATE bullet (unoptimised)
		//bulletClone = Instantiate(bullet, bulletPos, Quaternion.identity);
			
		// Pooling Method : grab a bullet from "ObjectPooler"'s gameObjects list
			
		// Prepare the bullet position
		bulletPos = myTr.TransformPoint(-0.1f, 0.0f, 0.0f);
		// Prepare the bullet direction
		bulletDir = new Vector3(-1.0f, 0.0f, 0.0f);
		bulletClone = weaponPool.Spawn(); // Spawn from pool
		bulletClone.transform.position = bulletPos;
		enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
		enemyWeaponScript.direction = (Vector2)bulletDir;
		
		// Prepare the bullet position
		bulletPos = myTr.TransformPoint(-0.1f, 0.0f, 0.0f);
		// Prepare the bullet direction
		bulletDir = new Vector3(-1.0f, 0.3f, 0.0f);
		bulletClone = weaponPool.Spawn();
		bulletClone.transform.position = bulletPos;
		enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
		enemyWeaponScript.direction = (Vector2)bulletDir;
		
		// Prepare the bullet position
		bulletPos = myTr.TransformPoint(-0.1f, 0.0f, 0.0f);
		// Prepare the bullet direction
		bulletDir = new Vector3(-1.0f, -0.3f, 0.0f);
		bulletClone = weaponPool.Spawn();
		bulletClone.transform.position = bulletPos;
		enemyWeaponScript = bulletClone.GetComponent<EnemyWeaponScript>() as EnemyWeaponScript;
		enemyWeaponScript.direction = (Vector2)bulletDir;
		
			
		audio.clip = bulletSound;
		audio.Play();
		
	}
	
	public IEnumerator ApplyDamage(int damage)
	{	
	
		// Ensure that object receiving damage is not sleeping (and therefore out of screen)
		if (asleep == true) yield break;//return null; // FIX CS 13 01 2015 VERIFIER !!
		
		hp = hp-1;
		
		if (hp > 0)
		{
			StartCoroutine(DamageBlink ());
			audio.clip = armorSound;
			audio.Play();
			
			// Raise the difficulty for our boss fight !
			
			if (hp < 20) { actionDelayMax = shootDelayMax = 60.0f; }
		
			if (hp < 10) { actionDelayMax = shootDelayMax = 35.0f; }
			
		}
		
		else
		{	
			audio.Play();
			myTr.collider2D.enabled = false;
			yield return null;
			asleep = true;
			
			// Stop all coroutines relative to the audio in "MainScript"
			mainScript.StopCoroutine("MusicPlay");
			mainScript.StopCoroutine("MusicStop");
			yield return null;
			mainScript.audio.Stop();
			
			StartCoroutine(mainScript.MusicPlay(victorySound, false, false, 0.0f)); // music, musicLoop, musicWaitForClipEnd, musicDelay
			
			playerScript.UpdateScore (scoreValue);
			
			//REGULAR METHOD : INSTANTIATE (unoptimised)
			//explosionClone = Instantiate(explosion, Vector3 (myTr.position.x, myTr.position.y, myTr.position.z), Quaternion.identity);
			
			// Pooling Method : grab object from "ObjectPooler"'s gameObjects list
			// Create a big explosion !
			for(int i = 0; i < 20; i++)
			{
				explosionClone = explosionPool.Spawn();
				float randomXPos = UnityEngine.Random.Range(-0.08f, 0.08f);
				float randomYPos = UnityEngine.Random.Range(-0.08f, 0.08f);

				explosionClone.transform.position = new Vector3(myTr.position.x + randomXPos, myTr.position.y + randomYPos, explosionClone.transform.position.z);

				audio.clip = deathSound;
				audio.Play();
				yield return new WaitForSeconds (audio.clip.length);
			}
			
			var tmpColor = mySpriteRdr.color;
			tmpColor.a = 0.0f;
			mySpriteRdr.color = tmpColor;
			
			// search for "GiveCoins" value in player preferences
			if (PlayerPrefs.HasKey("Give coins") == false || PlayerPrefs.GetInt("Give coins") == 1) giveCoins = false; // we use the value 2 for true, and one for false
			else if (PlayerPrefs.GetInt("Give coins") == 2) giveCoins = true;
			
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
			
			yield return new WaitForSeconds (2.0f);
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





