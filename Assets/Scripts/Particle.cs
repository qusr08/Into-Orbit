using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : GravityObject {
	public bool IsInitialized;

	private void Start ( ) {
		// The particle class must be itialized beforehand to make sure the parameters are correct
		if (!IsInitialized) {
			Debug.LogError("Cannot create particle without initialization.");

			Destroy(gameObject);
			return;
		}
	}

	public void Initialize (Transform parent, Color color, float size, MeshType meshType, bool disableColliders) {
		// Set all values of the particle based on the arguments given
		MeshType = meshType;
		Size = size;
		SetColor(color);
		GetComponent<PolygonCollider2D>( ).enabled = !disableColliders;
		/*
		if (setTransformParent) {
			particle.transform.SetParent(parent);
		}
		*/

		// Since the values of this particle were set, the particle object is now intialized and can appear in the scene
		IsInitialized = true;
	}

	public void GiveRandomForce (Rigidbody2D parentRigidBody = null) {
		// if (parentRigidBody == null) {
		rigidBody.AddForce(Utils.RandNormVect2( ) * 0.25f, ForceMode2D.Impulse);
		// } else {
		// 	rigidBody.AddForce(Utils.RandVect2OnArc(-parentRigidBody.velocity, 100).normalized * 0.5f, ForceMode2D.Impulse);
		// }
	}
}
