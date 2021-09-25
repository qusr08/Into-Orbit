using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	[Header("--- Level Manager Class ---")]
	[SerializeField] private GameObject particlePrefab;
	[Space]
	[SerializeField] private List<Planet> planets = new List<Planet>( );
	[Header("--- Level Manager Constants ---")]
	[SerializeField] private float G = 0.75f;

	private void OnValidate ( ) {
		// Find all planet objects in the scene and add it to this list
		planets.Clear( );
		planets.AddRange(FindObjectsOfType<Planet>( ));
	}

	public Vector2 CalculateGravityForce (GravityObject gravityObject, List<MeshObject> onlyParents = null) {
		return CalculateGravityForce(gravityObject.Position, gravityObject.Mass, onlyParents);
	}

	public Vector2 CalculateGravityForce (Vector2 position, float mass, List<MeshObject> onlyParents = null) {
		Vector2 gravityForce = Vector2.zero;

		if (onlyParents != null) {
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

	public List<Particle> SpawnParticles (Transform parent, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.Triangle, LayerType layerType = LayerType.Front, bool giveRandomForce = true, bool showTrail = true, bool disableColliders = false) {
		List<Particle> particles = new List<Particle>( );

		// Create the particles with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new particle object and initialize its values
			Particle particle = Instantiate(particlePrefab, parent.position, Quaternion.identity).GetComponent<Particle>( );
			particle.Initialize(parent, color, size, meshType, layerType, showTrail, disableColliders);

			// Launch the particle in a random direction if told to do so
			if (giveRandomForce) {
				particle.GiveRandomForce(parent.GetComponent<Rigidbody2D>( ));
			}

			particles.Add(particle);
		}

		return particles;
	}
}
