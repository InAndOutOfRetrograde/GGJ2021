﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerCam
	:
	MonoBehaviour
{
	void Start()
	{
		cam = transform.Find( "Main Camera" );

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Assert.IsTrue( verticalCutoff > 0.0f );

		rayMask = ~LayerMask.GetMask( "Player" );
		holdSpot = cam.transform.Find( "HoldSpot" );

		wepInfoPanel = GameObject.Find( "WepInfoPanel" );
		wepInfoTitle = wepInfoPanel.transform.Find( "Name" ).GetComponent<Text>();
		wepInfoDesc = wepInfoPanel.transform.Find( "Info" ).GetComponent<Text>();
		wepInfoPanel.SetActive( false );
	}

	void Update()
	{
		// if( Input.GetKeyDown( KeyCode.Escape ) )
		// {
		// 	Cursor.lockState = CursorLockMode.None;
		// }
		// if( Input.GetMouseButtonDown( 0 ) )
		// {
		// 	Cursor.lockState = CursorLockMode.Locked;
		// }

		var aim = new Vector2( Input.GetAxis( "Mouse X" ),
			Input.GetAxis( "Mouse Y" ) );

		if( Cursor.lockState == CursorLockMode.None ) aim.Set( 0.0f,0.0f );

		if( aim.y > maxAimMove ) aim.y = maxAimMove;
		if( aim.y < -maxAimMove ) aim.y = -maxAimMove;

		var tempAng = cam.transform.eulerAngles;
		tempAng.x = tempAng.x - aim.y * rotationSpeed * Time.deltaTime;
		if( tempAng.x > 90.0f - verticalCutoff && tempAng.x < 180.0f ) tempAng.x = 90.0f - verticalCutoff;
		if( tempAng.x < 270.0f + verticalCutoff && tempAng.x > 180.0f ) tempAng.x = 270.0f + verticalCutoff;
		tempAng.y = tempAng.y + aim.x * rotationSpeed * Time.deltaTime;
		tempAng.z = 0.0f;
		cam.transform.eulerAngles = tempAng;

		WeaponStats oldLookItem = lookItem;
		var ray = new Ray( cam.transform.position,cam.transform.forward );
		RaycastHit hit;
		if( Physics.Raycast( ray,out hit,5.0f,rayMask ) )
		{
			if( Input.GetAxis( "Interact" ) > 0.0f && heldItem == null )
			{
				var throwable = hit.transform.GetComponentInParent<Throwable>();
				if( throwable != null ) heldItem = throwable.PickUp( holdSpot );
				var keg = hit.transform.GetComponentInParent<Keg>();
				if( keg != null )
				{
					var mug = Instantiate( keg.GetMugPrefab(),holdSpot );
					heldItem = mug.GetComponent<Throwable>().PickUp( holdSpot );
				}

				var refBook = hit.transform.GetComponentInParent<RefBook>();
				if( refBook != null ) refBook.Preview();
			}

			lookItem = hit.transform.GetComponentInParent<WeaponStats>();
		}
		else lookItem = null;

		if( lookItem != oldLookItem && lookItem != null ) UpdateWepInfo( lookItem );
		if( lookItem == null )
		{
			if( wepInfoClose.Update( Time.deltaTime ) ) UpdateWepInfo( null );
		}
		else wepInfoClose.Reset();

		if( Input.GetAxis( "Fire1" ) > 0.0f && heldItem != null )
		{
			heldItem.GetComponent<Throwable>().Throw( cam.transform.forward );
			heldItem = null;
		}
	}

	void UpdateWepInfo( WeaponStats wep )
	{
		wepInfoPanel.SetActive( wep != null );

		if( wep != null )
		{
			wepInfoTitle.text = lookItem.GetTitle();
			wepInfoDesc.text = lookItem.GetDesc();
		}
	}

	Transform cam;

	[SerializeField] float rotationSpeed = 5.0f;
	[SerializeField] float verticalCutoff = 10.0f;
	const float maxAimMove = 90.0f - 1.0f;

	LayerMask rayMask;

	Transform holdSpot;
	GameObject heldItem = null;

	WeaponStats lookItem = null;

	GameObject wepInfoPanel;
	Text wepInfoTitle;
	Text wepInfoDesc;

	Timer wepInfoClose = new Timer( 0.3f );
}
