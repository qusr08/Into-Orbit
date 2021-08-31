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

	public void Initialize (Transform parent, Color color, float size, MeshType meshType, LayerType layerType, bool disableColliders) {
		// Set all values of the particle based on the arguments given
		MeshType = meshType;
		LayerType = layerType;
		Size = size;
		/*
		if (setTransformParent) {
			particle.transform.SetParent(parent);
		}
		*/
		
		// Run OnValidate just to make sure all components are found
		OnValidate( );

		// Update components
		SetColor(color);
		polyCollider.enabled = !disableColliders;

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
