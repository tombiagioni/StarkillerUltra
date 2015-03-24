// JOYSTICK CUSTOM CS 20150119_1100 //

// This is a modified version of the 'Joystick.js' from the Penelope iPhone Tutorial
// This version handle single press input, and checks if button was just pressed down, or over several frames. Use 'IsFinger()' and 'IsFingerDown()' as you would use 'Getbutton()'/'GetbuttonDown()'.
// Added values are 'isSinglePressionButton' and 'isAlreadyPressed'
// Simply enable 'isSinglePressionButton' if/when needed.

//////////////////////////////////////////////////////////////
// Original file :
//////////////////////////////////////////////////////////////
// Joystick.js
// Penelope iPhone Tutorial
//
// Joystick creates a movable joystick (via GUITexture) that 
// handles touch input, taps, and phases. Dead zones can control
// where the joystick input gets picked up and can be normalized.
//
// Optionally, you can enable the touchPad property from the editor
// to treat this Joystick as a TouchPad. A TouchPad allows the finger
// to touch down at any point and it tracks the movement relatively 
// without moving the graphic
//////////////////////////////////////////////////////////////

using UnityEngine;
using System;

// A simple class for bounding how far the GUITexture will move
[System.Serializable]
public class Boundary 
{
	public Vector2 min = Vector2.zero;
	public Vector2 max = Vector2.zero;
}

[RequireComponent( typeof( GUITexture ) )]

public class JoystickCustom: MonoBehaviour
{
	
	static JoystickCustom[] joysticks;			// A static collection of all joysticks
	static bool enumeratedJoysticks = false;
	static float tapTimeDelta = 0.3f;				// Time allowed between taps
	
	public bool touchPad; 									// Is this a TouchPad?
	public Rect touchZone;
	public Vector2 deadZone = Vector2.zero;						// Control when position is output
	public bool normalize = false; 							// Normalize output after the dead-zone?
	public Vector2 position; 									// [-1, 1] in x,y
	public int tapCount;											// Current tap count
	
	int lastFingerId = -1;								// Finger last used for this joystick
	float tapTimeWindow;							// How much time there is left for a tap to occur
	Vector2 fingerDownPos;
	//float fingerDownTime;
	//float firstDeltaTime = 0.5f;
	
	GUITexture gui;								// Joystick graphic
	Rect defaultRect;								// Default position / extents of the joystick graphic
	Boundary guiBoundary = new Boundary();			// Boundary for joystick graphic
	Vector2 guiTouchOffset;						// Offset to apply to touch input
	Vector2 guiCenter;							// Center of joystick
	
	// Custom implementation :
	public bool isSinglePressionButton = false;				// is this a single pression button ? (won't work if true and held down)
	bool isAlreadyPressed = false;				// is button already pressed ?
	//
	
	public void Start()
	{
		// Cache this component at startup instead of looking up every frame	
		gui = GetComponent<GUITexture>();
		
		// Store the default rect for the gui, so we can snap back to it
		defaultRect = gui.pixelInset;	
	    
	    defaultRect.x += transform.position.x * Screen.width;// + gui.pixelInset.x; // -  Screen.width * 0.5;
	    defaultRect.y += transform.position.y * Screen.height;// - Screen.height * 0.5;
	    
	    var tmpPos = transform.position;
		tmpPos.x = 0.0f;
		tmpPos.y = 0.0f;
		transform.position = tmpPos;
	        
		if ( touchPad )
		{
			// If a texture has been assigned, then use the rect ferom the gui as our touchZone
			if ( gui.texture != null )
				touchZone = defaultRect;
		}
		else
		{				
			// This is an offset for touch input to match with the top left
			// corner of the GUI
			guiTouchOffset.x = defaultRect.width * 0.5f;
			guiTouchOffset.y = defaultRect.height * 0.5f;
			
			// Cache the center of the GUI, since it doesn't change
			guiCenter.x = defaultRect.x + guiTouchOffset.x;
			guiCenter.y = defaultRect.y + guiTouchOffset.y;
			
			// Let's build the GUI boundary, so we can clamp joystick movement
			var tmpGuiBoundary = guiBoundary.min;
			tmpGuiBoundary.x = defaultRect.x - guiTouchOffset.x;
			guiBoundary.min = tmpGuiBoundary;
			tmpGuiBoundary = guiBoundary.max;
			tmpGuiBoundary.x = defaultRect.x + guiTouchOffset.x;
			guiBoundary.max = tmpGuiBoundary;
			tmpGuiBoundary = guiBoundary.min;
			tmpGuiBoundary.y = defaultRect.y - guiTouchOffset.y;
			guiBoundary.min = tmpGuiBoundary;
			tmpGuiBoundary = guiBoundary.max;
			tmpGuiBoundary.y = defaultRect.y + guiTouchOffset.y;
			guiBoundary.max = tmpGuiBoundary;
		}
	}
	
	public void Disable()
	{
		gameObject.SetActive(false);
		enumeratedJoysticks = false;
	}
	
	public void ResetJoystick()
	{
		// Release the finger control and set the joystick back to the default position
		gui.pixelInset = defaultRect;
		lastFingerId = -1;
		
		position = Vector2.zero;
		fingerDownPos = Vector2.zero;
	
		if ( touchPad )
			
            {
            var tmpGuiColor = gui.color;
			tmpGuiColor.a = 0.1f;
			gui.color = tmpGuiColor;
            }	
	}
	
	
	public bool IsFingerUp() // Is finger is released this frame - Unused in the kit
	{
	
		//if (lastFingerId == -1) isAlreadyPressed = false;
	
		if (isAlreadyPressed == true && lastFingerId == -1)
		{
			isAlreadyPressed = false;
			return true;
		}
		
		else return false;
		
	}
	
	
	public bool IsFingerDown() // Is finger is down this frame - Called by 'PlayerScript.js'
	{
	
		if (lastFingerId == -1) isAlreadyPressed = false;
	
		if (isAlreadyPressed == false && tapCount != 0f)
		{
			isAlreadyPressed = true;
			return (lastFingerId != -1);
		}
		
		else return false;
		
	}
	
	
	public bool IsFinger() // Is finger kept down - Called by 'PlayerScript.js'
	{
	
		if (lastFingerId == -1) isAlreadyPressed = false;
		
		if (tapCount != 0f)
		{
			// Check if we want single pression button input and if it is arleady in use 
			if (isSinglePressionButton == true && isAlreadyPressed == true) return false;
			
			isAlreadyPressed = true;
			
			return (lastFingerId != -1);
		}
		
		else return false;
		
	}
	
	public void LatchedFinger( int fingerId )
	{
		// If another joystick has latched this finger, then we must release it
		if ( lastFingerId == fingerId )
			ResetJoystick();
	}
	
	public void Update()
	{	
		if ( !enumeratedJoysticks )
		{
			// Collect all joysticks in the game, so we can relay finger latching messages
			joysticks = FindObjectsOfType( typeof( JoystickCustom ) ) as JoystickCustom[];
			enumeratedJoysticks = true;
		}	
			
		int count = Input.touchCount;
		
		if (tapCount == 0) isAlreadyPressed = false;
		
		// Adjust the tap time window while it still available
		if ( tapTimeWindow > 0 )
			tapTimeWindow -= Time.deltaTime;
		
		else
			tapCount = 0;
		
		if ( count == 0 )
			ResetJoystick();
		
		else
		{
			for( int i = 0; i < count; i++)
			{
				Touch touch = Input.GetTouch(i);			
				Vector2 guiTouchPos = touch.position - guiTouchOffset;
		
				bool shouldLatchFinger = false;
				if ( touchPad )
				{				
					if ( touchZone.Contains( touch.position ) )
						shouldLatchFinger = true;
				}
				else if ( gui.HitTest( touch.position ) )
				{
					shouldLatchFinger = true;
				}		
		
				// Latch the finger if this is a new touch
				if ( shouldLatchFinger && ( lastFingerId == -1 || lastFingerId != touch.fingerId ) )
				{
					
					if ( touchPad )
					{
						var tmpGuiColor = gui.color;
						tmpGuiColor.a = 0.05f;
						gui.color = tmpGuiColor;
						
						lastFingerId = touch.fingerId;
						fingerDownPos = touch.position;
						//fingerDownTime = Time.time;
					}
					
					lastFingerId = touch.fingerId;
					
					// Accumulate taps if it is within the time window
					if ( tapTimeWindow > 0 )
						tapCount++;
					else
					{
						tapCount = 1;
						tapTimeWindow = tapTimeDelta;
					}
												
					// Tell other joysticks we've latched this finger
					foreach( JoystickCustom j in joysticks )
					{
						if ( j != this )
							j.LatchedFinger( touch.fingerId );
					}						
				}				
		
				if ( lastFingerId == touch.fingerId )
				{	
					// Override the tap count with what the iPhone SDK reports if it is greater
					// This is a workaround, since the iPhone SDK does not currently track taps
					// for multiple touches
					if ( touch.tapCount > tapCount )
						tapCount = touch.tapCount;
					
					if ( touchPad )
					{	
						// For a touchpad, let's just set the position directly based on distance from initial touchdown
						position.x = ( float )Mathf.Clamp( ( int )( ( touch.position.x - fingerDownPos.x ) / ( touchZone.width / 2 ) ), -1, 1 );
						position.y = ( float )Mathf.Clamp( ( int )( ( touch.position.y - fingerDownPos.y ) / ( touchZone.height / 2 ) ), -1, 1 );
					}
					else
					{					
						// Change the location of the joystick graphic to match where the touch is
						var tmpGuiPixelInset = gui.pixelInset;
						tmpGuiPixelInset.x = Mathf.Clamp( guiTouchPos.x, guiBoundary.min.x, guiBoundary.max.x );
						tmpGuiPixelInset.y = Mathf.Clamp( guiTouchPos.y, guiBoundary.min.y, guiBoundary.max.y );
						gui.pixelInset = tmpGuiPixelInset;		
					}
					
					if ( touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled )
						ResetJoystick();					
				}			
			}
		}
		
		if ( !touchPad )
		{
			// Get a value between -1 and 1 based on the joystick graphic location
			position.x = ( gui.pixelInset.x + guiTouchOffset.x - guiCenter.x ) / guiTouchOffset.x;
			position.y = ( gui.pixelInset.y + guiTouchOffset.y - guiCenter.y ) / guiTouchOffset.y;
		}
		
		// Adjust for dead zone	
		float absoluteX = Mathf.Abs( position.x );
		float absoluteY = Mathf.Abs( position.y );
		
		if ( absoluteX < deadZone.x )
		{
			// Report the joystick as being at the center if it is within the dead zone
			position.x = 0.0f;
		}
		else if ( normalize )
		{
			// Rescale the output after taking the dead zone into account
			position.x = Mathf.Sign( position.x ) * ( absoluteX - deadZone.x ) / ( 1 - deadZone.x );
		}
			
		if ( absoluteY < deadZone.y )
		{
			// Report the joystick as being at the center if it is within the dead zone
			position.y = 0.0f;
		}
		else if ( normalize )
		{
			// Rescale the output after taking the dead zone into account
			position.y = Mathf.Sign( position.y ) * ( absoluteY - deadZone.y ) / ( 1 - deadZone.y );
		}
	}
}