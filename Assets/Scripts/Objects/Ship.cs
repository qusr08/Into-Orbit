using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityObject {
	private const int CRASH_PARTICLE_COUNT = 8;
	private const int LAUNCH_PARTICLE_COUNT = 6;
	private const int LAUNCH_PARTICLE_ITERATION_INTERVAL = 4;
	private const float MAX_LAUNCH_DISTANCE = 5;

	[Header("--- Ship Class ---")]
	[SerializeField] private GameObject explosionParticleSystem;
	[SerializeField] private GameObject launchParticleSystem;
	[Space]
	[SerializeField] private Transform launchingIndicator;

	// Whether or not the player is currently launching the ship
	private bool IsLaunching {
		get {
			return launchingIndicator.gameObject.activeSelf;
		}

		set {
			launchingIndicator.gameObject.SetActive(value);

			if (value) {
				// Create particles for the trail
				launchingParticles = levelManager.SpawnParticles(transform, LAUNCH_PARTICLE_COUNT, Utils.Hex2Color("EDEDED"),
					size: 0.1f, meshType: MeshType.Circle, layerType: LayerType.ShipDetail, giveRandomForce: false, showTrail: false, disableColliders: true);
				// Make sure to lock all of the particles because the ones for the trail should not move
				foreach (Particle particle in launchingParticles) {
					particle.IsLocked = true;
				}
			} else {
				// Destroy all of the launching particles
				for (int i = launchingParticles.Count - 1; i >= 0; i--) {
					Destroy(launchingParticles[i].gameObject);
					launchingParticles.RemoveAt(i);
				}

				// Clear the list of particles since they have all been destroyed by now
				launchingParticles.Clear( );
			}
		}
	}
	private List<Particle> launchingParticles; // All of the particles that make up the trail while launching

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		// If the ship collides with a planet, it should be destroyed
		if (collision2D.transform.tag.Equals("Planet")) {
			Death( );
		}
	}

	private void OnMouseOver ( ) {
		// If the mouse is hovered over the ship and the left mouse button is pressed and it is not currently launching,
		// begin to launch the ship
		if (Input.GetMouseButtonDown(0) && !IsLaunching) {
			IsLaunching = true;
		}
	}

	private void Update ( ) {
		// While the ship is being launched, update the positions of the particles on the trail
		if (IsLaunching) {
			Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 p2 = Position;

			// Set indicator to the midpoint between the mouse and the ship
			Vector3 indicatorPosition = new Vector2(p1.x + p2.x, p1.y + p2.y) / 2;
			launchingIndicator.localPosition = Utils.SetZ(Utils.LimitVector3(Position, indicatorPosition, 0, MAX_LAUNCH_DISTANCE / 2), 1);

			// Calculate rotation angle of this transform relative to the indicator
			launchingIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, Utils.GetRotation2D(p2, p1)));

			// Set the size of the indicator based on the distance of the mouse from the ship
			float height = launchingIndicator.GetComponent<SpriteRenderer>( ).size.y;
			float width = Utils.Limit(Vector2.Distance(p1, p2), 0, MAX_LAUNCH_DISTANCE);
			launchingIndicator.GetComponent<SpriteRenderer>( ).size = new Vector2(width, height);

			// Get the current force that would be applied to the ship if it was launched right now
			Vector2 direction = (p2 - p1).normalized;
			Vector2 force = direction * (width / MAX_LAUNCH_DISTANCE);
			Vector2 currPosition = Position;

			// Calculate the initial velocity
			// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
			Vector2 v0 = force / Mass;

			// Helpful : https://stackoverflow.com/questions/55997293/change-velocity-of-rigidbody-using-addforce

			// Calculate the positions of each of the particles along the ships path
			for (int i = 0; i < launchingParticles.Count; i++) {
				for (int j = 0; j < LAUNCH_PARTICLE_ITERATION_INTERVAL; j++) {
					// Calculate the gravity that the ship will experience at the current position
					Vector2 gravityForce = levelManager.CalculateGravityForce(currPosition, Mass);
					// Calculate the new velocity of the ship at that position
					Vector2 v = ((gravityForce * Time.fixedDeltaTime) / Mass) + v0;

					// Increment the current position that is being checked based on the velocity
					currPosition += ((v + v0) / 2) * Time.fixedDeltaTime;

					// Set the current velocity as the new starting velocity for the next iteration
					v0 = v;
				}

				// Once a certain amount of iterations have been done, set the particle to that position
				launchingParticles[i].Position = currPosition;
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				IsLaunching = false;

				// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
				IsLocked = false;
				rigidBody.AddForce(direction * (width / MAX_LAUNCH_DISTANCE), ForceMode2D.Impulse);

				// Spawn launch explosion particles
				float angleModifier = 90 + launchParticleSystem.transform.rotation.eulerAngles.z;
				Instantiate(launchParticleSystem, transform.position, Quaternion.Euler(new Vector3(0, 0, angleModifier + Utils.GetRotation2D(Position, direction))));
			}
		}
	}

	private void Death ( ) {
		// Create particle pieces of the ship as it gets destroyed to make for a cool effect
		levelManager.SpawnParticles(transform, CRASH_PARTICLE_COUNT, Color, layerType: LayerType.Ship);

		// Spawn explosion
		Instantiate(explosionParticleSystem, Utils.SetZ(transform.position, (int) LayerType.Front), Quaternion.identity);

		// Destroy this ship gameobject
		Destroy(gameObject);
	}
}
