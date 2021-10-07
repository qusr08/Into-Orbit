using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {
	[Separator("Level Manager")]
	[SerializeField] private GameObject meshGravityPiecePrefab;
	[SerializeField] private GameObject meshStationaryPiecePrefab;
	[Space]
	[SerializeField] private Ship ship;
	[SerializeField] private List<Planet> planets = new List<Planet>( );
	[SerializeField] private List<Wormhole> wormholes = new List<Wormhole>( );
	[SerializeField] private List<Teleportal> teleportals = new List<Teleportal>( );
	[SerializeField] private List<Asteroid> asteroids = new List<Asteroid>( );
	[Separator("Constants")]
	[SerializeField] private float G = 0.75f;

	private void OnValidate ( ) {
		ship = FindObjectOfType<Ship>( );

		planets.Clear( );
		planets.AddRange(FindObjectsOfType<Planet>( ));

		wormholes.Clear( );
		wormholes.AddRange(FindObjectsOfType<Wormhole>( ));

		teleportals.Clear( );
		teleportals.AddRange(FindObjectsOfType<Teleportal>( ));

		asteroids.Clear( );
		asteroids.AddRange(FindObjectsOfType<Asteroid>( ));
	}

	public Vector2 CalculateGravityForce (GravityObject gravityObject, List<MeshObject> onlyParents = null) {
		return CalculateGravityForce(gravityObject.Position, gravityObject.Mass, onlyParents);
	}

	public Vector2 CalculateGravityForce (Vector2 position, float mass, List<MeshObject> onlyParents = null) {
		Vector2 gravityForce = Vector2.zero;

		if (onlyParents != null && onlyParents.Count > 0) {
			// For each of the objects in the list, calculate their gravitational infulence on the parameter object
			foreach (MeshObject spaceObject in onlyParents) {
				gravityForce += CalculateGravityForce(position, mass, spaceObject);
			}
		} else {
			// For each of the planets, calculate their gravitational influence on the parameter object
			foreach (Planet planet in planets) {
				gravityForce += CalculateGravityForce(position, mass, planet);
			}
		}

		return gravityForce;
	}

	public Vector2 CalculateGravityForce (Vector2 position, float mass, MeshObject spaceObject) {
		// Calculate the distance between the current planet and the object
		float distance = Vector2.Distance(spaceObject.Position, position);
		// Calculate the direction the planet is relative to the object
		Vector2 direction = (spaceObject.Position - position).normalized;

		// Calculate the gravitational force that the planet is applying onto the object
		float force = (G * mass * spaceObject.Mass) / Mathf.Pow(distance, 2);

		return direction * force;
	}

	public List<MeshPiece> SpawnGravityPieces (Transform parent, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.Triangle, LayerType layerType = LayerType.Front, bool giveRandomForce = true, bool showTrail = true) {
		List<MeshPiece> meshPieces = new List<MeshPiece>( );

		// Create the meshPieces with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new meshPiece object and initialize its values
			MeshPiece meshPiece = Instantiate(meshGravityPiecePrefab, parent.position, Quaternion.identity).GetComponent<MeshPiece>( );
			meshPiece.Initialize(parent, color, size, meshType, layerType, showTrail, false);

			// Launch the meshPiece in a random direction if told to do so
			if (giveRandomForce) {
				meshPiece.GiveRandomForce(parent.GetComponent<Rigidbody2D>( ));
			}

			meshPieces.Add(meshPiece);
		}

		return meshPieces;
	}

	public List<MeshPiece> SpawnStationaryPieces (Transform parent, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.Circle, LayerType layerType = LayerType.Front) {
		List<MeshPiece> meshPieces = new List<MeshPiece>( );

		// Create the meshPieces with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new meshPiece object and initialize its values
			MeshPiece meshPiece = Instantiate(meshStationaryPiecePrefab, parent.position, Quaternion.identity).GetComponent<MeshPiece>( );
			meshPiece.Initialize(parent, color, size, meshType, layerType, false, true);

			meshPieces.Add(meshPiece);
		}

		return meshPieces;
	}
}
