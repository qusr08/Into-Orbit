using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MonoBehaviour {
	[Header("--- Wormhole Class ---")]
	[SerializeField] private List<MeshObject> rings = new List<MeshObject>( );
	[Space]
	[SerializeField] private float oscSpeed = 0.01f;
	[SerializeField] private float scaleRange = 0.2f;

	private float oscillationAngle;

	private void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));
	}

	private void FixedUpdate ( ) {
		oscillationAngle += oscSpeed;
		/*
		if (oscillationAngle >= Mathf.PI * 2) {
			oscillationAngle -= Mathf.PI * 2;
		}
		*/

		foreach (MeshObject ring in rings) {
			float scaleValue = scaleRange * Mathf.Sin((1 / ring.Size) * oscillationAngle) + 1;

			ring.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
		}
	}
}
