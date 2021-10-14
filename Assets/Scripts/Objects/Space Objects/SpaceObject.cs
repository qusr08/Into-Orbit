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

	private MeshObject outsideRing;
	protected float scalingAngle;
	protected float rotationAngle;

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
			outsideRing = transform.Find("Outside").GetComponent<MeshObject>( );
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

	protected void OnCollisionEnter2D (Collision2D collision) {
		if (collision.transform.tag.Equals("Space Object")) {
			OnObjectCollision( );
		}
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

		Animate( );
	}

	protected abstract void Animate ( );
	protected abstract void OnObjectCollision ( );
}
