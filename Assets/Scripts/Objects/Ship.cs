using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityObject {
	[Header("--- Ship Class ---")]
	[SerializeField] private Transform trail;
	[Header("--- Ship Constants ---")]
	[SerializeField] private int SHIP_PARTICLE_COUNT = 8;
	[SerializeField] private int LAUNCH_PARTICLE_COUNT = 10;
	[SerializeField] private float MAX_LAUNCH_DISTANCE = 5;

	private bool isLaunching; // Whether or not the player is currently launching the ship
	private List<Particle> launchingParticles; // All of the particles that make up the trail while launching

	private new void OnValidate ( ) {
		base.OnValidate( );

		// Make sure the trail is (basically) on the same layer as the ship. The +1 is to make sure it is behind the ship
		trail.position = Utils.SetZ(transform.position, ((int) LayerType) + 1);
	}

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		// If the ship collides with a planet, it should be destroyed
		if (collision2D.transform.tag.Equals("Planet")) {
			// Create particle pieces of the ship as it gets destroyed to make for a cool effect
			levelManager.SpawnParticles(transform, SHIP_PARTICLE_COUNT, colorHex, layerType: LayerType.Ship);

			// Destroy this ship gameobject
			Destroy(gameObject);
		}
	}

	private void OnMouseOver ( ) {
		// If the mouse is hovered over the ship and the left mouse button is pressed and it is not currently launching,
		// begin to launch the ship
		if (Input.GetMouseButtonDown(0) && !isLaunching) {
			isLaunching = true;

			// Create particles for the trail
			launchingParticles = levelManager.SpawnParticles(transform, LAUNCH_PARTICLE_COUNT, "EDEDED", size: 0.05f, meshType: MeshType.Circle, layerType: LayerType.ShipDetail, giveRandomForce: false, disableColliders: true);
			// Make sure to lock all of the particles because the ones for the trail should not move
			foreach (Particle particle in launchingParticles) {
				particle.IsLocked = true;
			}
		}
	}

	private void Update ( ) {
		// While the ship is being launched, update the positions of the particles on the trail
		if (isLaunching) {
			// Update the positions of the particles
			for (int i = 0; i < launchingParticles.Count; i++) {
				// Linearly interpolate between the mouse position and ship position
				float t = (float) i / (launchingParticles.Count - 1);
				Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 p2 = transform.position;

				// Make sure the distance that the mouse travels from the ship is not too far away
				// This causes the ship to be launched at a very fast speed
				float distance = Vector2.Distance(p1, p2);
				if (distance > MAX_LAUNCH_DISTANCE) {
					p1 *= MAX_LAUNCH_DISTANCE / distance;
				}

				// Set the position of the particle
				launchingParticles[i].Position = Utils.LinearInterpolation(t, p1, p2);
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				isLaunching = false;

				// Begin a small animation that moves all of the particles part of the trail towards the ship
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

		// While there are still particles left in the trail, move them closer to the ship
		while (launchingParticles.Count > 0) {
			for (int i = launchingParticles.Count - 1; i >= 0; i--) {
				// Have the particles speed up the more times this loop is called
				float iterationMod = iterations * 0.0005f;
				// Have the particles go faster towards the ship the further away they are
				float distanceMod = Vector3.Distance(launchingParticles[i].Position, transform.position) * iterationMod;
				// Modify the position of the particle
				launchingParticles[i].Position += direction * (iterationMod + distanceMod);

				// Check to see if the particle has passed the ship. If it has, destroy it
				float t = (launchingParticles[i].Position - p1).magnitude / distance;
				if (t >= 1) {
					Destroy(launchingParticles[i].gameObject);
					launchingParticles.RemoveAt(i);
				}
			}

			iterations++;

			yield return null;
		}

		// Clear the list of particles since they have all been destroyed by now
		launchingParticles.Clear( );

		// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
		IsLocked = false;
		rigidBody.AddForce(direction * distance / MAX_LAUNCH_DISTANCE, ForceMode2D.Impulse);

		yield return null;
	}
}
