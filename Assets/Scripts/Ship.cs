using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityObject {
	[Header("--- Ship Class ---")]
	[SerializeField] private Collider2D launchingCollider;
	[Space]
	[SerializeField] private bool IsDead;

	protected new void Awake ( ) {
		base.Awake( );

		rigidBody.AddForce(Utils.RandNormVect2( ) * 0.5f, ForceMode2D.Impulse);
	}

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		if (!IsDead) {
			if (collision2D.transform.tag.Equals("Planet")) {
				Death( );
			}
		}
	}

	private void OnMouseOver ( ) {

	}

	private void Death ( ) {
		IsDead = true;

		levelManager.SpawnParticles(transform.position, Constants.SHIP_PARTICLE_COUNT);

		Destroy(gameObject);
	}

	protected override void GenerateMesh ( ) {
		Mesh mesh = new Mesh( );
		meshFilter.mesh = mesh;

		// Generate Vertices
		List<Vector3> verticesList = new List<Vector3>( );

		verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
		verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
		verticesList.Add(new Vector3(Size / 2, Size / 2, 0));
		verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

		Vector3[ ] vertices = verticesList.ToArray( );

		// Generate Triangles from vertices
		List<int> trianglesList = new List<int>( );

		trianglesList.Add(0);
		trianglesList.Add(1);
		trianglesList.Add(3);

		trianglesList.Add(1);
		trianglesList.Add(2);
		trianglesList.Add(3);

		int[ ] triangles = trianglesList.ToArray( );

		// Generate normals for all vertices
		Vector3[ ] normals = GenerateNormals(vertices);

		// Initialize the mesh with all of the calculated values
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;

		SetMeshColor(new Color(ColorR, ColorG, ColorB));

		// Edit the collider of the game object to take the shape of the mesh
		UpdatePolyCollider(vertices);
	}
}
