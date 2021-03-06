using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshParticle : GravityObject {
	[Separator("Mesh Particle")]
	[SerializeField] public bool IsInitialized;

	protected void Start ( ) {
		// The meshPiece class must be itialized beforehand to make sure the parameters are correct
		if (!IsInitialized) {
			Debug.LogError("Cannot create mesh particle without initialization.");

			Destroy(gameObject);
			return;
		}
	}

	public void Initialize (Color color, float size, MeshType meshType, LayerType layerType, bool showTrail, bool disableColliders) {
		// Set all values of the meshPiece based on the arguments given
		MeshType = meshType;
		LayerType = layerType;
		Size = size;
		ShowTrail = showTrail;
		DisableSolidColliders = disableColliders;
		Color = color;

		UpdateVariables( );

		// Since the values of this meshPiece were set, the meshPiece object is now intialized and can appear in the scene
		IsInitialized = true;
	}

	public void GiveRandomForce ( ) {
		rigidBody.AddForce(Utils.RandNormVect2( ) * (0.25f * SizeToMassRatio), ForceMode2D.Impulse);
	}

	protected override void Death ( ) {
	}
}
