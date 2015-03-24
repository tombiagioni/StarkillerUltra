
// IMPACT SCRIPT CS 20150119_1051 //

using UnityEngine;
using System;
using System.Collections;


public class ImpactScript:MonoBehaviour
{
	
	// Object values :
	[HideInInspector]
	public bool bombImpact = false; // Is the impact created by a bomb ? (else use regular animation)
	
	public void OnBecameVisible()
	{
	
		if (bombImpact == true) transform.GetComponent<Animator>().SetBool("Bomb_Bool", true);
	
	}
	
	public IEnumerator DestroyObject()
	{
	
		// REGULAR METHOD : // Kills the game object
		// Destroy (gameObject);
			
		// pooling method : We set the object to inactive so it can be retreived from object pool list (in "ObjectPoolerScript")
		
		yield return null; // yield function is needed because an animation is playing
		
		if (gameObject.activeInHierarchy == true)
		{
			bombImpact = false;
			transform.GetComponent<Animator>().SetBool("Bomb_Bool", false);
			gameObject.SetActive(false);
		}
		
	}
}