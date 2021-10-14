using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportalPortal : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float rotationValue = rotationAngle - (i / 20f);

			rings[i].Rotation = Mathf.Rad2Deg * rotationValue;
		}
	}

	protected override void OnObjectCollision ( ) {
		throw new System.NotImplementedException( );
	}
}
