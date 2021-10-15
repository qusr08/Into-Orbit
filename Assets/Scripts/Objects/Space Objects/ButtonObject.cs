using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonObject : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float rotationValue = -(Mathf.PI / 4) * Mathf.Sin(rotationAngle - ((i / 2f) * rotationAngleMod)) + Mathf.PI;

			rings[i].Rotation = Mathf.Rad2Deg * rotationValue;
		}
	}

	public override void OnObjectCollision (GameObject collisionObject) {
		Shrink(0.5f);

		ChangeColorOfRings(toInsideRingColor: new Color(152 / 255f, 198 / 255f, 0 / 255f),
			toOutsideRingColor: new Color(179 / 255f, 234 / 255f, 0 / 255f));
	}
}
