
// PLAYER WEAPON SCRIPT CS 20150119_1114 //

using UnityEngine;
using System;
using System.Collections;


public class PlayerWeaponScript:MonoBehaviour
{
	
	// Sprite references :
	public Sprite spriteType1; // bullet
	public Sprite spriteType2; // diagonal up bullet
	public Sprite spriteType3; // diagonal down bullet
	public Sprite spriteType4; // laser
	public Sprite spriteType5; // bomb
	
	// Object values :
	public int type = 1; // 1 = bullet  , 2 = diagonal up bullet, 3 = diagonal down bullet, 4 = laser, 5 = bomb
	public int damage = 1;
	public float speed = 1.0f;
	public Vector2 direction;
	public LayerMask groundLayerMask;
	bool ready;
	
	// Object Cached components
	GameObject myGo;
	Transform myTr;
	SpriteRenderer mySpriteRdr;
	
	//Camera cam;
	
	// Player references vars
	GameObject player;
	PlayerScript playerScript;
	
	// This targets the impacts ObjectPool
	ObjectPoolerScript impactPool;
	GameObject impactClone;
	
	
	// This targets the player Napalm ObjectPool - Napalm is used with bombs ("type" 5)
	ObjectPoolerScript napalmPool;
	GameObject napalmClone;
	PlayerNapalmScript playerNapalmScript;
	
	// Bomb Napalm values
	//Vector3 bombNapalmBelow = new Vector3(0.0f,0.0f,0.0f);
	int bombNapalmMaxCount = 9;
	float napalmPosX = 0.0f;
	float napalmPreviousHeight = 0.0f;
	float napalmLifeTime = 0.0f;
	float napalmStartDelay = 0.0f;
	int i = 0;
	
	
	public void OnEnable()
	{	
	
		if (ready == false) return;
		
		// When a pooled bullet is enabled, call "OnSpawn()" to see if our bullet is on screen
		StartCoroutine(OnSpawn ());
		
	}
	
	// Called by "OnEnable()"
	public IEnumerator OnSpawn()
	{
	
		// Pause the execution until the following frame
		yield return null;
		
		
		// Our pooled bullet was enabled but seems to be out of screen bounds, so destroy it 
		if (mySpriteRdr.isVisible == false) StartCoroutine(DestroyObject());
		
		else
		
		// Enable collider
		myTr.collider2D.enabled = true;
		
		StartCoroutine(Prepare());
		
	}
	
	
	public void OnBecameInvisible()
	{
		
		// On became invisible, destroy object
		if (gameObject.activeInHierarchy == true) StartCoroutine(DestroyObject());
	
	}
	
	public IEnumerator Start()
	{
		
		// cache object gameObject, transform and renderer
		myGo = gameObject;
		myTr = transform;
		mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
		
		//cam = Camera.main;
		player = GameObject.FindWithTag ("Player");
		playerScript =  player.GetComponent<PlayerScript>();
		
		yield return null;
		
		if (mySpriteRdr.isVisible == false) OnBecameInvisible();
		
		GameObject impactPoolGO = GameObject.Find("ObjectPool Impacts");
		impactPool = impactPoolGO.GetComponent<ObjectPoolerScript>() as ObjectPoolerScript;
		
		GameObject napalmPoolGO = GameObject.Find("ObjectPool PlayerNapalm");
		napalmPool = napalmPoolGO.GetComponent<ObjectPoolerScript>() as ObjectPoolerScript;
		
		StartCoroutine(Prepare ());
		
	}
	
	public IEnumerator Prepare()
	{
	
		if (type == 1)
		{
			mySpriteRdr.sprite = spriteType1; // horizontal bullet
			speed = 4.0f;
			mySpriteRdr.sortingOrder = -2;
		}
		else if (type == 2)
		{
			mySpriteRdr.sprite = spriteType2; // diagonal up bullet
			speed = 4.0f;
			mySpriteRdr.sortingOrder = -2;
		}
		
		else if (type == 3)
		{
			mySpriteRdr.sprite = spriteType3; // diagonal down bullet
			speed = 4.0f;
			mySpriteRdr.sortingOrder = -2;
		}
		
		else if (type == 4)
		{
			mySpriteRdr.sprite = spriteType4; // laser
			speed = 5.0f;
			mySpriteRdr.sortingOrder = 2;
		}
		
		else if (type == 5)
		{
			mySpriteRdr.sprite = spriteType5; // laser
			speed = 0.8f;
			mySpriteRdr.sortingOrder = -2;
		}
		
		yield return null;
		
		ready = true;
	
	}
	
	public void Update()
	{
	
		myTr.position = myTr.position + new Vector3(direction.x * speed, direction.y * speed, 0.0f)  * Time.deltaTime;
	
	}
	
	public IEnumerator DestroyObject()
	{
	
		// Pause the execution until the following frame
		yield return null;
		
		// if type isn't special upgrade 'Bomb', update the player's bullet count - Bomb upgrade is not part of player bullet system
		if (type != 5) playerScript.bulletCount = playerScript.bulletCount-1;
		
		mySpriteRdr.sprite = null;
		
		// REGULAR METHOD : // Kills the game object
		// Destroy (gameObject);
					
		// pooling method : We set the object to inactive so it can be retreived from object pool list (in "ObjectPoolerScript")
		myGo.SetActive(false);	
		
	}
	
	
	public IEnumerator BombLandingDamage()
	{
	
		// Already setup in variables declaration :
		//bombNapalmMaxCount = 9;
		
		napalmLifeTime = 3.0f;
		napalmStartDelay = 0.0f;
		//bombNapalmBelow = transform.TransformDirection (0.03f,-1.0f,0.0f);
		napalmPosX = myTr.position.x;
		napalmPreviousHeight = myTr.position.y;
		
		yield return null;
		
		// Deploy a range of napalm
		for(i= 0; i<bombNapalmMaxCount; i++)
		{
			if (i > 0)
			{
				if (i%2==1) {  // if "i" is even, place to the left and change start and die delays
					napalmPosX = napalmPosX + i * 0.06f;
					napalmStartDelay = napalmStartDelay + 0.1f;
					napalmLifeTime = napalmLifeTime * 0.95f;
				}
				
				else {napalmPosX = napalmPosX - i * 0.06f;} // else if "i" is odd, place to the right
			}
	
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(napalmPosX, napalmPreviousHeight + 0.32f), -Vector2.up,  Mathf.Infinity, groundLayerMask.value);
			
			if ( hit /*!= null*/)
			{
				//Debug.DrawRay (Vector2(napalmPosX, napalmPreviousHeight + 0.32), -Vector2.up, Color.green);
				napalmPosX = hit.point.x;	
	
				napalmClone = napalmPool.Spawn();
				napalmClone.transform.position = new Vector3(hit.point.x, hit.point.y, napalmClone.transform.position.z);
				playerNapalmScript = napalmClone.GetComponent<PlayerNapalmScript>() as PlayerNapalmScript;
				
				playerNapalmScript.startDelay = napalmStartDelay;
	
				playerNapalmScript.lifeTime = napalmLifeTime;
			}	
			//else Debug.DrawRay (Vector2(napalmPosX, napalmPreviousHeight + 0.32), -Vector2.up, Color.red);	
		}
		
		yield return null;
		
	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Enemy"))
		{	
			// Disable the collider so we can't hit another enemy
			if (type != 4) myTr.collider2D.enabled = false; // laser can shoot through enemies !
			
			//Debug.Log("Bullet collided with an enemy", gameObject);
			other.SendMessageUpwards ("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
			
			// Call the DestroyObject function
			if (type != 4) StartCoroutine(DestroyObject());
		}
		
		if (other.CompareTag("Ground"))
		{	
			
			//Debug.Log("Player Bullet collided with the ground", gameObject);
			
			// If type = laser
			if (type == 4) return; // laser can shoot through walls !
			
			// If type = bomb, deploy napalm !
			if (type == 5)
			{
				impactClone = impactPool.Spawn();
				impactClone.transform.position = myTr.position;
				
				ImpactScript impactCloneScript = impactClone.GetComponent<ImpactScript>() as ImpactScript;//var impactCloneScript : ImpactScript = impactClone.GetComponent<ImpactScript>() as ImpactScript;
				impactCloneScript.bombImpact = true;
				
				StartCoroutine(BombLandingDamage());
				
				StartCoroutine(DestroyObject());
				
				return;
			
			}
			
			impactClone = impactPool.Spawn();
			impactClone.transform.position = myTr.position;
			
			// Call the DestroyObject function
			StartCoroutine(DestroyObject());
		}
		
	}
}






