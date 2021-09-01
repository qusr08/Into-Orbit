using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GravityObject {
	private float rotationSpeed;

	protected new void Awake ( ) {
		base.Awake( );

		// Generate a random rotation speed
		rotationSpeed = Utils.RandFloat(-10, 10);
	}

	protected new void FixedUpdate ( ) {
		base.FixedUpdate( );

		// Rotate the planet a random amount as time goes on
		if (!IsLocked) {
			transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
		}
	}
}
