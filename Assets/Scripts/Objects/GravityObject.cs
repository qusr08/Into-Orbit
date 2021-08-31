using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	[Header("--- Gravity Object Class ---")]
	[SerializeField] public bool IsLocked = false;

	private new void Awake ( ) {
		base.Awake( );

		// If the object is locked, it should not be able to move
		rigidBody.isKinematic = IsLocked;
	}

	protected void FixedUpdate ( ) {
		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
			rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
		}
	}
}
