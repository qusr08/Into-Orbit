using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Teleportal : MonoBehaviour {
	[Separator("Teleportal")]
	[SerializeField] private TeleportalPortal portal1;
	[SerializeField] private TeleportalPortal portal2;
	[SerializeField] private Transform connectorBack;
	[SerializeField] private Transform connectorFront;
	[SerializeField] private GameObject connectorSegmentPrefab;
	[Space]
	[SerializeField] private bool regenerateConnector;
	[SerializeField] private bool forceClearLists;
	[SerializeField] [Min(1)] private int segmentDensity;

	public Vector2 Portal1Position {
		get {
			return portal1.Position;
		}
	}

	public Vector2 Portal2Position {
		get {
			return portal2.Position;
		}
	}

	public float Distance {
		get {
			return Vector2.Distance(Portal2Position, Portal1Position);
		}
	}

	public float Angle {
		get {
			// Debug.Log(Utils.GetAngleBetween(Portal2Position, Portal1Position));
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

		if (regenerateConnector) {
			// Make sure the segments of the connector aren't too long
			// This increases the segment density until they are less than the final length
			while (Distance / segmentDensity > Constants.MAX_SEGMENT_STARTLENGTH) {
				segmentDensity++;
			}

			// Generate the connectors
			GenerateConnectors(connectorBack, LayerType.EnvironmentGlowBack, new Color(27 / 255f, 27 / 255f, 27 / 255f));
			GenerateConnectors(connectorFront, LayerType.EnvironmentGlowFront, new Color(35 / 255f, 35 / 255f, 35 / 255f));

			regenerateConnector = false;
		}

		if (forceClearLists) {
			// Clear all children from the connector parents
			ClearConnectorChildren(connectorBack);
			ClearConnectorChildren(connectorFront);

			forceClearLists = false;
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
		Vector2 minPoint = center + new Vector2(Mathf.Cos(Mathf.Deg2Rad * (angle + 90)), Mathf.Sin(Mathf.Deg2Rad * (angle + 90)));
		Vector2 maxPoint = center + new Vector2(Mathf.Cos(Mathf.Deg2Rad * (angle - 90)), Mathf.Sin(Mathf.Deg2Rad * (angle - 90)));

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

	public Vector2 GetTeleportPosition (TeleportalPortal portal) {
		return (portal == portal1 ? Portal2Position : Portal1Position);
	}
}
