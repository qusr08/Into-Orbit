using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpaceObject : MonoBehaviour {
	[Separator("Space Object")]
	[SerializeField] protected List<MeshObject> rings = new List<MeshObject>( );
	[Space]
	[SerializeField] protected float scaleChange;
	[SerializeField] private bool randomScaleStartingAngle;
	[SerializeField] private bool randomRotationStartingAngle;
	[SerializeField] private float scaleSpeed;
	[SerializeField] private float rotationSpeed;

	protected MeshObject outsideRing;
	protected MeshObject middleRing;
	protected MeshObject insideRing;
	protected float scalingAngle;
	protected float rotationAngle;

	private float startingRotationModAngle;
	private float rotationSpeedMod = 1;
	private bool doingSpin;

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

	protected void OnValidate ( ) {
		rings.Clear( );
		rings.AddRange(GetComponentsInChildren<MeshObject>( ));

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
	}

	protected void FixedUpdate ( ) {
		// Update the scale and rotation values
		scalingAngle += scaleSpeed;
		if (scalingAngle >= Mathf.PI * 2) {
			scalingAngle -= Mathf.PI * 2;
		}

		if (doingSpin) {
			rotationAngle += rotationSpeed * rotationSpeedMod;
			if (rotationAngle - Mathf.PI * 2 >= startingRotationModAngle) {
				doingSpin = false;
			}
		} else {
			rotationAngle += rotationSpeed;
			if (rotationAngle >= Mathf.PI * 2) {
				rotationAngle -= Mathf.PI * 2;
			}
		}

		Animate( );
	}

	protected abstract void Animate ( );
	public abstract void OnObjectCollision ( );

	protected void DoQuickSpin (float speedMod) {
		doingSpin = true;

		rotationSpeedMod = speedMod;
		startingRotationModAngle = rotationAngle;
	}
}
