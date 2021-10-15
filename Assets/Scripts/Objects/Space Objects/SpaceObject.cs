using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpaceObject : MonoBehaviour {
	[Separator("Space Object")]
	[SerializeField] protected List<MeshObject> rings = new List<MeshObject>( );
	[SerializeField] protected LevelManager levelManager;
	[SerializeField] protected UIManager uiManager;
	[Space]
	[SerializeField] private bool randomScaleStartingAngle;
	[SerializeField] private bool randomRotationStartingAngle;
	[SerializeField] private bool randomScaleAngleDirection;
	[SerializeField] private bool randomRotationAngleDirection;
	[SerializeField] protected float scaleChange;
	[SerializeField] private float scaleSpeed;
	[SerializeField] private float rotationSpeed;

	protected MeshObject outsideRing;
	protected MeshObject middleRing;
	protected MeshObject insideRing;

	protected float scalingAngle;
	protected float scalingAngleMod;
	protected float rotationAngle;
	protected float rotationAngleMod;

	private bool doAnimation;
	private bool stopAnimatingWhenDone;

	private bool isShrinking;
	private float scalingMod;
	private Vector2 startingScale;
	private Vector2 scaleVelocity;

	private bool isChangingColor;
	private Color toInsideRingColor;
	private Color toMiddleRingColor;
	private Color toOutsideRingColor;
	private Vector3 insideRingColorVelocity;
	private Vector3 middleRingColorVelocity;
	private Vector3 outsideRingColorVelocity;

	public float Radius {
		get {
			return outsideRing.Size / 2;
		}
	}

	public Vector2 Position {
		get {
			return transform.position;
		}
	}

	public Vector2 Scale {
		get {
			return transform.localScale;
		}

		set {
			transform.localScale = new Vector3(Utils.Limit(value.x, 0, 100), Utils.Limit(value.y, 0, 100), 1);
		}
	}

	protected void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));

		if (levelManager == null) {
			levelManager = FindObjectOfType<LevelManager>( );
		}
		if (uiManager == null) {
			uiManager = FindObjectOfType<UIManager>( );
		}

		if (outsideRing == null) {
			if (transform.Find("Outside") != null) {
				outsideRing = transform.Find("Outside").GetComponent<MeshObject>( );
			}
		}
		if (middleRing == null) {
			if (transform.Find("Middle") != null) {
				middleRing = transform.Find("Middle").GetComponent<MeshObject>( );
			}
		}
		if (insideRing == null) {
			if (transform.Find("Inside") != null) {
				insideRing = transform.Find("Inside").GetComponent<MeshObject>( );
			}
		}
	}

	protected void Start ( ) {
		if (randomScaleStartingAngle) {
			scalingAngle = Random.Range(0, Mathf.PI * 2);
		}
		if (randomRotationStartingAngle) {
			scalingAngle = Random.Range(0, Mathf.PI * 2);
		}

		scalingAngleMod = rotationAngleMod = 1;
		if (randomScaleAngleDirection) {
			scalingAngleMod = Utils.RandBool( ) ? 1 : -1;
		}
		if (randomRotationAngleDirection) {
			rotationAngleMod = Utils.RandBool( ) ? 1 : -1;
		}

		doAnimation = true;
		scalingMod = 1;
		startingScale = Scale;
	}

	protected void FixedUpdate ( ) {
		if (doAnimation) {
			// Update the scale and rotation values
			scalingAngle = Utils.LoopRange(scalingAngle, scaleSpeed * scalingAngleMod, 0, Mathf.PI * 2);
			rotationAngle = Utils.LoopRange(rotationAngle, rotationSpeed * rotationAngleMod, 0, Mathf.PI * 2);

			if (isShrinking) {
				Scale = Vector2.SmoothDamp(Scale, startingScale * scalingMod, ref scaleVelocity, Constants.PROPERTY_CHANGE_TIME);

				if (Utils.Vect3CloseEnough(Scale, startingScale * scalingMod)) {
					doAnimation = !stopAnimatingWhenDone;

					isShrinking = false;
					Scale = startingScale * scalingMod;
				}
			}

			if (isChangingColor) {
				if (insideRing != null) {
					insideRing.Color = Utils.ColorSmoothDamp(insideRing.Color, toInsideRingColor, ref insideRingColorVelocity, Constants.PROPERTY_CHANGE_TIME * 2);
				}
				if (middleRing != null) {
					middleRing.Color = Utils.ColorSmoothDamp(middleRing.Color, toMiddleRingColor, ref middleRingColorVelocity, Constants.PROPERTY_CHANGE_TIME * 2);
				}
				if (outsideRing != null) {
					outsideRing.Color = Utils.ColorSmoothDamp(outsideRing.Color, toOutsideRingColor, ref outsideRingColorVelocity, Constants.PROPERTY_CHANGE_TIME * 2);
				}

				bool metInsideRingColor = insideRing == null || Utils.ColorCloseEnough(insideRing.Color, toInsideRingColor);
				bool metMiddleRingColor = middleRing == null || Utils.ColorCloseEnough(middleRing.Color, toMiddleRingColor);
				bool metOutsideRingColor = outsideRing == null || Utils.ColorCloseEnough(outsideRing.Color, toOutsideRingColor);

				if (metInsideRingColor && metMiddleRingColor && metOutsideRingColor) {
					doAnimation = !stopAnimatingWhenDone;

					isChangingColor = false;
					if (insideRing != null) {
						insideRing.Color = toInsideRingColor;
					}
					if (middleRing != null) {
						middleRing.Color = toMiddleRingColor;
					}
					if (outsideRing != null) {
						outsideRing.Color = toOutsideRingColor;
					}
				}
			}

			Animate( );
		}
	}

	protected abstract void Animate ( );
	public abstract void OnObjectCollision (GameObject collisionObject);

	protected void Shrink (float scalingMod, bool stopAnimatingWhenDone = false) {
		this.scalingMod = scalingMod;

		if (!this.stopAnimatingWhenDone) {
			this.stopAnimatingWhenDone = stopAnimatingWhenDone;
		}

		isShrinking = true;
		startingScale = Scale;
	}

	protected void ChangeColorOfRings (Color toInsideRingColor = default(Color), Color toMiddleRingColor = default(Color), Color toOutsideRingColor = default(Color), bool stopAnimatingWhenDone = false) {
		this.toInsideRingColor = toInsideRingColor;
		this.toMiddleRingColor = toMiddleRingColor;
		this.toOutsideRingColor = toOutsideRingColor;

		if (!this.stopAnimatingWhenDone) {
			this.stopAnimatingWhenDone = stopAnimatingWhenDone;
		}

		isChangingColor = true;
	}
}
