using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType {
	Circle,
	RoughCircle,
	Square,
	Triangle
}

public abstract class GravityObject : MonoBehaviour {
	[Header("--- Gravity Object Class ---")]
	[SerializeField] private Material baseMaterial;
	[SerializeField] protected Rigidbody2D rigidBody;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected PolygonCollider2D polyCollider;
	[Space]
	[SerializeField] public bool IsLocked;
	[SerializeField] public MeshType MeshType;
	[SerializeField] public float Size;
	[SerializeField] public float SizeToMassRatio;
	[SerializeField] protected SerializableColor color;

	protected LevelManager levelManager;

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

		set {
			transform.position = value;
		}
	}

	protected void OnValidate ( ) => UnityEditor.EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		UnityEditor.EditorApplication.delayCall -= _OnValidate;
		if (this == null)
			return;

		GenerateMesh( );
		Mass = Size * SizeToMassRatio;
	}

	protected void Awake ( ) {
		levelManager = FindObjectOfType<LevelManager>( );

		rigidBody.isKinematic = IsLocked;
	}

	protected void FixedUpdate ( ) {
		if (!IsLocked) {
			rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
		}
	}

	protected void GenerateMesh ( ) {
		// https://stackoverflow.com/questions/50606756/creating-a-2d-circular-mesh-in-unity

		Mesh mesh = new Mesh( );
		meshFilter.mesh = mesh;

		// Generate Vertices
		List<Vector3> verticesList = new List<Vector3>( );

		switch (MeshType) {
			case MeshType.Circle:
				float x1;
				float y1;
				for (int i = 0; i < Constants.CIRCLE_PRECISION; i++) {
					x1 = Size * Mathf.Sin((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);
					y1 = Size * Mathf.Cos((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);

					verticesList.Add(new Vector3(x1, y1, 0f));
				}

				break;
			case MeshType.RoughCircle:
				float x2;
				float y2;
				for (int i = 0; i < Constants.CIRCLE_PRECISION; i++) {
					x2 = (Size + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Sin((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);
					y2 = (Size + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Cos((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);

					verticesList.Add(new Vector3(x2, y2, 0f));
				}

				break;
			case MeshType.Square:
				verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
				verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

				break;
			case MeshType.Triangle:
				verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
				verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

				break;
		}

		Vector3[ ] vertices = verticesList.ToArray( );

		// Generate Triangles from vertices
		List<int> trianglesList = new List<int>( );

		switch (MeshType) {
			case MeshType.Circle:
				for (int i = 0; i < (Constants.CIRCLE_PRECISION - 2); i++) {
					trianglesList.Add(0);
					trianglesList.Add(i + 1);
					trianglesList.Add(i + 2);
				}

				break;
			case MeshType.RoughCircle:
				for (int i = 0; i < (Constants.CIRCLE_PRECISION - 2); i++) {
					trianglesList.Add(0);
					trianglesList.Add(i + 1);
					trianglesList.Add(i + 2);
				}

				break;
			case MeshType.Square:
				trianglesList.Add(0);
				trianglesList.Add(1);
				trianglesList.Add(3);

				trianglesList.Add(1);
				trianglesList.Add(2);
				trianglesList.Add(3);

				break;
			case MeshType.Triangle:
				trianglesList.Add(0);
				trianglesList.Add(1);
				trianglesList.Add(2);

				break;
		}

		int[ ] triangles = trianglesList.ToArray( );

		// Generate normals for all vertices
		List<Vector3> normalsList = new List<Vector3>( );

		for (int i = 0; i < vertices.Length; i++) {
			normalsList.Add(-Vector3.forward);
		}

		Vector3[ ] normals = normalsList.ToArray( );

		// Initialize the mesh with all of the calculated values
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		SetColor(color);

		// Edit the collider of the game object to take the shape of the mesh
		polyCollider.pathCount = 1;
		List<Vector2> pathList = new List<Vector2>( );

		for (int i = 0; i < vertices.Length; i++) {
			pathList.Add(new Vector2(vertices[i].x, vertices[i].y));
		}

		Vector2[ ] path = pathList.ToArray( );
		polyCollider.SetPath(0, path);
	}

	public void SetColor (Color color) {
		this.color = color;

		Material material = new Material(baseMaterial);
		material.SetColor("_Color", color);
		meshRenderer.material = material;
	}
}
