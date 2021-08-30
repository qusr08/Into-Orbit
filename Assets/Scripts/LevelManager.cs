using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	[Header("--- Level Manager Class ---")]
	[SerializeField] private GameObject particlePrefab;
	[Space]
	[SerializeField] private List<Planet> planets = new List<Planet>( );

	private void OnValidate ( ) {
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

	public List<Particle> SpawnParticles (Transform parent, int amount, MeshType meshType, Color color, bool setParent = false, bool disableColliders = false) {
		List<Particle> particles = new List<Particle>( );

		for (int i = 0; i < amount; i++) {
			GameObject particle = particlePrefab;
			particle.GetComponent<Particle>( ).MeshType = meshType;
			particle.GetComponent<Particle>( ).SetColor(color);
			particle.GetComponent<PolygonCollider2D>( ).enabled = !disableColliders;

			particle = Instantiate(particle, parent.position, Quaternion.identity);
			if (setParent) {
				particle.transform.SetParent(parent);
			}

			particles.Add(particle.GetComponent<Particle>( ));
		}

		return particles;
	}
}
