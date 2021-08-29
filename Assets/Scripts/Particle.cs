using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : GravityObject {
	protected void Start ( ) {
		rigidBody.AddForce(Utils.RandNormVect2( ) * 0.5f, ForceMode2D.Impulse);
	}

	protected override void GenerateMesh ( ) {
		Mesh mesh = new Mesh( );
		meshFilter.mesh = mesh;

		// Generate Vertices
		List<Vector3> verticesList = new List<Vector3>( );

		verticesList.Add(new Vector3(-Size / 2, -Size / 2, 0));
		verticesList.Add(new Vector3(-Size / 2, Size / 2, 0));
		verticesList.Add(new Vector3(Size / 2, -Size / 2, 0));

		Vector3[ ] vertices = verticesList.ToArray( );

		// Generate Triangles from vertices
		List<int> trianglesList = new List<int>( );

		trianglesList.Add(0);
		trianglesList.Add(1);
		trianglesList.Add(2);

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
