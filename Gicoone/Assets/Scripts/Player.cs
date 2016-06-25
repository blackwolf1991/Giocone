﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : Mobile
{
    public int maxLives;
	public int stealthCycles;
	
	[HideInInspector]
	public bool canMove;
	[HideInInspector]
	public bool inStealth;
	
	private bool axisPressed; // Il giocatore sta tenendo premuti i tasti di movimento?
    private int lives;
	private Direction stealthExit; // In quale direzione il personaggio viene risputato fuori quando deve lasciare lo stealth?
	
    private GameController gameController;
	private Slider livesUI;
	private Slider stealthUI;
	
    new void Start()
    {
        base.Start();
		
        canMove = false;

		axisPressed = false;
        lives = maxLives;
		
		gameController = GameObject.FindGameObjectWithTag( "GameController" ).GetComponent<GameController>();
		livesUI = GameObject.Find( "Canvas/LivesPanel" ).GetComponent<Slider>();
		stealthUI = GameObject.Find( "Canvas/StealthBar" ).GetComponent<Slider>();
		
		UpdateLivesUI();
		ExitStealth();
    }

	void Update()
	{
		int hor = (int) Input.GetAxisRaw( "Horizontal" );
		int ver = (int) Input.GetAxisRaw( "Vertical" );
			
		if ( hor != 0 || ver != 0 )
		{
			if ( !axisPressed )
			{
				if ( canMove && !moving )
				{
					if ( hor == 0 || ver == 0 ) // Se il giocatore prova a muoversi contemporaneamente su due assi, l'input viene ignorato.
					{
						Vector3 vec = new Vector3( hor, 0.0f, ver );
						Rotate( vec );
						bool hasMoved = AttemptMove( vec );
						
						if ( hasMoved )
							ExitStealth(); // Il giocatore non può essere in stealth quando effettivamente inizia a muoversi.
						
						canMove = false; // Il giocatore non può muoversi due volte nello stesso beat.
						
						gameController.CatchBeatTack();
					}
				}
				else
				{
					LoseLife();
				}
			}
				
			axisPressed = true;
		}
		else
			axisPressed = false;
		
		if ( Input.GetButtonDown( "Stealth" ) )
			GetFacing();
	}
	
	public void AfterMoveChecks()
	{
		Collider[] overlap = Physics.OverlapBox( transform.position, new Vector3( 0.4f, 0.4f, 0.4f ) );
		
		foreach ( Collider trigger in overlap )
		{
			if ( trigger.CompareTag( "Pickup" ) )
			{
				Destroy( trigger.gameObject );
				GainLife();
			}
			else if ( trigger.CompareTag( "Stealther" ) )
			{
				EnterStealth();
				stealthExit = GetFacing().Invert();
			}
			else if ( trigger.CompareTag( "Finish" ) )
			{
				Destroy( trigger.gameObject );
				// Gestire il game over.
				Debug.Log( "Il player ha preso il vinile!" );
			}
		}
	}

	public void GainLife()
	{
		if ( lives < maxLives )
		{
			lives++;
		}
		
		UpdateLivesUI();
	}

    public void LoseLife()
    {
        lives--;
		
		if ( lives == 0 )
		{
			// Gestire il game over.
			Debug.Log( "Il player è morto!" );
		}
		
        UpdateLivesUI();
    }
	
	private void UpdateLivesUI()
	{
		livesUI.value = lives;
	}
	
	private void EnterStealth()
	{
		inStealth = true;
		stealthUI.gameObject.SetActive( true );
		stealthUI.value = 1.0f;
	}
	
	private void ExitStealth()
	{
		inStealth = false;
		stealthUI.gameObject.SetActive( false );
		StopCoroutine( "StealthSegment" );
	}
	
	public void StartStealthSegment()
	{
		StartCoroutine( "StealthSegment" );
	}
	
	private IEnumerator StealthSegment()
	{
		float lapse = gameController.beat * stealthCycles;
		float endTime = Time.time + lapse;
		
		while ( Time.time <= endTime )
		{
			stealthUI.value = ( endTime - Time.time ) / lapse;
			yield return null;
		}
		
		AttemptMove( stealthExit );
		canMove = false;
		ExitStealth();
	}
}