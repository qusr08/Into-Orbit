using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRechargeObject : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float offsetAngle = rotationAngle - (i / 10f);
			float rotationValue = -Mathf.PI * Mathf.Cos(offsetAngle + Mathf.Cos(offsetAngle)) + Mathf.PI;

			rings[i].Rotation = Mathf.Rad2Deg * rotationValue;
		}
	}

	public override void OnObjectCollision ( ) {
		DoQuickSpin(15);
	}
}
