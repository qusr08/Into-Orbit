using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : GravityObject {
	[Header("--- Asteroid Class ---")]
	[SerializeField] private Planet parent;
	[SerializeField] private Vector2 initialVelocity;

	protected void Start ( ) {
		rigidBody.AddForce(initialVelocity, ForceMode2D.Impulse);
	}

	protected new void FixedUpdate ( ) {
		// Calculate the gravity that the ship will experience at the current position
		rigidBody.AddForce(levelManager.CalculateGravityForce(this, new List<MeshObject> { parent }), ForceMode2D.Force);
	}
}
