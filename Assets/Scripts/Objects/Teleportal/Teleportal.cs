using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Teleportal : MonoBehaviour {
	[Separator("Teleportal")]
	[SerializeField] private Transform portal1;
	[SerializeField] private Transform portal2;
	[SerializeField] private Transform connectorBack;
	[SerializeField] private Transform connectorFront;
	[SerializeField] private GameObject connectorSegmentPrefab;
	[SerializeField] private List<MeshObject> portal1Rings = new List<MeshObject>( );
	[SerializeField] private List<MeshObject> portal2Rings = new List<MeshObject>( );
	[SerializeField] private Ship ship;
	[Space]
	[SerializeField] private float scaleSpeed;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float scaleChange;
	[SerializeField] [Range(0, 1)] private float xScale;
	[Space]
	[SerializeField] private bool regenerateConnector;
	[SerializeField] private bool forceClearLists;
	[SerializeField] [Min(1)] private int segmentDensity;

	private Collider2D portal1OutsideRing;
	private Collider2D portal2OutsideRing;
	private Collider2D shipCollider;

	private float portal1AngleOffset;
	private float portal2AngleOffset;
	private float scalingAngle;
	private float rotationAngle;

	private float teleportBufferTimer;

	public Vector2 Portal1Position {
		get {
			return portal1.position;
		}
	}

	public Vector2 Portal2Position {
		get {
			return portal2.position;
		}
	}

	public float Distance {
		get {
			return Vector2.Distance(Portal2Position, Portal1Position);
		}
	}

	public float Angle {
		get {
			return Utils.GetAngleBetween(Portal2Position, Portal1Position);
		}
	}

	protected void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		// This is used to suppress warnings that Unity oh so kindy throws when editting meshes in OnValidate
		EditorApplication.delayCall -= _OnValidate;
		if (this == null || EditorApplication.isPlayingOrWillChangePlaymode) {
			return;
		}

		portal1Rings.Clear( );
		portal2Rings.Clear( );
		portal1Rings.AddRange(portal1.GetComponentsInChildren<MeshObject>( ));
		portal2Rings.AddRange(portal2.GetComponentsInChildren<MeshObject>( ));

		if (regenerateConnector) {
			// Make sure the segments of the connector aren't too long
			// This increases the segment density until they are less than the final length
			while (Distance / segmentDensity > Constants.MAX_SEGMENT_STARTLENGTH) {
				segmentDensity++;				
			}

			// Generate the connectors
			GenerateConnectors(connectorBack, LayerType.EnvironmentGlowBack, new Color(27 / 255f, 27 / 255f, 27 / 255f));
			GenerateConnectors(connectorFront, LayerType.EnvironmentGlowFront, new Color(33 / 255f, 33 / 255f, 33 / 255f));

			regenerateConnector = false;
		}

		if (forceClearLists) {
			// Clear all children from the connector parents
			ClearConnectorChildren(connectorBack);
			ClearConnectorChildren(connectorFront);

			forceClearLists = false;
		}
	}

	private void Start ( ) {
		// Generate a random offset between the portals so they look different in game
		portal1AngleOffset = Random.Range(0, Mathf.PI * 2);
		portal2AngleOffset = Random.Range(0, Mathf.PI * 2);

		// Get colliders of ship and outer rings
		shipCollider = ship.GetComponent<Collider2D>( );
		portal1OutsideRing = portal1.Find("Outside").GetComponent<Collider2D>( );
		portal2OutsideRing = portal2.Find("Outside").GetComponent<Collider2D>( );
	}

	private void Update ( ) {
		if (ship != null) {
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

	private void GenerateConnectors (Transform connectorParent, LayerType layerType, Color color) {
		// Remove and destroy all current segments because they are going to be regenerated
		ClearConnectorChildren(connectorParent);

		Vector2 lastPoint = Vector2.zero;
		Vector2 lastCurrPoint = Vector2.zero;

		// A list of all the segments
		List<TeleportalSegment> segmentList = new List<TeleportalSegment>( );

		for (int i = 0; i <= segmentDensity; i++) {
			// Calcuate the current position of the segment connection
			// As in, where 2 segments connect ends
			Vector2 point = Utils.LinearInterpolation((float) i / segmentDensity, Portal1Position, Portal2Position);
			Vector2 currPoint = GetRandPerpPoint(point, Angle, 0, Constants.MAX_SEGMENT_OFFSET);

			if (i > 0) {
				// Create a new instance of the segment
				TeleportalSegment segment = Instantiate(connectorSegmentPrefab, connectorParent).GetComponent<TeleportalSegment>( );

				// Set all segment variables
				segment.SetPoints(lastPoint, point, lastCurrPoint, currPoint);
				segment.LayerType = layerType;
				segment.Color = color;

				segmentList.Add(segment);
			}

			// Set the neighboring segments of the current segment
			// The segments each need to know the position of the segments next to them so they always stay connected
			if (i > 1) {
				TeleportalSegment lastSegment = null;
				TeleportalSegment nextSegment = null;

				// Make sure there are segments next to the current segment
				// The first and last segments won't have both neighbors
				if (i - 3 >= 0) {
					lastSegment = segmentList[i - 3];
				}
				if (i - 1 < segmentDensity) {
					nextSegment = segmentList[i - 1];
				}

				// Set this segments neighboring segments
				segmentList[i - 2].SetSegments(this, lastSegment, nextSegment);

				// If this is the last segment, there is no next segment, so set the segments manually
				if (i == segmentDensity) {
					segmentList[i - 1].SetSegments(this, segmentList[i - 2], null);
				}
			}

			lastPoint = point;
			lastCurrPoint = currPoint;
		}
	}

	public Vector2 GetRandPerpPoint (Vector2 center, float angle, float minMag, float maxMag) {
		// Based on the angle that the portals are from each other, find 2 points on either side of an imaginary line connecting the portals
		//	and then get a random value between it. This is so the segments form a jagged line instead of a straight one.
		Vector2 minPoint = center + new Vector2(Mathf.Cos(angle + (Mathf.PI / 2)), Mathf.Sin(angle + (Mathf.PI / 2)));
		Vector2 maxPoint = center + new Vector2(Mathf.Cos(angle - (Mathf.PI / 2)), Mathf.Sin(angle - (Mathf.PI / 2)));

		// Make sure the point is not further away from the straight line than the maximum offset
		minPoint = Utils.LimitVect3(center, minPoint, minMag, maxMag);
		maxPoint = Utils.LimitVect3(center, maxPoint, minMag, maxMag);

		// Generate a random number and get a random point to set the segment intersection to
		return Utils.LinearInterpolation(Random.Range(0f, 1f), minPoint, maxPoint);
	}

	private void ClearConnectorChildren (Transform connector) {
		TeleportalSegment[ ] childList = connector.GetComponentsInChildren<TeleportalSegment>( );

		for (int i = childList.Length - 1; i >= 0; i--) {
			DestroyImmediate(childList[i].gameObject);
		}
	}
}
