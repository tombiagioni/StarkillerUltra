// EVENT SCRIPT CS 20150119_1051 //

using UnityEngine;
using System;
using System.Collections;


public class EventScript:MonoBehaviour
{
	
	// 'Set checkpoint'
	public bool setCheckpoint;
	public float checkpointPosition;
	
	// 'Set music'
	public bool setMusic;
	public AudioClip music;
	public bool musicLoop;
	public bool musicWaitForClipEnd;
	public float musicDelay;
	
	// 'Stop Music'
	public bool stopMusic;
	public bool musicFadeOut;
	
	// 'Set text'
	public bool setText;
	public string text;
	public float textDuration = 10.0f;
	public Color textColor = Color.white;
	public float textSize = 0.05f;
	public Vector2 textPosition = new Vector2 (0.0f, 0.0f);
	
	// 'Set level'
	public bool setLevel;
	public bool loadLevelAdditive;
	public int levelValue;
	
	// 'Set upgrade'
	public bool setUpgrade;
	public ObjectPoolerScript upgradePool;
	public Vector2 upgradePosition;
	
	// UI Text references :
	GameObject uiTextGO;
	TextMesh uiTextMesh;
	
	// Camera :
	Camera cam;
	MainScript mainScript; // "Main Script" component attached to camera
	
	public IEnumerator Start()
	{
	
		cam = Camera.main;
		mainScript =  cam.GetComponent<MainScript>();
		
		yield return null;
		
		uiTextGO = GameObject.Find("UI_Text");
		uiTextMesh = uiTextGO.GetComponent<TextMesh>();
		
	}
	
	public IEnumerator EventProcess()
	{
	
		transform.collider2D.enabled = false;
		
		if (setCheckpoint == true)	mainScript.checkpointPos = /*transform.position.x*/checkpointPosition;
		
		if (setMusic == true)		StartCoroutine(mainScript.MusicPlay(music, musicLoop, musicWaitForClipEnd, musicDelay));
	
		else // if (setMusic == false)
		
		if (stopMusic == true)		StartCoroutine(mainScript.MusicStop(musicFadeOut));
	
		
		if (setText == true)
		{
			uiTextMesh.text = text;
			uiTextGO.transform.localPosition = new Vector3(textPosition.x, textPosition.y, uiTextGO.transform.localPosition.z);
			uiTextMesh.characterSize = textSize;
			uiTextMesh.color = textColor;
			
			// (Text duration : if set to 0, text duration is null and text will stay displayed)
			
			if (textDuration>0)
			{
				yield return new WaitForSeconds (textDuration);	
				uiTextMesh.text = "";
			}
		}
		
		if (setLevel == true)
		{
			if (setCheckpoint == true) PlayerPrefs.SetFloat("Spawn position", checkpointPosition);
			
			mainScript.level = levelValue; // Change level value in "MainScript"
	
			StartCoroutine(mainScript.SetLevel());
		}
		
		if  (setUpgrade == true)
		{
			GameObject upgradeClone = upgradePool.Spawn();
			upgradeClone.transform.position = new Vector3(transform.position.x + upgradePosition.x, transform.position.y + upgradePosition.y, upgradeClone.transform.localPosition.z);
		}
		
	}
	
	// Process the event when triggering Camera collider
	public void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.CompareTag("MainCamera"))
		{
			//Debug.Log("Event collided with Player", gameObject);
			if (transform.position.x > other.transform.position.x + 1.49f) // the Event object must be at the limit of camera X position
			StartCoroutine(EventProcess()); // Process the event
		}
	
	}
}







