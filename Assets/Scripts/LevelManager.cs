using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	[Header("--- Level Manager Class ---")]
	[SerializeField] private GameObject particlePrefab;
	[Space]
	[SerializeField] private List<Planet> planets = new List<Planet>( );

	private void OnValidate ( ) {
		// Find all planet objects in the scene and add it to this list
		planets.Clear( );
		planets.AddRange(FindObjectsOfType<Planet>( ));

		/*
		foreach (Planet planet in planets) {
			planet.transform.SetParent(transform);
		}
		*/
	}

	public Vector2 CalculateGravityForce (GravityObject gravityObject) {
		Vector2 gravityForce = Vector2.zero;

		// For each of the planets, calculate their gravitation influence on the parameter object
		foreach (Planet planet in planets) {
			// Calculate the distance between the current planet and the object
			float distance = Vector2.Distance(planet.Position, gravityObject.Position);
			// Calculate the direction the planet is relative to the object
			Vector2 direction = (planet.Position - gravityObject.Position).normalized;

			// Calculate the gravitational force that the planet is applying onto the object
			float currForce = (Constants.G * gravityObject.Mass * planet.Mass) / Mathf.Pow(distance, 2);
			gravityForce += direction * currForce;
		}

		return gravityForce;
	}

	public List<Particle> SpawnParticles (Transform parent, int amount, Color color, float size = 0.05f, MeshType meshType = MeshType.Triangle, LayerType layerType = LayerType.Front, bool giveRandomForce = true, bool disableColliders = false) {
		List<Particle> particles = new List<Particle>( );

		// Create the particles with set values
		for (int i = 0; i < amount; i++) {
			// Instatiate a new particle object and initialize its values
			Particle particle = Instantiate(particlePrefab, parent.position, Quaternion.identity).GetComponent<Particle>( );
			particle.Initialize(parent, color, size, meshType, layerType, disableColliders);

			// Launch the particle in a random direction if told to do so
			if (giveRandomForce) {
				particle.GiveRandomForce(parent.GetComponent<Rigidbody2D>( ));
			}

			particles.Add(particle);
		}

		return particles;
	}
}
