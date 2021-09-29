using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MonoBehaviour {
	[Header("--- Wormhole Class ---")]
	[SerializeField] private List<MeshObject> rings = new List<MeshObject>( );
	[SerializeField] private MeshObject outsideRing;
	[SerializeField] private Ship ship;
	[Space]
	[SerializeField] private float oscSpeed;
	[SerializeField] private float scaleChange;

	public float Radius {
		get {
			return outsideRing.Size / 2;
		}
	}

	public Vector2 Position {
		get {
			return transform.position;
		}
	}

	private float oscillationAngle;

	private void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));
	}

	private void Update ( ) {
		if (ship != null && ship.Wormhole == null) {
			if (Vector2.Distance(ship.Position, Position) <= Radius) {
				ship.Wormhole = this;
			}
		}
	}

	private void FixedUpdate ( ) {
		oscillationAngle += oscSpeed;
		if (oscillationAngle >= Mathf.PI * 2) {
			oscillationAngle -= Mathf.PI * 2;
		}

		// Animate the rings of the wormhole to make them pulse
		// Also, make sure they don't overlap
		for (int i = 0; i < rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(oscillationAngle + i) - scaleChange + 1;
			rings[i].transform.localScale = new Vector3(scaleValue, scaleValue, 1);
		}
	}
}
