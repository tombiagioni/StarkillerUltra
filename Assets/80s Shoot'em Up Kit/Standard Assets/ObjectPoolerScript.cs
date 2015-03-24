
// If you are not familiar with object pooling concept, you can find a great tutorial video here : http://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/object-pooling

using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPoolerScript: MonoBehaviour
{
	
	public string id;
	//public static var current : ObjectPoolerScript;
	public GameObject pooledObject;
	public int amount = 3;
	public bool canGrow = true;
	
	public List<GameObject> pooledObjects = new List<GameObject>();

	public void Awake()
	{
	
		//current = this; 
		
	}
	
	public void Start()
	{
		
		for(int i = 0; i < amount; i++)
		{
			GameObject obj = (GameObject)Instantiate(pooledObject);
			obj.SetActive(false);
			obj.transform.parent = transform;
			pooledObjects.Add(obj);
		}
		
	}

	public GameObject Spawn()
	{
		
		// Else (this is the targeted script holding our object's list)
		
		// Log reminders
		// if "pooledObject" was not set in editor
		if (pooledObject == null)
		{
			Debug.LogWarning("(ObjectPoolerScript " + id + ") pooledObject value is null !", gameObject);
			return null;
		}
		// if "amount" was not set in editor or set to incorrect value
		if (amount<=0)
		{
			Debug.LogWarning("(ObjectPoolerScript " + id + ") amount value is null or negative !", gameObject);
			return null;
		}
		
		// Return the first inactive object from the list
		for(int i = 0; i < pooledObjects.Count; i++)
		{	
			if (pooledObjects[i].activeInHierarchy == false)
			{
				pooledObjects[i].SetActive(true);
				return pooledObjects[i];
			}
		}
		
		// Else, none are found, so if the list allowed to grow then instantiate a new object, and add it to the list
		if (canGrow == true)
		{
			GameObject obj = (GameObject)Instantiate(pooledObject);
			pooledObjects.Add(obj);
			obj.transform.parent = transform;
			obj.SetActive(true);
			return obj;
		}
		
		// Else, no objects are available, so return null
		return null;
	}
	
}