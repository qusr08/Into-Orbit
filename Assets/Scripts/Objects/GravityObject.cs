using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : SpaceObject {
	[Header("--- Gravity Object Class ---")]
	public Wormhole Wormhole;

	protected void FixedUpdate ( ) {
		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
			if (Wormhole != null) {
				// move position towards wormhole
			} else {
				// Calculate the gravity that the ship will experience at the current position
				rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
			}
		}
	}
}
