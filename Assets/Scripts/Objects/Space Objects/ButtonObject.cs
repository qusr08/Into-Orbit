using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonObject : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float rotationValue = -(Mathf.PI / 4) * Mathf.Sin(rotationAngle - (i / 2f)) + Mathf.PI;

			rings[i].Rotation = Mathf.Rad2Deg * rotationValue;
		}
	}

	public override void OnObjectCollision ( ) {

	}
}
