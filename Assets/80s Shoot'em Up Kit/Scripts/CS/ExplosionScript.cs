
// EXPLOSION SCRIPT CS 20150119_1051 //

using UnityEngine;
using System;
using System.Collections;


public class ExplosionScript:MonoBehaviour
{
	
	public IEnumerator DestroyObject()
	{
	
		// REGULAR METHOD : // Kills the game object
		// Destroy (gameObject);
			
		// pooling method : We set the object to inactive so it can be retreived from object pool list (in "ObjectPoolerScript")
		
		yield return null; // yield function is needed because an animation is playing
		
		if (gameObject.activeInHierarchy == true)
		{
			gameObject.SetActive(false);
		}
		
	}
}