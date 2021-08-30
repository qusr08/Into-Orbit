using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityObject {
	[Header("--- Ship Class ---")]
	[SerializeField] private Collider2D launchingCollider;

	private bool isLaunching;
	private List<Particle> launchingParticles;

	protected new void Awake ( ) {
		base.Awake( );

		// rigidBody.AddForce(Utils.RandNormVect2( ) * 0.5f, ForceMode2D.Impulse);
		IsLocked = true;
	}

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		if (collision2D.transform.tag.Equals("Planet")) {
			levelManager.SpawnParticles(transform, Constants.SHIP_PARTICLE_COUNT, MeshType.Triangle, color);

			Destroy(gameObject);
		}
	}

	private void OnMouseOver ( ) {
		if (Input.GetMouseButtonDown(0) && !isLaunching) {
			isLaunching = true;

			launchingParticles = levelManager.SpawnParticles(transform, 8, MeshType.Circle, Color.white, true, true);
			foreach (Particle particle in launchingParticles) {
				particle.IsLocked = true;
			}
		}
	}

	private void Update ( ) {
		if (isLaunching) {
			// Update the positions of the particles
			for (int i = 0; i < launchingParticles.Count; i++) {
				float t = (float) i / (launchingParticles.Count - 1);
				Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 p2 = transform.position;

				launchingParticles[i].Position = p1 + (t * (p2 - p1));
			}

			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1)) {
				isLaunching = false;

				for (int i = launchingParticles.Count - 1; i >= 0; i--) {
					Destroy(launchingParticles[i].gameObject);
				}

				launchingParticles.Clear( );
			}
		}
	}
}
