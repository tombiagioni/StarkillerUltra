// ENEMY1 GROUP SCRIPT CS 20150119_1021 //

using UnityEngine;
using System;
using System.Collections;


public class Enemy1GroupScript:MonoBehaviour
{
	
	// Object values - These vars are shared with childs :
	public int hp = 1;
	public int scoreValue = 100;
	[HideInInspector]
	public int damageValue = 1;
	
	public float speed = 0.7f;
	public float targetXDist = -2.0f;
	public float targetYDist = -0.5f;
	bool goUp = false;
	
	// Checks number of remaining children (for example, for bonus rewarding)
	int childrenNumber = 0;
	
	// Become true when the first child become visible and call "ChildBecameVisible()" in this script
	bool visibilityActive = false;
	
	// Children :
	Enemy1ChildScript enemy1ChildScript; // Reference to child scripts
	
	// Coins :
	public bool giveCoins = true;
	
	ObjectPoolerScript explosionPool;
	ObjectPoolerScript coinPool;
	ObjectPoolerScript upgradePool;
	
	
	public void Start()
	{
				
		explosionPool = GameObject.Find("ObjectPool EnemyExplosions").GetComponent<ObjectPoolerScript>(); 
		coinPool = GameObject.Find("ObjectPool ItemCoins").GetComponent<ObjectPoolerScript>(); 
		upgradePool = GameObject.Find("ObjectPool ItemUpgrades").GetComponent<ObjectPoolerScript>(); 
	
		
		// Determine Y direction of the pattern by checking "targetYDist"
		if (targetYDist < 0) goUp = false;
		else goUp = true;
	
	}
	
	// function called via children object message
	// This Script is processed once, when the first child became visible
	public IEnumerator ChildBecameVisible()
	{
		
		// If the group is already active, abort the function
		if (visibilityActive == true) yield break;//return null; // FIX CS 13 01 2015 VERIFIER !!
		
		visibilityActive = true;
		
		// Share parent values with children
		for(int i = 0; i < transform.childCount; i++)
		{
			enemy1ChildScript = transform.GetChild(i).GetComponent<Enemy1ChildScript>() as Enemy1ChildScript;
			enemy1ChildScript.hp = hp;
			enemy1ChildScript.speed = speed;
			enemy1ChildScript.damageValue = damageValue;
			enemy1ChildScript.goUp = goUp;
			enemy1ChildScript.asleep = false;
			enemy1ChildScript.targetXPos = targetXDist;
			enemy1ChildScript.targetYPos = targetYDist;
			enemy1ChildScript.explosionPool = explosionPool;
			enemy1ChildScript.scoreValue = scoreValue;
			enemy1ChildScript.coinPool = coinPool;
			childrenNumber = i + 1;
		}
		
		yield return null;
		
	}
	
	// This function is called each time a child is destroyed, to check for combo
	public void SubstractChild(GameObject go)
	{
	
		childrenNumber = childrenNumber - 1;
	
		if (childrenNumber == 0)
		{
			Debug.Log ("Well Done ! you made a combo", gameObject);
			
			GameObject upgradeClone = upgradePool.Spawn();
			upgradeClone.transform.position = go.transform.position;
		}
	
	}
}







