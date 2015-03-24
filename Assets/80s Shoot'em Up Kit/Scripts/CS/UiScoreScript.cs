
// UI SCORE SCRIPT CS 20150119_1114 //

using UnityEngine;
using System;
using System.Collections;


public class UiScoreScript:MonoBehaviour
{
	
	// Object values :
	public Color color1 = Color.cyan;
	public Color color2 = Color.yellow;
	Vector3 scale1 = new Vector3 (1.0f, 1.0f, 1.0f);
	Vector3 scale2 = new Vector3 (1.25f, 1.25f, 1.0f);
	
	public int scoreGet; // Real score sent by "PlayerScript"
	
	int scoreDisplay = 0; // Displayed text value - 8 digits (max 99 999 999)
	
	// 3D Text (TextMesh) : 
	public TextMesh textMesh;
	
	public void Start()
	{
		
		textMesh.color = color1;
		textMesh.text = "00000000";
		
	}
	
	// start coroutine ProcessScoreEntry called by "PlayerScript" - increments the score
	public void ProcessScoreEntry(int playerScore)
	{
	
		scoreGet = playerScore;
		textMesh.color = color2;
		transform.localScale = scale2;
	
		StartCoroutine(UpdateScoreEntry());
		
	}
	
	// Called by "ProcessScoreEntry()" (itself called by "PlayerScript()")
	// Displays score increment
	public IEnumerator UpdateScoreEntry()
	{
	
		while (scoreGet > scoreDisplay) // While REAL score is greater than DISPLAYED score
		{ 
			//audio.Play();
			scoreDisplay = scoreDisplay+5;
	
			textMesh.text = ""+scoreDisplay;
			
				while (textMesh.text.Length < 8)
				{
					textMesh.text = "0"+textMesh.text;
				}
			
			yield return new WaitForSeconds(0.01f);
	
		}
	
		transform.localScale = scale1;
		textMesh.color = color1;
		
	}
	
	// Called by "MainScript" when player respawn
	// Displays directly score increment without visual effect
	public void UpdateScoreEntryDirect()
	{
	
		//audio.Play();
		scoreDisplay = scoreGet;
	
		textMesh.text = ""+scoreDisplay;
		
			while (textMesh.text.Length < 8)
			{
				textMesh.text = "0"+textMesh.text;
			}
	
		transform.localScale = scale1;
		textMesh.color = color1;
		
	}

}