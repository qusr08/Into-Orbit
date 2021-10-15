using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(scalingAngle + i) - scaleChange + 1;

			rings[i].Scale = new Vector3(scaleValue, scaleValue, 1);
		}
	}

	public override void OnObjectCollision (GameObject collisionObject) {
		if (collisionObject.GetComponent<Ship>() != null) {
			uiManager.HasCompleted = true;
		}
	}
}
