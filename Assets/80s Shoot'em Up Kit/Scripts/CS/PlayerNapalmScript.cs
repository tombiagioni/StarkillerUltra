// PLAYER NAPALM SCRIPT CS 20151402_0718 //

using UnityEngine;
using System;
using System.Collections;


public class PlayerNapalmScript:MonoBehaviour
{
	
	// Object cached components :
	SpriteRenderer mySpriteRdr;
	Animator myAnimator;
	
	// Object values :
	[HideInInspector]
	public float startDelay; // populated by "PlayerWeaponScript"
	[HideInInspector]
	public float lifeTime; // populated by "PlayerWeaponScript"
	
	public void Start()
	{
	
		mySpriteRdr = transform.GetComponent<SpriteRenderer>();
		myAnimator = transform.GetComponent<Animator>();
	
	}
	
	public void OnBecameVisible()
	{
	
		StartCoroutine(Process());
		
	} 
	
	public IEnumerator Process()
	{
		mySpriteRdr.color = new Color (mySpriteRdr.color.r, mySpriteRdr.color.g, mySpriteRdr.color.b, 0.0f);

		yield return new WaitForSeconds(startDelay); 	// "startDelay" value is sent by "PlayerWeaponScript"
		
		myAnimator.SetBool("Spawned_Bool", true);
	
		yield return new WaitForSeconds(lifeTime); 	// "lifeTime" value is sent by "PlayerWeaponScript"
		
		StartCoroutine(DestroyObject());
		
	}
	
	public IEnumerator DestroyObject()
	{
	
		// Wait one frame
		yield return null;
		
		myAnimator.SetBool("Spawned_Bool", false);
	
		gameObject.SetActive(false);	
		
	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("Enemy"))
		{	
		
			//Debug.Log("Bullet collided with an enemy", gameObject);
			other.SendMessageUpwards ("ApplyDamage", 1, SendMessageOptions.DontRequireReceiver);
	
		}
		
	}

}