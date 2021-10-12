using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportalPortal : MonoBehaviour {
	[Separator("Teleportal Portal")]
	[SerializeField] private List<MeshObject> rings = new List<MeshObject>( );
	[Space]
	[SerializeField] private float scaleSpeed;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float scaleChange;
	[SerializeField] [Range(0, 1)] private float xScale;

	private float angleOffset;
	private float scalingAngle;
	private float rotationAngle;

	protected void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));
	}

	protected void Start ( ) {
		// Generate a random offset between the portals so they look different in game
		angleOffset = Random.Range(0, Mathf.PI * 2);
	}

	protected void FixedUpdate ( ) {
		// Update the scale and rotation values
		scalingAngle += scaleSpeed;
		if (scalingAngle >= Mathf.PI * 2) {
			scalingAngle -= Mathf.PI * 2;
		}
		rotationAngle += rotationSpeed;
		if (rotationAngle >= Mathf.PI * 2) {
			rotationAngle -= Mathf.PI * 2;
		}

		// Animate the rings of the portal to make them pulse and rotate
		for (int i = 0; i < rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(scalingAngle + i + angleOffset) - scaleChange + 1;
			rings[i].Scale = new Vector3(scaleValue * xScale, scaleValue, 1);
			float rotationValue = rotationAngle + (i / 20) + angleOffset;
			rings[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * rotationValue);
		}
	}
}
