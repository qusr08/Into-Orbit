using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GravityObject {
	[Header("--- Planet Class ---")]
	[SerializeField] [Range(-10f, 10f)] private float rotationSpeed;

	protected new void FixedUpdate ( ) {
		base.FixedUpdate( );

		// Rotate the planet a random amount as time goes on
		if (!IsLocked) {
			transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
		}
	}
}
