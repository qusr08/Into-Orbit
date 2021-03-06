using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	[Separator("Level Manager")]
	[SerializeField] private GameObject meshGravityParticlePrefab;
	[SerializeField] private GameObject meshStationaryParticlePrefab;
	[Space]
	[SerializeField] private GameObject explosionParticleSystemPrefab;
	[SerializeField] private GameObject collisionParticleSystemPrefab;
	[SerializeField] private GameObject teleportParticleSystemPrefab;
	[SerializeField] private GameObject launchParticleSystemPrefab;
	[SerializeField] private GameObject boostRechargeParticleSystemPrefab;
	[SerializeField] private GameObject buttonParticleSystemPrefab;
	[Space]
	[SerializeField] private List<Planet> planets = new List<Planet>( );
	[SerializeField] private List<Wormhole> wormholes = new List<Wormhole>( );
	[SerializeField] private List<Teleportal> teleportals = new List<Teleportal>( );
	[SerializeField] private List<Asteroid> asteroids = new List<Asteroid>( );
	[SerializeField] private List<ButtonObject> buttons = new List<ButtonObject>( );
	[SerializeField] private List<GateObject> gates = new List<GateObject>( );
	[SerializeField] private List<BoostRechargeObject> boostRecharges = new List<BoostRechargeObject>( );
	[Space]
	[SerializeField] private float G = 0.75f;
	[SerializeField] public Vector2 CenterOfMass;

	public int ActiveButtonCount;
	public bool GatesOpen {
		get {
			return ActiveButtonCount == buttons.Count;
		}
	}

	protected void OnValidate ( ) {
		planets.Clear( );
		planets.AddRange(FindObjectsOfType<Planet>( ));

		wormholes.Clear( );
		wormholes.AddRange(FindObjectsOfType<Wormhole>( ));

		teleportals.Clear( );
		teleportals.AddRange(FindObjectsOfType<Teleportal>( ));

		asteroids.Clear( );
		asteroids.AddRange(FindObjectsOfType<Asteroid>( ));

		buttons.Clear( );
		buttons.AddRange(FindObjectsOfType<ButtonObject>( ));

		gates.Clear( );
		gates.AddRange(FindObjectsOfType<GateObject>( ));

		boostRecharges.Clear( );
		boostRecharges.AddRange(FindObjectsOfType<BoostRechargeObject>( ));
	}

	protected void Start ( ) {
		Vector2 totalPosition = Vector2.zero;
		foreach (Planet planet in planets) {
			totalPosition += planet.Position;
		}
		CenterOfMass = totalPosition / (planets.Count + 1f);
	}

	protected void Update ( ) {
		if (GatesOpen) {
			foreach (GateObject gate in gates) {
				gate.IsOpen = true;
			}
		}
	}

	public Vector2 CalculateGravityForce (GravityObject gravityObject, List<MeshObject> onlyParents = null) {
		return CalculateGravityForce(gravityObject.Position, gravityObject.Mass, onlyParents: onlyParents);
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

	public List<MeshParticle> SpawnGravityParticles (Vector2 position, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.RightTriangle, LayerType layerType = LayerType.Front, bool giveRandomForce = true, bool showTrail = true) {
		List<MeshParticle> meshParticles = new List<MeshParticle>( );

		// Create the meshPieces with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new meshPiece object and initialize its values
			MeshParticle meshParticle = Instantiate(meshGravityParticlePrefab, position, Quaternion.identity).GetComponent<MeshParticle>( );
			meshParticle.Initialize(color, size, meshType, layerType, showTrail, false);

			// Launch the meshPiece in a random direction if told to do so
			if (giveRandomForce) {
				meshParticle.GiveRandomForce( );
			}

			meshParticles.Add(meshParticle);
		}

		return meshParticles;
	}

	public List<MeshParticle> SpawnStationaryParticles (Vector2 position, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.Circle, LayerType layerType = LayerType.Front) {
		List<MeshParticle> meshParticles = new List<MeshParticle>( );

		// Create the meshPieces with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new meshPiece object and initialize its values
			MeshParticle meshParticle = Instantiate(meshStationaryParticlePrefab, position, Quaternion.identity).GetComponent<MeshParticle>( );
			meshParticle.Initialize(color, size, meshType, layerType, false, true);

			meshParticles.Add(meshParticle);
		}

		return meshParticles;
	}

	public GameObject SpawnParticleSystem (ParticleSystemType systemType, Vector2 position, float angle = 0) {
		GameObject system = null;

		switch (systemType) {
			case ParticleSystemType.Collision:
				system = Instantiate(collisionParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
			case ParticleSystemType.Launch:
				system = Instantiate(launchParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
			case ParticleSystemType.Explosion:
				system = Instantiate(explosionParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
			case ParticleSystemType.Teleport:
				system = Instantiate(teleportParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
			case ParticleSystemType.BoostRecharge:
				system = Instantiate(boostRechargeParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
			case ParticleSystemType.Button:
				system = Instantiate(buttonParticleSystemPrefab, position, Quaternion.Euler(0, 0, angle));
				break;
		}

		return system;
	}

	public GameObject SpawnParticleSystem (ParticleSystemType systemType, Transform parent, float angle = 0) {
		GameObject system = SpawnParticleSystem(systemType, parent.position, angle: angle);
		system.transform.SetParent(parent);

		return system;
	}
}
