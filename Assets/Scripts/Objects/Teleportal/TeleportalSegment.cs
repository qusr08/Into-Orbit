using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportalSegment : MonoBehaviour {
	[SerializeField] private SpriteRenderer spriteRenderer;

	private Vector2 point1;
	private Vector2 point2;
	public Vector2 currPoint1;
	public Vector2 currPoint2;
	private Vector2 toPoint1;
	private Vector2 toPoint2;

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
			transform.position = value;
		}
	}
	public Vector2 Point1 {
		set {
			point1 = value;
			Position = (point1 + point2) / 2f;

			UpdateVariables( );
		}
	}
	public Vector2 Point2 {
		set {
			point2 = value;
			Position = (point1 + point2) / 2f;

			UpdateVariables( );
		}
	}
	public Vector2 CurrPoint1 {
		set {
			currPoint1 = toPoint1 = value;

			UpdateVariables( );
		}
	}
	public Vector2 CurrPoint2 {
		set {
			currPoint2 = toPoint2 = value;

			UpdateVariables( );
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

	private void FixedUpdate ( ) {
		if (!Utils.CloseEnough(currPoint1, toPoint1)) {
			CurrPoint1 = Vector2.Lerp(currPoint1, toPoint1, Time.fixedDeltaTime);
		}

		if (!Utils.CloseEnough(currPoint2, toPoint2)) {
			CurrPoint2 = Vector2.Lerp(currPoint2, toPoint2, Time.fixedDeltaTime);
		}

		UpdateVariables( );
	}

	private void UpdateVariables ( ) {
		Angle = Utils.GetAngleBetween(point1, point2);
		Width = Vector2.Distance(point1, point2) + Constants.SEGMENT_OVERLAP;
	}
}
