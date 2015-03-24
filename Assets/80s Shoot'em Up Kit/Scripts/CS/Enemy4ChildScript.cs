// ENEMY4 CHILD SCRIPT CS 20150119_1048 //

using UnityEngine;
using System;
using System.Collections;


public class Enemy4ChildScript:MonoBehaviour
{
	
	public Enemy4Script enemy4Script;
	
	public void Start()
	{
	
		StartCoroutine(DisableCollider());
	
	}
	
	// Disable collider some times after scene has initialised
	// Collider is disabled to be sure that nothing will reach an out of screen object
	// Disabling is delayed to let "MainScript"'s "Spawn()" function, wich use enemies colliders, operate first.
	public IEnumerator DisableCollider()
	{
		
		yield return new WaitForSeconds(0.3f);
		
		transform.collider2D.enabled = false;
		
	}
	
	public void OnBecameVisible()
	{
		
		// On became visible, tell the parent ("Enemy4Script()") 
		enemy4Script.SharedOnBecameVisible();
		transform.collider2D.enabled = true;
		
	}
	
	public void OnBecameInvisible()
	{
	
		// On became invisible, tell the parent ("Enemy4Script()") 
		enemy4Script.SharedOnBecameInvisible();
	
	}
	
	public void ApplyDamage(int damage)
	{
	
		// tell the parent
		StartCoroutine(enemy4Script.SharedApplyDamage(damage));
		
	}

}




