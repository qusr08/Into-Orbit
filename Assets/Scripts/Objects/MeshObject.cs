using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshObject : MonoBehaviour {
	[Separator("Mesh Object")]
	[SerializeField] private bool recalculateMesh;
	[Space]
	[SerializeField] private Material meshMaterial;
	[SerializeField] private Material trailMaterial;
	[Space]
	[SerializeField] protected LineRenderer trailRenderer;
	[SerializeField] protected Rigidbody2D rigidBody;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected PolygonCollider2D polyCollider;
	[SerializeField] protected LevelManager levelManager;
	[SerializeField] protected CameraController cameraController;
	[Space]
	[SerializeField] private bool showTrail = false;
	[SerializeField] private bool isLocked = false;
	[SerializeField] private bool disableSolidColliders = false;
	[SerializeField] private bool enableCollisions = true;
	[Space]
	[SerializeField] private SerializableColor3 color;
	[SerializeField] private MeshType meshType = MeshType.Circle;
	[SerializeField] private LayerType layerType = LayerType.Front;
	[SerializeField] private float size = 1;
	[SerializeField] private float sizeToMassRatio = 1;
	[SerializeField] [ConditionalField("meshType", false, MeshType.Circle, MeshType.RoughCircle)] private int meshPrecision = 20;
	[SerializeField] [ConditionalField("meshType", false, MeshType.RoughCircle)] private float meshRoughness = 0.1f;
	[SerializeField] [ConditionalField("showTrail")] private bool baseTrailColorOffMesh = false;
	[SerializeField] [ConditionalField("showTrail")] private SerializableColor4 trailStartColor;
	[SerializeField] [ConditionalField("showTrail")] private SerializableColor4 trailEndColor;
	[SerializeField] [ConditionalField("showTrail")] private int trailLength = 5;
	[SerializeField] [ConditionalField("showTrail")] [Range(0f, 1f)] private float trailToObjectScale = 1;

	private List<Vector3> lastTrailPositions = new List<Vector3>( );

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
			Mass = size * SizeToMassRatio;

			// Update trail size
			trailRenderer.startWidth = trailToObjectScale * size;

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
	public bool ShowTrail {
		get {
			return showTrail;
		}

		set {
			showTrail = value;

			trailRenderer.enabled = showTrail;
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
	public bool DisableSolidColliders {
		get {
			return disableSolidColliders;
		}

		set {
			disableSolidColliders = value;

			polyCollider.enabled = true;
			polyCollider.isTrigger = DisableSolidColliders;
		}
	}
	public bool EnableCollisions {
		get {
			return enableCollisions;
		}

		set {
			enableCollisions = value;

			polyCollider.enabled = value;
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
	public Vector2 Scale {
		get {
			return transform.localScale;
		}

		set {
			transform.localScale = new Vector2(Utils.Limit(value.x, 0, 100), Utils.Limit(value.y, 0, 100));
		}
	}

	protected void OnValidate ( ) => UnityEditor.EditorApplication.delayCall += _OnValidate;
	private void _OnValidate ( ) {
		// This is used to suppress warnings that Unity oh so kindy throws when editting meshes in OnValidate
		UnityEditor.EditorApplication.delayCall -= _OnValidate;
		if (this == null)
			return;

		// Make sure all components of the object are not null
		if (levelManager == null) {
			levelManager = FindObjectOfType<LevelManager>( );
		}
		if (cameraController == null) {
			cameraController = FindObjectOfType<CameraController>( );
		}

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
		if (trailRenderer == null) {
			trailRenderer = (GetComponent<LineRenderer>( ) == null) ? gameObject.AddComponent<LineRenderer>( ) : GetComponent<LineRenderer>( );
		}

		if (recalculateMesh) {
			recalculateMesh = false;
		}

		UpdateVariables( );
	}

	protected void Awake ( ) {
		levelManager = FindObjectOfType<LevelManager>( );
	}

	protected void FixedUpdate ( ) {
		UpdateTrail( );
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
				for (int i = 0; i < meshPrecision; i++) {
					x1 = (Size / 2) * Mathf.Sin((2 * Mathf.PI * i) / meshPrecision);
					y1 = (Size / 2) * Mathf.Cos((2 * Mathf.PI * i) / meshPrecision);

					verticesList.Add(new Vector3(x1, y1, 0f));
				}

				break;
			case MeshType.RoughCircle:
				float x2;
				float y2;
				for (int i = 0; i < meshPrecision; i++) {
					x2 = ((Size / 2) + Utils.RandFloat(-meshRoughness, meshRoughness)) * Mathf.Sin((2 * Mathf.PI * i) / meshPrecision);
					y2 = ((Size / 2) + Utils.RandFloat(-meshRoughness, meshRoughness)) * Mathf.Cos((2 * Mathf.PI * i) / meshPrecision);

					verticesList.Add(new Vector3(x2, y2, 0f));
				}

				break;
			case MeshType.Square:
				verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
				verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

				break;
			case MeshType.RightTriangle:
				verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
				verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
				verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

				break;
			case MeshType.EquilateralTriangle:
				verticesList.Add(new Vector3(0, Size, 0));
				verticesList.Add(new Vector3(Size * 0.866f, -Size / 2, 0));
				verticesList.Add(new Vector3(-Size * 0.866f, -Size / 2, 0));

				break;
		}

		Vector3[ ] vertices = verticesList.ToArray( );

		// Generate Triangles from vertices
		List<int> trianglesList = new List<int>( );

		// Based on the type (shape) of the mesh, generate the triangles from the vertices
		switch (MeshType) {
			case MeshType.Circle:
				for (int i = 0; i < (meshPrecision - 2); i++) {
					trianglesList.Add(0);
					trianglesList.Add(i + 1);
					trianglesList.Add(i + 2);
				}

				break;
			case MeshType.RoughCircle:
				for (int i = 0; i < (meshPrecision - 2); i++) {
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
			case MeshType.RightTriangle:
				trianglesList.Add(0);
				trianglesList.Add(1);
				trianglesList.Add(2);

				break;
			case MeshType.EquilateralTriangle:
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

		/*
		
		SO BASICALLY this is some 11/10 code but I dont think we are gonna use it currently. I do want to keep it doe
		
		I encountered a problem where I couldnt find a material that was unlit and had the ability to set the vertex color. I tried
		the Standard Particle Unlit material but that had some weird settings that caused the color of the objects to change and not
		be what I set them to. Eventually I came to the conclusion that (at least for now) a plain color might look better than the
		textured one I wanted to do originally.

		// https://stackoverflow.com/questions/45854076/set-color-for-each-vertex-in-a-triangle
		// Split the mesh up so each triangle has 3 unique vertices
		// When you color a mesh, you color the vertices, not the triangles. To make all triangles different colors, you need
		//	to individually color the vertices.
		int numTriangles = triangles.Length;
		Vector3[ ] splitVertices = new Vector3[numTriangles];
		Vector3[ ] splitNormals = new Vector3[numTriangles];

		// Make the new vertices based on the triangle count
		for (int i = 0; i < numTriangles; i++) {
			splitVertices[i] = vertices[triangles[i]];
			splitNormals[i] = normals[triangles[i]];
			triangles[i] = i;
		}

		vertices = splitVertices;
		normals = splitNormals;

		// Add colors to the vertices
		Color[ ] colors = new Color[vertices.Length];
		Color currentColor = new Color( );
		for (int i = 0; i < colors.Length; i++) {
			if (i % 3 == 0) {
				currentColor = Utils.RandColorInRange(Color, colorOffset);
			}

			colors[i] = currentColor;
		}

		*/

		// Initialize the mesh with all of the calculated values
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		// mesh.colors = colors;
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

	protected void UpdateVariables ( ) {
		// Reset and update component variables
		rigidBody.bodyType = RigidbodyType2D.Dynamic;
		rigidBody.gravityScale = 0;
		rigidBody.angularDrag = 0;
		rigidBody.drag = 0;

		trailRenderer.alignment = LineAlignment.TransformZ;
		trailRenderer.endWidth = 0;
		trailRenderer.material = new Material(trailMaterial);
		if (baseTrailColorOffMesh) {
			trailRenderer.startColor = trailRenderer.endColor = new Color(color.R / 2f, color.G / 2f, color.B / 2f);
		} else {
			trailRenderer.startColor = trailStartColor.Color;
			trailRenderer.endColor = trailEndColor.Color;
		}

		// Update all class variables
		MeshType = MeshType;
		LayerType = LayerType;
		Color = Color;
		Size = Size;
		SizeToMassRatio = SizeToMassRatio;
		ShowTrail = ShowTrail;
		IsLocked = IsLocked;
		DisableSolidColliders = DisableSolidColliders;
		EnableCollisions = EnableCollisions;

		// Regenerate the mesh;
		GenerateMesh( );
	}

	public void UpdateColor ( ) {
		// Create a new temporary material from the base material
		Material material = new Material(meshMaterial);
		// Set the temporary material color
		material.SetColor("_Color", Color);
		// Set the mesh material to this temporary materal
		// This is needed so objects can all have the same material but all be different colors
		meshRenderer.material = material;
	}

	private void UpdateTrail ( ) {
		if (ShowTrail) {
			lastTrailPositions.Insert(0, Utils.SetVectZ(Position + (rigidBody.velocity * Time.fixedDeltaTime), (int) LayerType.Trail));
			if (lastTrailPositions.Count > trailLength) {
				lastTrailPositions.RemoveAt(trailLength);
			}
		} else {
			if (lastTrailPositions.Count > 0) {
				lastTrailPositions.RemoveAt(lastTrailPositions.Count - 1);
			}
		}
		
		trailRenderer.positionCount = lastTrailPositions.Count;
		trailRenderer.SetPositions(lastTrailPositions.ToArray( ));
	}
}
