using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	[Header("--- Gravity Object Class ---")]
	[SerializeProperty("IsLocked")] public bool isLocked = false;

	public bool IsLocked {
		get {
			return isLocked;
		}

		set {
			isLocked = value;

			// If the object is locked, it should not be able to move
			rigidBody.isKinematic = isLocked;
		}
	}

	protected void FixedUpdate ( ) {
		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
			rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
		}
	}
}
