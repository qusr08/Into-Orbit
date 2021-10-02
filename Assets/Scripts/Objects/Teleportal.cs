using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportal : MonoBehaviour {
	[Header("--- Teleportal Class ---")]
	[SerializeField] private Transform portal1;
	[SerializeField] private Transform portal2;
	[SerializeField] private List<MeshObject> portal1Rings = new List<MeshObject>( );
	[SerializeField] private List<MeshObject> portal2Rings = new List<MeshObject>( );
	[Space]
	[SerializeField] private float oscSpeed;
	[SerializeField] private float scaleChange;

	private MeshObject portal1OutsideRing;
	private MeshObject portal2OutsideRing;

	public Vector2 Position {
		get {
			return transform.position;
		}
	}

	private float oscillationAngle;

	private void OnValidate ( ) {
		portal1Rings.Clear( );
		portal1Rings.AddRange(portal1.GetComponentsInChildren<MeshObject>( ));
		portal2Rings.Clear( );
		portal2Rings.AddRange(portal2.GetComponentsInChildren<MeshObject>( ));

		if (portal1OutsideRing == null) {
			portal1OutsideRing = portal1.Find("Outside").GetComponent<MeshObject>();
		}
		if (portal2OutsideRing == null) {
			portal2OutsideRing = portal2.Find("Outside").GetComponent<MeshObject>( );
		}
	}

	private void FixedUpdate ( ) {
		oscillationAngle += oscSpeed;
		if (oscillationAngle >= Mathf.PI * 2) {
			oscillationAngle -= Mathf.PI * 2;
		}

		// Animate the rings of the teleportal to make them pulse
		// Also, make sure they don't overlap
		for (int i = 0; i < portal1Rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(oscillationAngle + i) - scaleChange + 1;
			portal1Rings[i].transform.localScale = new Vector3(scaleValue, scaleValue, 1);
			portal2Rings[i].transform.localScale = new Vector3(scaleValue, scaleValue, 1);
		}
	}
}
