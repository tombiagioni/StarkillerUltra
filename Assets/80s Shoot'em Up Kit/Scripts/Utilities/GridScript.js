// GRID SCRIPT CS 20141024_0406

// Draws a grid in editor's scene window to help tile placement.
// To use in conjonction with Snap Settings (/Edit/Snap Settings...), with snap values 'Move X' and 'Move Y' set to 0.32 (32 pixels at 100 px/Unit).
// (hold the Control key (Command on Mac) to snap to increments defined in the Snap Settings)
	
#if UNITY_EDITOR

	var width : float = 0.32;
    var height : float = 0.32;
	var gridColor : Color = Color.blue;
	var customPos : Vector3 = Vector3(0.08, 0.16, 0);

	function OnDrawGizmos()
    {
		
		if (width < 0.01) width = 0.01;
		if (height < 0.01) height = 0.01;
		
		
		var pos : Vector3 = Camera.current.transform.position;	
		
		Gizmos.color = gridColor;

		for (var y : float = pos.y - 800.0; y < pos.y + 800.0; y+= height)
   		{
        	Gizmos.DrawLine(new Vector3(-1000000.0f, Mathf.Floor(y / height) * height, 0.0) + Vector3(0, customPos.y, 0) ,
            new Vector3(1000000.0f , Mathf.Floor(y / height) * height, 0.0) + Vector3(0, customPos.y, 0) );
    	}
    
    	for (var x : float = pos.x - 1200.0; x < pos.x + 1200.0; x+= width)
    	{
       		Gizmos.DrawLine(new Vector3(Mathf.Floor(x / width) * width, -1000000.0f , 0.0) + Vector3(customPos.x, 0, 0),
            new Vector3(Mathf.Floor(x / width) * width, 1000000.0f, 0.0) + Vector3(customPos.x, 0, 0));
   		}
	}

#endif 
	

