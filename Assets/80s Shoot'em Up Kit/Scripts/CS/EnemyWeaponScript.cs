// ENEMY WEAPON SCRIPT CS 20150119_1051 //

using UnityEngine;
using System;
using System.Collections;


public class EnemyWeaponScript:MonoBehaviour
{
	
	// Object values :
	public int type = 1; // 1 = simple bullet  , ...
	public int damage = 1;
	public float speed = 1.0f;
	
	public Vector2 direction; // direction of bullet's movement
	
	// Object references :
	GameObject myGo;
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	// Camera :
	//Camera cam;
	
	// Player :
	//GameObject player;
	//PlayerScript playerScript;
	
	// Impact (unused) :
	// var impact : GameObject;
	// private var impactClone : GameObject;
	
	public IEnumerator Start()
	{
		
		// cache object gameObject, transform and renderer
		myGo = gameObject;
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		
		//cam = Camera.main;
		//player = GameObject.FindWithTag ("Player");
		//playerScript = player.GetComponent<PlayerScript>();
		
		yield return null;
		
		if (mySpriteRdr.isVisible == false) OnBecameInvisible();
		
	}
	
	public void OnBecameInvisible()
	{
	
		// On became invisible, destroy object
		if (gameObject.activeInHierarchy == true) DestroyObject();
	
	}
	
	public void Update()
	{
	
			myTr.position = myTr.position + new Vector3(direction.x * speed, direction.y * speed, 0.0f)  * Time.deltaTime;
		
	}
	
	public void DestroyObject()
	{
		
		// REGULAR METHOD : // Kills the game object
		// Destroy (gameObject);
		
		// pooling method : We set the object to inactive so it can be retreived from object pool list (in "ObjectPoolerScript")
		myGo.SetActive(false);	
		
	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Player"))
		{		
			//Debug.Log("Enemy bullet collided with player", gameObject);
			other.SendMessageUpwards ("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
			
			// Call the DestroyObject function
			DestroyObject();
		}
	
	}
}




