// ENEMY1 CHILD SCRIPT CS 20151402_0647 //

using UnityEngine;
using System;
using System.Collections;


public class Enemy1ChildScript:MonoBehaviour
{
	
	// Most values of "Enemy1ChildScript" are populated by "Enemy1GroupScript"
	
	// Object values :
	// "HideInInspector" attribute hides the public variable in the editor inspector
	[HideInInspector] public bool asleep = true;
	[HideInInspector] public int hp;
	[HideInInspector] public int scoreValue;
	[HideInInspector] public int damageValue;
	[HideInInspector] public float speed;
	[HideInInspector] public bool goUp;
	[HideInInspector] public float targetXPos;
	[HideInInspector] public float targetYPos;
	bool targetXPosReached = false;
	bool targetYPosReached = false;
	
	// Object cached components :
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	// Player :
	GameObject player;
	PlayerScript playerScript;
	
	// Explosion :
	//var deathSound : AudioClip;
	[HideInInspector] public ObjectPoolerScript explosionPool;
	//var explosion : GameObject;
	GameObject explosionClone;
	
	// Coins :
	public bool giveCoins = true;
	[HideInInspector] public ObjectPoolerScript coinPool;
	//var coin : GameObject;
	GameObject coinClone;
	Vector3 coinPos;
	int i;
	
	public void OnBecameVisible()
	{
	
		// On became visible, tell the parent ("Enemy1GroupScript()") 
		myTr.parent.SendMessage ("ChildBecameVisible", SendMessageOptions.DontRequireReceiver);
		myTr.collider2D.enabled = true; // Enable collision
		
	}
	
	public void OnBecameInvisible()
	{
		
		// On became invisible, destroy object
		if (gameObject.activeInHierarchy == true) StartCoroutine(DestroyObject());
	
	}
	
	
	public IEnumerator DestroyObject()
	{
	
		yield return new WaitForSeconds (audio.clip.length); // Wait for the end of explosion audio clip
		
		// Kills the game object
		Destroy (gameObject);
		
	}
	
	public void Start()
	{
		
		// cache object transform and renderer
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		
		// Find player gameObject and his script
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		
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
		if (asleep == true || hp <= 0) return;
		
		// Step 1 - Begin - reach a particular horizontal distance
		if (targetXPosReached == false && myTr.position.x >  myTr.parent.transform.position.x + targetXPos)
		{
			myTr.position = new Vector3 (myTr.position.x - speed * Time.deltaTime, myTr.position.y, myTr.position.z);
			return;
		}
		
		// Step 1 - End - horizontal distance reached
		else if (targetXPosReached == false && myTr.position.x <= myTr.parent.transform.position.x + targetXPos)
		{	
			myTr.position = new Vector3 (myTr.parent.transform.position.x + targetXPos, myTr.position.y, myTr.position.z);
			targetXPosReached = true;
			return;
		}
		
		// Step 2 - Begin - reach a particular vertical distance, while executing diagonal step back movement
		else if ((goUp == false && targetXPosReached == true && targetYPosReached == false && myTr.position.y > myTr.parent.transform.position.y + targetYPos)
				|| (goUp == true && targetXPosReached == true && targetYPosReached == false && myTr.position.y  < myTr.parent.transform.position.y + targetYPos))
		{	
			myTr.position = new Vector3 (myTr.position.x + speed * 0.75f * Time.deltaTime, myTr.position.y, myTr.position.z);

			if (goUp == false)	myTr.position = new Vector3 (myTr.position.x, myTr.position.y - speed * 0.75f * Time.deltaTime, myTr.position.z);      
			else				myTr.position = new Vector3 (myTr.position.x, myTr.position.y + speed * 0.75f * Time.deltaTime, myTr.position.z);
			
		}
	
		
		// Step 2 - End - vertical distance reached
		else if ((goUp == false && targetXPosReached == true && targetYPosReached == false && myTr.position.y <= myTr.parent.transform.position.y + targetYPos)
				||(goUp == true && targetXPosReached == true && targetYPosReached == false && myTr.position.y >= myTr.parent.transform.position.y + targetYPos))
		{	
			myTr.position = new Vector3 (myTr.position.x, myTr.parent.transform.position.y + targetYPos, myTr.position.z);
			targetYPosReached = true;
			return;
		}
	
		
		// Step 3 - Step back
		else if (targetXPosReached == true && targetYPosReached == true) myTr.position = new Vector3 (myTr.position.x + speed * Time.deltaTime, myTr.position.y, myTr.position.z);
					
	}
	
	public IEnumerator ApplyDamage(int damage)
	{
	
		// Ensure that object receiving damage is not sleeping (and therefore out of screen)
		if (asleep == true) yield break;// FIX CS return null;
		
		hp = hp-1;
		
		if (hp > 0) StartCoroutine(DamageBlink ());
		
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
			
			playerScript.UpdateScore (scoreValue);
			
			myTr.parent.SendMessage ("SubstractChild", gameObject, SendMessageOptions.DontRequireReceiver);
	
			StartCoroutine(DestroyObject());
		}
		
	}
	
	// When the object take damage but is still alive, make it blink to provide a visual indication
	public IEnumerator DamageBlink()
	{

		mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, 0.0f);

		yield return new WaitForSeconds (0.05f);

		mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, 1.0f);

	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Player")) other.SendMessageUpwards ("ApplyDamage", damageValue, SendMessageOptions.DontRequireReceiver); //Debug.Log("Bullet collided with an enemy", gameObject);

	}
}










