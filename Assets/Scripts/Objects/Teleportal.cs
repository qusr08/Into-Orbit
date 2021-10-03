using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportal : MonoBehaviour {
	[Header("--- Teleportal Class ---")]
	[SerializeField] private Transform portal1;
	[SerializeField] private Transform portal2;
	[SerializeField] private Transform connector;
	[SerializeField] private List<MeshObject> portal1Rings = new List<MeshObject>( );
	[SerializeField] private List<MeshObject> portal2Rings = new List<MeshObject>( );
	[SerializeField] private Ship ship;
	[Space]
	[SerializeField] private float scaleSpeed;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float scaleChange;
	[SerializeField] private float xScale = 0.6f;

	private Collider2D portal1OutsideRing;
	private Collider2D portal2OutsideRing;
	private Collider2D shipCollider;

	private float portal1AngleOffset;
	private float portal2AngleOffset;
	private float scalingAngle;
	private float rotationAngle;

	private bool shipJustTeleported;
	private float teleportBufferTimer;

	public Vector2 Position {
		get {
			return transform.position;
		}
	}

	protected void OnValidate ( ) => UnityEditor.EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		// This is used to suppress warnings that Unity oh so kindy throws when editting meshes in OnValidate
		UnityEditor.EditorApplication.delayCall -= _OnValidate;
		if (this == null)
			return;

		portal1Rings.Clear( );
		portal1Rings.AddRange(portal1.GetComponentsInChildren<MeshObject>( ));
		portal2Rings.Clear( );
		portal2Rings.AddRange(portal2.GetComponentsInChildren<MeshObject>( ));

		if (portal1OutsideRing == null) {
			portal1OutsideRing = portal1.Find("Outside").GetComponent<Collider2D>( );
		}
		if (portal2OutsideRing == null) {
			portal2OutsideRing = portal2.Find("Outside").GetComponent<Collider2D>( );
		}

		if (ship != null) {
			shipCollider = ship.GetComponent<Collider2D>( );
		}

		if (portal1 != null && portal2 != null && connector != null) {
			PositionConnector( );
		}
	}

	private void Start ( ) {
		// Generate a random offset between the portals so they look different in game
		portal1AngleOffset = Random.Range(0, Mathf.PI * 2);
		portal2AngleOffset = Random.Range(0, Mathf.PI * 2);

		// Make sure the connector is in the right spot
		PositionConnector( );
	}

	private void Update ( ) {
		if (ship != null && portal1OutsideRing != null && portal2OutsideRing != null) {
			// As long as the ship has not just teleported, wait for it to touch one of the portals
			// This is needed so the ship doesn't spaz teleport between the portals
			if (teleportBufferTimer <= 0) {
				// If the ship is touching either one of the colliders of the portals, then teleport it to the opposite one
				if (shipCollider.IsTouching(portal1OutsideRing)) {
					Vector2 positionOffset = ship.Position - (Vector2) portal1.position;
					ship.Position = (Vector2) portal2.position + positionOffset;
					teleportBufferTimer = Constants.TELEPORT_BUFFER_TIME;
				} else if (shipCollider.IsTouching(portal2OutsideRing)) {
					Vector2 positionOffset = ship.Position - (Vector2) portal2.position;
					ship.Position = (Vector2) portal1.position + positionOffset;
					teleportBufferTimer = Constants.TELEPORT_BUFFER_TIME;
				}
			} else {
				teleportBufferTimer -= Time.deltaTime;
			}
		}
	}

	private void FixedUpdate ( ) {
		// Update the scale and rotation values
		scalingAngle += scaleSpeed;
		if (scalingAngle >= Mathf.PI * 2) {
			scalingAngle -= Mathf.PI * 2;
		}
		rotationAngle += rotationSpeed;
		if (rotationAngle >= Mathf.PI * 2) {
			rotationAngle -= Mathf.PI * 2;
		}

		// Animate the rings of the teleportal to make them pulse and rotate
		for (int i = 0; i < portal1Rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(scalingAngle + i + portal1AngleOffset) - scaleChange + 1;
			portal1Rings[i].transform.localScale = new Vector3(scaleValue * xScale, scaleValue, 1);
			float rotationValue = rotationAngle + (i / 20) + portal1AngleOffset;
			portal1Rings[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * rotationValue);
		}
		for (int i = 0; i < portal2Rings.Count; i++) {
			float scaleValue = scaleChange * Mathf.Sin(scalingAngle + i + portal2AngleOffset) - scaleChange + 1;
			portal2Rings[i].transform.localScale = new Vector3(scaleValue * xScale, scaleValue, 1);
			float rotationValue = rotationAngle + (i / 20) + portal2AngleOffset;
			portal2Rings[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * rotationValue);
		}
	}

	private void PositionConnector ( ) {
		// Get positions of portals and the distance between them
		Vector2 p1 = portal1.position;
		Vector2 p2 = portal2.position;
		float distance = Vector2.Distance(p1, p2);

		// Set the position of the connector to the midpoint of the 2 portals
		Vector3 connectorPosition = new Vector2(p1.x + p2.x, p1.y + p2.y) / 2;
		connector.position = Utils.SetVectZ(connectorPosition, (int) LayerType.EnvironmentGlowBack);
		// Set the rotation as the angle between the portals
		connector.rotation = Quaternion.Euler(0, 0, Utils.GetAngleBetween(p2, p1));

		// Set the width of the sprite so it connects both of the portals
		float height = connector.GetComponent<SpriteRenderer>( ).size.y;
		connector.GetComponent<SpriteRenderer>( ).size = new Vector2(distance, height);
	}
}
