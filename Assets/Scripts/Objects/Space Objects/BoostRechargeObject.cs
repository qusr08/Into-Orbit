using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRechargeObject : SpaceObject {
	protected override void Animate ( ) {
		for (int i = 0; i < rings.Count; i++) {
			float rotationValue = rotationAngle - ((i / 10f) * rotationAngleMod);

			rings[i].Rotation = Mathf.Rad2Deg * rotationValue;
		}
	}

	public override void OnObjectCollision (GameObject collisionObject) {
		Shrink(0.5f);

		ChangeColorOfRings(toInsideRingColor: new Color(128 / 255f, 30 / 255f, 0 / 255f),
			toMiddleRingColor: new Color(139 / 255f, 33 / 255f, 0 / 255f),
			toOutsideRingColor: new Color(151 / 255f, 36 / 255f, 0 / 255f));

		Ship ship = collisionObject.GetComponent<Ship>( );
		if (isActive && ship != null) {
			ship.ResetLaunch(true);
			levelManager.SpawnParticleSystem(ParticleSystemType.BoostRecharge, Position);

			isActive = false;
		}
	}
}
