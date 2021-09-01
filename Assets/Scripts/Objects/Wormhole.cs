using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MeshObject {
	[Header("--- Wormhole Class ---")]
	[SerializeField] private List<MeshObject> rings = new List<MeshObject>( );
	[Header("--- Wormhole Constants ---")]
	[SerializeField] private float OSCILLATION_SPEED = 0.01f;
	[SerializeField] private float SCALE_LIMIT = 0.2f;

	private float oscillationAngle;

	private new void OnValidate ( ) {
		base.OnValidate( );

		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));
		rings.Remove(this);
	}

	private void FixedUpdate ( ) {
		oscillationAngle += OSCILLATION_SPEED;
		/*
		if (oscillationAngle >= Mathf.PI * 2) {
			oscillationAngle -= Mathf.PI * 2;
		}
		*/

		foreach (MeshObject ring in rings) {
			float scaleValue = SCALE_LIMIT * Mathf.Sin((1 / ring.Size) * oscillationAngle) + 1;

			ring.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
		}
	}
}
