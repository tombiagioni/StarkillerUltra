
// CAMERA RENDERING OPTIONS SCRIPT CS 20150119_0956 //

using UnityEngine;
using System;


public class CameraRenderingOptionsScript:MonoBehaviour
{
	
	float previousScreenHeight;
	
	public bool setCameraAspect = true; // STRETCH THE GAME VIEW TO 16/10 CAMERA RATIO
	public bool setPixelPerfect = false; // SET THE GAME VIEW FIT TO 320 x 200 PIXELS RESOLUTION
	
	public void Start()
	{
	
		if (setCameraAspect == true) camera.aspect = 1.6f; // camera aspect is 16/10
		
		previousScreenHeight = (float)Screen.height;
		if (setPixelPerfect == true) SetOrthoSize();
	
	}
	
	public void Update()
	{
	
		if (previousScreenHeight != Screen.height)
		{
			if (setCameraAspect == true) camera.aspect = 1.6f; // camera aspect is 16/10
			if (setPixelPerfect == true) SetOrthoSize();
		}
		
	}
	
	// Example to create a pixel perfect view on a 320x200 screen
	public void SetOrthoSize()
	{
	
		gameObject.camera.orthographicSize = (Screen.height/100.0f/2.0f);// 100f is the PixelPerUnit that you have set on your sprite. Default is 100.
	
	}

}