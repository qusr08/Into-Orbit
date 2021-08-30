using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : GravityObject {
	protected void Start ( ) {
		rigidBody.AddForce(Utils.RandNormVect2( ) * 0.5f, ForceMode2D.Impulse);
	}
}
