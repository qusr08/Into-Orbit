using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateObject : MonoBehaviour {
	[Separator("Gate Object")]
	[SerializeField] private MeshObject gateTop;
	[SerializeField] private MeshObject gateCenter;
	[SerializeField] private MeshObject gateBottom;
	[Space]
	[SerializeField] private float width;

	private Vector2 gateTopPositionVelocity;
	private Vector2 gateTopScaleVelocity;
	private Vector2 gateCenterVelocity;
	private Vector2 gateBottomPositionVelocity;
	private Vector2 gateBottomScaleVelocity;

	private Vector2 toGateTopPosition;
	private Vector2 toGateTopScale;
	private Vector2 toGateBottomPosition;
	private Vector2 toGateBottomScale;

	public Vector2 Position {
		get {
			return transform.position;
		}
	}
	private bool isOpening;
	public bool IsOpen {
		set {
			if (value) {
				isOpening = value;

				toGateTopPosition = Position + new Vector2(0, width / 2);
				toGateBottomPosition = Position - new Vector2(0, width / 2);
				toGateTopScale = toGateBottomScale = Vector2.right;
			}
		}
	}

	protected void OnValidate ( ) {
		gateTop.Position = Position + new Vector2(0, gateTop.Size * (width / 2));
		gateTop.Scale = new Vector2(1, width);
		gateCenter.Position = Position;
		gateBottom.Position = Position - new Vector2(0, gateBottom.Size * (width / 2));
		gateBottom.Scale = new Vector2(1, width);
	}

	protected void FixedUpdate ( ) {
		if (isOpening) {
			gateTop.Position = Vector2.SmoothDamp(gateTop.Position, toGateTopPosition, ref gateTopPositionVelocity, Constants.PROPERTY_CHANGE_TIME);
			gateCenter.Position = Vector2.SmoothDamp(gateCenter.Position, toGateTopPosition, ref gateCenterVelocity, Constants.PROPERTY_CHANGE_TIME);
			gateBottom.Position = Vector2.SmoothDamp(gateBottom.Position, toGateBottomPosition, ref gateBottomPositionVelocity, Constants.PROPERTY_CHANGE_TIME);

			gateTop.Scale = Vector2.SmoothDamp(gateTop.Scale, toGateTopScale, ref gateTopScaleVelocity, Constants.PROPERTY_CHANGE_TIME);
			gateBottom.Scale = Vector2.SmoothDamp(gateBottom.Scale, toGateBottomScale, ref gateBottomScaleVelocity, Constants.PROPERTY_CHANGE_TIME);

			bool metGateTop = Utils.Vect3CloseEnough(gateTop.Position, toGateTopPosition) && Utils.Vect3CloseEnough(gateTop.Scale, toGateTopScale);
			bool metGateCenter = Utils.Vect3CloseEnough(gateCenter.Position, toGateTopPosition);
			bool metGateBottom = Utils.Vect3CloseEnough(gateBottom.Position, toGateBottomPosition) && Utils.Vect3CloseEnough(gateBottom.Scale, toGateBottomScale);
			if (metGateTop && metGateCenter && metGateBottom) {
				isOpening = false;

				gateTop.Position = toGateTopPosition;
				gateCenter.Position = toGateTopPosition;
				gateBottom.Position = toGateBottomPosition;
				gateTop.Scale = toGateTopScale;
				gateBottom.Scale = toGateBottomScale;

				gameObject.SetActive(false);
			}
		}
	}
}
