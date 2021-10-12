using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierObject : MeshObject {
	[Separator("Barrier Object")]
	[SerializeField] private Planet planet1;
	[SerializeField] private Planet planet2;

	protected new void OnValidate ( ) {
		base.OnValidate( );

		if (planet1 != null && planet2 != null) {
			Position = (planet1.Position + planet2.Position) / 2;
			transform.rotation = Quaternion.Euler(0, 0, Utils.GetAngleBetween(planet1.Position, planet2.Position));
			Scale = new Vector2(Vector2.Distance(planet1.Position, planet2.Position) / Size, 1);
		}
	}
}
