﻿using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	public float moveSpeed = 60;

	Rigidbody Rigidbody;
	Camera viewCamera;
	Vector3 velocity;

	void Start () {
		Rigidbody = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
	}

	void Update () {
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
		transform.LookAt (mousePos + Vector3.up * transform.position.y);
		velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;
	}

	void FixedUpdate() {
		Rigidbody.MovePosition (Rigidbody.position + velocity * Time.fixedDeltaTime);
	}
}