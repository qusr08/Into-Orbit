using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : GravityObject {
	public void GiveRandomForce (Rigidbody2D parentRigidBody = null) {
		if (parentRigidBody == null) {
			rigidBody.AddForce(Utils.RandNormVect2( ) * 0.25f, ForceMode2D.Impulse);
		} else {
			rigidBody.AddForce(Utils.RandVect2OnArc(-parentRigidBody.velocity, 100).normalized * 0.5f, ForceMode2D.Impulse);
		}
	}
}
