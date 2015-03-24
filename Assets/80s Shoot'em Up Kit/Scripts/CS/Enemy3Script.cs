// ENEMY 3 SCRIPT CS 20151402_0647 //

using UnityEngine;
using System;
using System.Collections;


public class Enemy3Script:MonoBehaviour
{
	
	// Object values :
	public int hp = 1;
	public int scoreValue = 100;
	[HideInInspector] int damageValue = 1;
	public float speed = 0.1f;
	
	// Object cached components :
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	// the base position used for Y movement
	float basePosY;
	
	bool asleep = true;
	
	// Player values :
	//private var player : GameObject;
	PlayerScript playerScript;
	
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
	
	
	public void Start()
	{
		
		explosionPool = GameObject.Find("ObjectPool EnemyExplosions").GetComponent<ObjectPoolerScript>(); 
		coinPool = GameObject.Find("ObjectPool ItemCoins").GetComponent<ObjectPoolerScript>(); 
	
		// cache object transform and renderer
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
	
		playerScript = GameObject.FindWithTag ("Player").GetComponent<PlayerScript>(); 
		
		// Set "basePosY" value to object Y position
		basePosY = myTr.position.y;
	
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
		
		// If our object is sleeping then abort the function
		if (asleep == true) return;

		myTr.position = new Vector3 (myTr.position.x - speed * Time.deltaTime, basePosY + 0.25f * Mathf.Sin(Time.time * speed * Mathf.PI), myTr.position.z);

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
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Player"))
		{	
			//Debug.Log("Enemy collided with player", gameObject);
			other.SendMessageUpwards ("ApplyDamage", damageValue, SendMessageOptions.DontRequireReceiver);
		}
	
	}
}







