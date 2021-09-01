using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	protected void FixedUpdate ( ) {
		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
			rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
		}
	}
}
