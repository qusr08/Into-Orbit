using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GravityObject {
	private float rotationSpeed;

	protected new void Awake ( ) {
		base.Awake( );

		rotationSpeed = Utils.RandFloat(-10, 10);
	}

	protected new void FixedUpdate ( ) {
		base.FixedUpdate( );

		transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
	}
}
