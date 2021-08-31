using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The type of mesh (shape) that the object will be
public enum MeshType {
	Circle,
	RoughCircle,
	Square,
	Triangle
}

public enum LayerType {
	Back = 100,
	Environment = 70,
	ShipDetail = 40,
	Ship = 30,
	Front = 0
}

public class MeshObject : MonoBehaviour {
	[Header("--- Mesh Object Class ---")]
	[SerializeField] private Material baseMaterial;
	[SerializeField] protected Rigidbody2D rigidBody;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected PolygonCollider2D polyCollider;
	[Space]
	[SerializeField] public MeshType MeshType = MeshType.Circle;
	[SerializeField] public LayerType LayerType = LayerType.Front;
	[SerializeField] protected SerializableColor color;
	[Space]
	[SerializeField] public float Size = 1;
	[SerializeField] public float SizeToMassRatio = 1;
	[Header("--- Mesh Object Constants ---")]
	[SerializeField] private int CIRCLE_MESH_PRECISION = 20;

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
			transform.position = new Vector3(value.x, value.y, transform.position.z);
		}
	}

	protected void OnValidate ( ) => UnityEditor.EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		// This is used to suppress warnings that Unity oh so kindy throws when editting meshes in OnValidate
		UnityEditor.EditorApplication.delayCall -= _OnValidate;
		if (this == null)
			return;

		// Make sure all components of the object are not null
		if (rigidBody == null) {
			rigidBody = GetComponent<Rigidbody2D>( );
		}
		if (meshFilter == null) {
			meshFilter = GetComponent<MeshFilter>( );
		}
		if (meshRenderer == null) {
			meshRenderer = GetComponent<MeshRenderer>( );
		}
		if (polyCollider == null) {
			polyCollider = GetComponent<PolygonCollider2D>( );
		}

		// Update the layer
		transform.position = new Vector3(transform.position.x, transform.position.y, (int) LayerType);

		// Regenerate the mesh of the object
		GenerateMesh( );
		// Recalculate the mass of the object
		Mass = Size * SizeToMassRatio;
	}

	protected void Awake ( ) {
		// Find the level manager (so gravitational forces can be calculated)
		levelManager = FindObjectOfType<LevelManager>( );

		// Call OnValidate one more time just to make sure the Mass and mesh are calculated correctly
		OnValidate( );
	}

	protected void GenerateMesh ( ) {
		// https://stackoverflow.com/questions/50606756/creating-a-2d-circular-mesh-in-unity

		// Create a new blank mesh
		Mesh mesh = new Mesh( );
		meshFilter.mesh = mesh;

		// Generate Vertices
		List<Vector3> verticesList = new List<Vector3>( );

		// Based on the type (shape) of mesh specified, generate the vertices
		switch (MeshType) {
			case MeshType.Circle:
				float x1;
				float y1;
				for (int i = 0; i < CIRCLE_MESH_PRECISION; i++) {
					x1 = Size * Mathf.Sin((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);
					y1 = Size * Mathf.Cos((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);

					verticesList.Add(new Vector3(x1, y1, 0f));
				}

				break;
			case MeshType.RoughCircle:
				float x2;
				float y2;
				for (int i = 0; i < CIRCLE_MESH_PRECISION; i++) {
					x2 = (Size + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Sin((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);
					y2 = (Size + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Cos((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);

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

		// Based on the type (shape) of the mesh, generate the triangles from the vertices
		switch (MeshType) {
			case MeshType.Circle:
				for (int i = 0; i < (CIRCLE_MESH_PRECISION - 2); i++) {
					trianglesList.Add(0);
					trianglesList.Add(i + 1);
					trianglesList.Add(i + 2);
				}

				break;
			case MeshType.RoughCircle:
				for (int i = 0; i < (CIRCLE_MESH_PRECISION - 2); i++) {
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

		// *** Make sure the vertices are in a circular order or else the poly collider will be messed up
		for (int i = 0; i < vertices.Length; i++) {
			pathList.Add(new Vector2(vertices[i].x, vertices[i].y));
		}

		Vector2[ ] path = pathList.ToArray( );
		polyCollider.SetPath(0, path);
	}

	public void SetColor (Color color) {
		this.color = color;

		// Create a new temporary material from the base material
		Material material = new Material(baseMaterial);
		// Set the temporary material color
		material.SetColor("_Color", color);
		// Set the mesh material to this temporary materal
		// This is needed so objects can all have the same material but all be different colors
		meshRenderer.material = material;
	}
}
