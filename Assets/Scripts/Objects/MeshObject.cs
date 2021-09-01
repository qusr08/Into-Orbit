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
	EnvironmentGlowBack = 95,
	EnvironmentGlowFront = 94,
	WormholeOutside = 82,
	WormholeMiddle = 81,
	WormholeInside = 80,
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
	[SerializeField] private MeshType meshType = MeshType.Circle;
	[SerializeField] private LayerType layerType = LayerType.Front;
	[SerializeField] private SerializableColor color;
	[Space]
	[SerializeField] private float size = 1;
	[SerializeField] private float sizeToMassRatio = 1;
	[SerializeField] private bool isLocked = false;
	[SerializeField] private bool disableColliders = false;
	[Header("--- Mesh Object Constants ---")]
	[SerializeField] private int CIRCLE_MESH_PRECISION = 20;

	protected LevelManager levelManager;

	public MeshType MeshType {
		get {
			return meshType;
		}

		set {
			meshType = value;

			GenerateMesh( );
		}
	}
	public LayerType LayerType {
		get {
			return layerType;
		}

		set {
			layerType = value;

			// Update the layer
			transform.position = new Vector3(transform.position.x, transform.position.y, (int) LayerType);
		}
	}
	public Color Color {
		get {
			return color;
		}

		set {
			color = value;

			UpdateColor( );
		}
	}
	public float Size {
		get {
			return size;
		}

		set {
			size = value;

			// Update the mass
			Mass = Size * SizeToMassRatio;

			// Regenerate the mesh because the size has changed
			GenerateMesh( );
		}
	}
	public float SizeToMassRatio {
		get {
			return sizeToMassRatio;
		}

		set {
			sizeToMassRatio = value;

			// Update the mass
			Mass = Size * sizeToMassRatio;
		}
	}
	public bool IsLocked {
		get {
			return isLocked;
		}

		set {
			isLocked = value;

			// If the object is locked, it should not be able to move
			rigidBody.constraints = (isLocked) ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
		}
	}
	public bool DisableColliders {
		get {
			return disableColliders;
		}

		set {
			disableColliders = value;

			polyCollider.enabled = !DisableColliders;
		}
	}
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
			rigidBody = (GetComponent<Rigidbody2D>( ) == null) ? gameObject.AddComponent<Rigidbody2D>( ) : GetComponent<Rigidbody2D>( );
		}
		if (meshFilter == null) {
			meshFilter = (GetComponent<MeshFilter>( ) == null) ? gameObject.AddComponent<MeshFilter>( ) : GetComponent<MeshFilter>( );
		}
		if (meshRenderer == null) {
			meshRenderer = (GetComponent<MeshRenderer>( ) == null) ? gameObject.AddComponent<MeshRenderer>( ) : GetComponent<MeshRenderer>( );
		}
		if (polyCollider == null) {
			polyCollider = (GetComponent<PolygonCollider2D>( ) == null) ? gameObject.AddComponent<PolygonCollider2D>( ) : GetComponent<PolygonCollider2D>( );
		}

		UpdateVariables( );
	}

	protected void Awake ( ) {
		// Find the level manager (so gravitational forces can be calculated)
		levelManager = FindObjectOfType<LevelManager>( );
	}

	protected void UpdateVariables ( ) {
		// Reset and update component variables
		rigidBody.bodyType = RigidbodyType2D.Dynamic;
		rigidBody.gravityScale = 0;
		rigidBody.angularDrag = 0;
		rigidBody.drag = 0;

		// Update all class variables
		MeshType = MeshType;
		LayerType = LayerType;
		Color = Color;
		Size = Size;
		SizeToMassRatio = SizeToMassRatio;
		IsLocked = IsLocked;
		DisableColliders = DisableColliders;

		// Regenerate the mesh;
		GenerateMesh( );
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
					x1 = (Size / 2) * Mathf.Sin((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);
					y1 = (Size / 2) * Mathf.Cos((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);

					verticesList.Add(new Vector3(x1, y1, 0f));
				}

				break;
			case MeshType.RoughCircle:
				float x2;
				float y2;
				for (int i = 0; i < CIRCLE_MESH_PRECISION; i++) {
					x2 = ((Size / 2) + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Sin((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);
					y2 = ((Size / 2) + Utils.RandFloat(-0.05f, 0.05f)) * Mathf.Cos((2 * Mathf.PI * i) / CIRCLE_MESH_PRECISION);

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
		UpdateColor( );

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

	public void UpdateColor ( ) {
		// Create a new temporary material from the base material
		Material material = new Material(baseMaterial);
		// Set the temporary material color
		material.SetColor("_Color", Color);
		// Set the mesh material to this temporary materal
		// This is needed so objects can all have the same material but all be different colors
		meshRenderer.material = material;
	}
}