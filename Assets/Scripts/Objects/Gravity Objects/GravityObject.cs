using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	[Separator("Gravity Object")]
	[SerializeField] protected List<MeshObject> parents = new List<MeshObject>( );

	protected void Start ( ) {
		parents = new List<MeshObject>( );
	}

	protected new void FixedUpdate ( ) {
		// Update the trail after the position has changed
		base.FixedUpdate( );

		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
			// Calculate the gravity that the ship will experience at the current position
			rigidBody.AddForce(levelManager.CalculateGravityForce(this, onlyParents: parents), ForceMode2D.Force);
		}
	}
}
