
// POINTER INPUT TARGET SCRIPT CS 20150119_1114 //

using UnityEngine;
using System;


public class PointerInputTargetScript:MonoBehaviour
{
	
	// Cached object component
	Transform myTr; // Target object's transform
	
	// Is the pointer moving to raycast hit
	//[HideInInspector var isActive:boolean=false];
	[HideInInspector]
	public bool isActive = false; 
	
	bool moved = false;
	
	// Game's camera
	public Camera cam;
	Transform camTr;
	
	// Player cached components :
	GameObject player;
	//Transform playerTr;
	PlayerScript playerScript; // Target player's transform
	
	
	// Limits pointer's position relative to camera position
	public Vector2 screenLimitsMin;
	public Vector2 screenLimitsMax;
	
	// The raycast hit of the input raycast
	RaycastHit2D hit;
	Vector3 hitRelativePos = new Vector3(0.5f,0.5f,0.0f);
	Vector3 hitRelativePosPrevious = new Vector3(0.5f,0.5f,0.0f);
	
	// LayerMask is used in the rayCast, to raycast for layer 11, "InputRaycast Layer". raycast is used to determine player movement.
	public LayerMask layerMask = (LayerMask)(1 << 11);
	
	public void Start()
	{
	
		// Store references
		myTr = transform;
		camTr = cam.transform;
		player = GameObject.FindWithTag ("Player");
		//playerTr = player.transform;
		playerScript =  player.GetComponent<PlayerScript>();
		
	}
	
	public void Update()
	{
	
		hit = Physics2D.Raycast((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 10.0f, layerMask);
			
		if (hit.collider != null)
		{
			
			// Record the last pointer position
			hitRelativePosPrevious = hitRelativePos;
			
			// Compute the new position
			// Calculate the hit point position relative to the camera
			hitRelativePos = camTr.InverseTransformPoint(hit.point);
			
			// Check if the pointer has moved
			
			if ( hitRelativePos != hitRelativePosPrevious) moved = true;
			else moved = false;
			
			// Share the "moved" variable with "PlayerScript"
			playerScript.pointerInputMoved = moved;
			
			// If "isActive" is set to false (by "playerScript"), then abort the function (don't move in this "Update()")
			if (isActive == false) return;
			
			// Else move the pointer input target
	
			// Constraint position to the screen limits
			if (hitRelativePos.x<screenLimitsMin.x) hitRelativePos.x=screenLimitsMin.x;
			else if (hitRelativePos.x>screenLimitsMax.x) hitRelativePos.x=screenLimitsMax.x;
			if (hitRelativePos.y<screenLimitsMin.y) hitRelativePos.y=screenLimitsMin.y;
			else if (hitRelativePos.y>screenLimitsMax.y) hitRelativePos.y=screenLimitsMax.y;
			
			// Set local position to constrained hit position
	    	myTr.localPosition = hitRelativePos;
		}
		
	}
}





