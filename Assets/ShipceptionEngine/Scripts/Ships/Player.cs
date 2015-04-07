using UnityEngine;
//using VirtualDropkick.DanmakuEngine.Unity;

//[RequireComponent(typeof(DanmakuOrigin))]
/// <summary>
/// A simple player-behaviour
/// </summary>
public class Player : MonoBehaviour
{
	/// <summary>
	/// Key for moving up
	/// </summary>
	public KeyCode upKey;

	/// <summary>
	/// Key for moving down
	/// </summary>
	public KeyCode downKey;
	
	/// <summary>
	/// Key for moving left
	/// </summary>
	public KeyCode leftKey;
	
	/// <summary>
	/// Key for moving right
	/// </summary>
	public KeyCode rightKey;
	
	/// <summary>
	/// Key for shooting
	/// </summary>
	public KeyCode fireKey;
	
	/// <summary>
	/// The movement speed of this player
	/// </summary>
	public float moveSpeed = 6f;

	/// <summary>
	/// The boundaries in which this player is
    /// allowed to move
	/// </summary>
	public Rect movementBounds;

	/// <summary>
	/// Reference to this player's DanmakuContext
	/// </summary>
	//private DanmakuContext context;

	/// <summary>
	/// Cached reference to the DanmakuOrigin component
	/// </summary>
	//private DanmakuOrigin origin;

	/// <summary>
	/// The direction vector of this player
	/// </summary>
	private Vector3 direction = new Vector3();

	/// <summary>
	/// Holds a cached reference to the Transform
	/// of this GameObject
	/// </summary>
	private Transform _transform;

	private void Awake()
	{
		_transform = transform;
		//origin = GetComponent<DanmakuOrigin>();
	}
	
	private void Start()
	{
		//context = origin.Context;
	}
	
	private void Update()
	{
		// handle movement
		direction.Set(0f, 0f, 0f);

		if(Input.GetKey(upKey))
		{
			direction.y = 1f;
		}
		else if(Input.GetKey(downKey))
		{
			direction.y = -1f;
		}
		
		if(Input.GetKey(leftKey))
		{
			direction.x = -1f;
		}
		else if(Input.GetKey(rightKey))
		{
			direction.x = 1f;
		}
		
		Vector2 newPosition = _transform.position +
                             (moveSpeed * direction.normalized * Time.deltaTime);

		if(newPosition.x < movementBounds.xMin)
		{
			newPosition.x = movementBounds.xMin;
		}
		else if(newPosition.x > movementBounds.xMax)
		{
			newPosition.x = movementBounds.xMax;
		}
		
		if(newPosition.y < movementBounds.yMin)
		{
			newPosition.y = movementBounds.yMin;
		}
		else if(newPosition.y > movementBounds.yMax)
		{
			newPosition.y = movementBounds.yMax;
		}
		
		_transform.position = newPosition;


		// handle shooting
		if(Input.GetKey(fireKey))
		{
        //    if(!origin.IsRunningBulletPattern)
        //    {
        //        origin.StartBulletPattern();
        //    }
			
        //    if(!origin.RootEmitter.enabled)
        //    {
        //        origin.RootEmitter.enabled = true;
        //    }
        //}
        //else if(origin.IsRunningBulletPattern && origin.RootEmitter.enabled)
        //{
        //    origin.RootEmitter.enabled = false;
        //    origin.ResetRootEmitter();
        }
	}
}
