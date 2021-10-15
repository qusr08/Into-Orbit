using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportalSegment : MonoBehaviour {
	[Separator("Teleportal Segment")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Space]
	[SerializeField] private TeleportalSegment lastSegment;
	[SerializeField] private TeleportalSegment nextSegment;
	[SerializeField] private Teleportal teleportal;
	[Space]
	[SerializeField] private Vector2 centerPoint1;
	[SerializeField] private Vector2 centerPoint2;
	[SerializeField] private Vector2 currPoint1;
	[SerializeField] private Vector2 currPoint2;
	[SerializeField] private Vector2 fromPoint1;
	[SerializeField] private Vector2 fromPoint2;
	[SerializeField] private Vector2 toPoint1;
	[SerializeField] private Vector2 toPoint2;

	private float moveTime1;
	private float moveTime2;
	private float moveTimeRemaining1;
	private float moveTimeRemaining2;

	public float Width {
		get {
			return spriteRenderer.size.x;
		}

		set {
			spriteRenderer.size = new Vector2(value, spriteRenderer.size.y);
		}
	}
	public Color Color {
		get {
			return spriteRenderer.color;
		}

		set {
			spriteRenderer.color = value;
		}
	}
	public float Angle {
		get {
			return transform.eulerAngles.z;
		}

		set {
			transform.rotation = Quaternion.Euler(0, 0, value);
		}
	}
	public Vector2 Position {
		get {
			return transform.position;
		}

		set {
			transform.position = new Vector3(value.x, value.y, (int) LayerType);
		}
	}
	public LayerType LayerType {
		get {
			return (LayerType) transform.position.z;
		}

		set {
			transform.position = new Vector3(Position.x, Position.y, (int) value);
		}
	}

	protected void Update ( ) {
		// Update the move time of both ends of this segment
		moveTimeRemaining1 -= Time.deltaTime;
		moveTimeRemaining2 -= Time.deltaTime;

		// If both this segment and the last segment are done moving, generate a new move time and a new point to move to
		if (lastSegment != null) {
			if (lastSegment.moveTimeRemaining2 <= 0 && moveTimeRemaining1 <= 0) {
				// Generate a new move time
				float newMoveTime = Random.Range(Constants.MIN_SEGMENT_MOVETIME, Constants.MAX_SEGMENT_MOVETIME);
				lastSegment.moveTimeRemaining2 = lastSegment.moveTime2 = moveTimeRemaining1 = moveTime1 = newMoveTime;

				// Generate a new point
				Vector2 newToPoint = teleportal.GetRandPerpPoint(centerPoint1, teleportal.Angle, 0, Constants.MAX_SEGMENT_OFFSET);
				fromPoint1 = lastSegment.fromPoint2 = currPoint1;
				toPoint1 = lastSegment.toPoint2 = newToPoint;

				// FindObjectOfType<LevelManager>( ).SpawnStationaryPieces(toPoint1, 1, Color.red);
			}
		}

		// If both this segment and the next segment are done moving, generate a new move time and a new point to move to
		if (nextSegment != null) {
			if (nextSegment.moveTimeRemaining1 <= 0 && moveTimeRemaining2 <= 0) {
				// Generate a new move time
				float newMoveTime = Random.Range(Constants.MIN_SEGMENT_MOVETIME, Constants.MAX_SEGMENT_MOVETIME);
				nextSegment.moveTimeRemaining1 = nextSegment.moveTime1 = moveTimeRemaining2 = moveTime2 = newMoveTime;

				// Generate a new point
				Vector2 newToPoint = teleportal.GetRandPerpPoint(centerPoint2, teleportal.Angle, 0, Constants.MAX_SEGMENT_OFFSET);
				fromPoint2 = nextSegment.fromPoint1 = currPoint2;
				toPoint2 = nextSegment.toPoint1 = newToPoint;

				// FindObjectOfType<LevelManager>( ).SpawnStationaryPieces(toPoint2, 1, Color.red);
			}
		}
	}

	protected void FixedUpdate ( ) {
		// If the current point is not equal to the point it should be, move the current point gradually based on the move time
		if (!Utils.Vect3CloseEnough(currPoint1, toPoint1) && moveTime1 != 0) {
			currPoint1 = Vector2.Lerp(fromPoint1, toPoint1, 1 - (moveTimeRemaining1 / moveTime1));
		}
		if (!Utils.Vect3CloseEnough(currPoint2, toPoint2) && moveTime2 != 0) {
			currPoint2 = Vector2.Lerp(fromPoint2, toPoint2, 1 - (moveTimeRemaining2 / moveTime2));
		}

		// Update the position, angle, and width of the segment
		UpdateVariables( );
	}

	private void UpdateVariables ( ) {
		Position = (currPoint1 + currPoint2) / 2f;
		Angle = Utils.GetAngleBetween(currPoint1, currPoint2);
		Width = Vector2.Distance(currPoint1, currPoint2) + Constants.SEGMENT_OVERLAP;
	}

	public void SetSegments (Teleportal teleportal, TeleportalSegment lastSegment, TeleportalSegment nextSegment) {
		this.teleportal = teleportal;
		this.lastSegment = lastSegment;
		this.nextSegment = nextSegment;
	}

	public void SetPoints (Vector2 centerPoint1, Vector2 centerPoint2, Vector2 currPoint1, Vector2 currPoint2) {
		this.centerPoint1 = centerPoint1;
		this.centerPoint2 = centerPoint2;
		this.currPoint1 = toPoint1 = fromPoint1 = currPoint1;
		this.currPoint2 = toPoint2 = fromPoint2 = currPoint2;

		UpdateVariables( );
	}
}
