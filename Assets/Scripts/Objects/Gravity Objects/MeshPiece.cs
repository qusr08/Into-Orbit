using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPiece : GravityObject {
	[Separator("Mesh Piece")]
	[SerializeField] public bool IsInitialized;

	protected new void Start ( ) {
		base.Start( );

		// The meshPiece class must be itialized beforehand to make sure the parameters are correct
		if (!IsInitialized) {
			Debug.LogError("Cannot create mesh piece without initialization.");

			Destroy(gameObject);
			return;
		}
	}

	public void Initialize (Transform parent, Color color, float size, MeshType meshType, LayerType layerType, bool showTrail, bool disableColliders) {
		// Set all values of the meshPiece based on the arguments given
		MeshType = meshType;
		LayerType = layerType;
		Size = size;
		ShowTrail = showTrail;
		DisableColliders = disableColliders;
		Color = color;

		UpdateVariables( );

		// Since the values of this meshPiece were set, the meshPiece object is now intialized and can appear in the scene
		IsInitialized = true;
	}

	public void GiveRandomForce (Rigidbody2D parentRigidBody = null) {
		rigidBody.AddForce(Utils.RandNormVect2( ) * (0.25f * SizeToMassRatio), ForceMode2D.Impulse);
	}
}
