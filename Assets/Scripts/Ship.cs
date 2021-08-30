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
			levelManager.SpawnParticles(transform, Constants.SHIP_PARTICLE_COUNT, color);

			Destroy(gameObject);
		}
	}

	private void OnMouseOver ( ) {
		if (Input.GetMouseButtonDown(0) && !isLaunching) {
			isLaunching = true;

			launchingParticles = levelManager.SpawnParticles(transform, Constants.LAUNCH_PARTICLE_COUNT, Color.white, size: 0.05f, meshType: MeshType.Circle, giveRandomForce: false, disableColliders: true);
			foreach (Particle particle in launchingParticles) {
				particle.IsLocked = true;
			}
		}
	}

	private void Update ( ) {
		if (isLaunching) {
			// Update the positions of the particles
			for (int i = 0; i < launchingParticles.Count; i++) {
				// Linearly interpolate between the mouse position and ship position
				float t = (float) i / (launchingParticles.Count - 1);
				Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 p2 = transform.position;

				launchingParticles[i].Position = Utils.LinearInterpolation(t, p1, p2);
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				isLaunching = false;

				StartCoroutine(LaunchParticlesAnimation(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
			}
		}
	}

	private IEnumerator LaunchParticlesAnimation (Vector3 lastMousePosition) {
		int iterations = 1;
		Vector2 p1 = lastMousePosition;
		Vector2 p2 = transform.position;
		Vector2 direction = (p2 - p1).normalized;
		float distance = Vector3.Distance(p1, p2);

		while (launchingParticles.Count > 0) {
			for (int i = launchingParticles.Count - 1; i >= 0; i--) {
				float iterationMod = iterations * 0.0005f;
				float distanceMod = Vector3.Distance(launchingParticles[i].Position, transform.position) * iterationMod;
				launchingParticles[i].Position += direction * (iterationMod + distanceMod);

				float t = (launchingParticles[i].Position - p1).magnitude / distance;
				if (t >= 1) {
					Destroy(launchingParticles[i].gameObject);
					launchingParticles.RemoveAt(i);
				}
			}

			iterations++;

			yield return null;
		}

		launchingParticles.Clear( );

		IsLocked = false;
		rigidBody.AddForce(direction * distance / 4, ForceMode2D.Impulse);

		yield return null;
	}
}
