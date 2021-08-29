using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GravityObject {
	private float rotationSpeed;

	protected new void Awake ( ) {
		base.Awake( );

		rotationSpeed = Utils.RandFloat(-10, 10);
	}

	protected new void FixedUpdate ( ) {
		base.FixedUpdate( );

		transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
	}

	protected override void GenerateMesh ( ) {
		// https://stackoverflow.com/questions/50606756/creating-a-2d-circular-mesh-in-unity

		Mesh mesh = new Mesh( );
		meshFilter.mesh = mesh;

		// Generate Vertices
		List<Vector3> verticesList = new List<Vector3>( );

		float x;
		float y;
		for (int i = 0; i < Constants.CIRCLE_PRECISION; i++) {
			x = Size * Mathf.Sin((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);
			y = Size * Mathf.Cos((2 * Mathf.PI * i) / Constants.CIRCLE_PRECISION);

			verticesList.Add(new Vector3(x, y, 0f));
		}

		Vector3[ ] vertices = verticesList.ToArray( );

		// Generate Triangles from vertices
		List<int> trianglesList = new List<int>( );

		for (int i = 0; i < (Constants.CIRCLE_PRECISION - 2); i++) {
			trianglesList.Add(0);
			trianglesList.Add(i + 1);
			trianglesList.Add(i + 2);
		}

		int[ ] triangles = trianglesList.ToArray( );

		// Generate normals for all vertices
		Vector3[] normals = GenerateNormals(vertices);

		// Initialize the mesh with all of the calculated values
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;

		SetMeshColor(new Color(ColorR, ColorG, ColorB));

		// Edit the collider of the game object to take the shape of the mesh
		UpdatePolyCollider(vertices);
	}
}
