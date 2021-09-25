using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MonoBehaviour {
	[Header("--- Wormhole Class ---")]
	[SerializeField] private List<MeshObject> rings = new List<MeshObject>( );
	[SerializeField] private MeshObject outsideRing;
	[SerializeField] private GravityObject ship;
	[Space]
	[SerializeField] private float oscSpeed;
	[SerializeField] private float scaleRange;

	public float Size {
		get {
			return outsideRing.Size;
		}
	}

	private float oscillationAngle;

	private void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));
	}

	private void Update ( ) {
		if (ship != null && ship.Wormhole == null) {
			if (Vector2.Distance(ship.Position, transform.position) <= outsideRing.Size / 2) {
				ship.Wormhole = this;
			}
		}
	}

	private void FixedUpdate ( ) {
		oscillationAngle += oscSpeed;
		/*
		if (oscillationAngle >= Mathf.PI * 2) {
			oscillationAngle -= Mathf.PI * 2;
		}
		*/

		// Animate the rings of the wormhole to make them pulse
		// Also, make sure they don't overlap
		foreach (MeshObject ring in rings) {
			float scaleValue = scaleRange * Mathf.Sin((1 / ring.Size) * oscillationAngle) + 1;

			ring.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
		}
	}
}
