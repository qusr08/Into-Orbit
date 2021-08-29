using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MonoBehaviour {
	[Header("--- Gravity Object Class ---")]
	[SerializeField] protected Rigidbody2D rigidBody;
	[SerializeField] public MeshFilter meshFilter;
	[SerializeField] public MeshRenderer meshRenderer;
	[SerializeField] public PolygonCollider2D polyCollider;
	[Space]
	[SerializeField] public bool IsLocked;
	[SerializeField] public float Size;
	[SerializeField] [Range(0, 255)] protected float ColorR;
	[SerializeField] [Range(0, 255)] protected float ColorG;
	[SerializeField] [Range(0, 255)] protected float ColorB;

	private LevelManager levelManager;

	public float Mass {
		get {
			return rigidBody.mass;
		}

		set {
			rigidBody.mass = value;
		}
	}
	public Vector2 Position {
		get {
			return transform.position;
		}
	}

	protected void OnValidate ( ) => UnityEditor.EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		UnityEditor.EditorApplication.delayCall -= _OnValidate;
		if (this == null)
			return;

		GenerateMesh( );
	}

	protected void Awake ( ) {
		rigidBody.isKinematic = IsLocked;

		levelManager = FindObjectOfType<LevelManager>( );
	}

	protected void FixedUpdate ( ) {
		if (!IsLocked) {
			rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
		}
	}

	protected abstract void GenerateMesh ( );

	protected Vector3[ ] GenerateNormals (Vector3[ ] vertices) {
		List<Vector3> normalsList = new List<Vector3>( );

		for (int i = 0; i < vertices.Length; i++) {
			normalsList.Add(-Vector3.forward);
		}

		return normalsList.ToArray( );
	}

	protected void UpdatePolyCollider (Vector3[ ] vertices) {
		polyCollider.pathCount = 1;
		List<Vector2> pathList = new List<Vector2>( );

		for (int i = 0; i < vertices.Length; i++) {
			pathList.Add(new Vector2(vertices[i].x, vertices[i].y));
		}

		Vector2[ ] path = pathList.ToArray( );
		polyCollider.SetPath(0, path);
	}

	protected void SetMeshColor (Color color) {
		meshRenderer.sharedMaterial.SetColor("_Color", new Color(color.r / 255f, color.g / 255f, color.b / 255f));
	}
}
