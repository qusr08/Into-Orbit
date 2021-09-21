using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship : GravityObject {
	private const int CRASH_PARTICLE_COUNT = 8;
	private const float MAX_LAUNCH_DISTANCE = 5;

	[Header("--- Ship Class ---")]
	[SerializeField] private GameObject explosionParticleSystem;
	[SerializeField] private GameObject launchParticleSystem;
	[Space]
	[SerializeField] private Transform launchingIndicator;
	[SerializeField] private int launchParticleCount = 10;
	[SerializeField] private float launchParticleDensity = 5;

	private Vector2 lastMousePosition;

	// Whether or not the player is currently launching the ship
	private bool IsLaunching {
		get {
			return launchingIndicator.gameObject.activeSelf;
		}

		set {
			launchingIndicator.gameObject.SetActive(value);

			if (value) {
				// Create particles for the trail
				launchingParticles = levelManager.SpawnParticles(transform, launchParticleCount, Utils.Hex2Color("EDEDED"),
					size: 0.1f, meshType: MeshType.Circle, layerType: LayerType.ShipDetail, giveRandomForce: false, showTrail: false, disableColliders: true);
				// Make sure to lock all of the particles because the ones for the trail should not move
				foreach (Particle particle in launchingParticles) {
					particle.IsLocked = true;
				}
			} else {
				/*
				// Destroy all of the launching particles
				for (int i = launchingParticles.Count - 1; i >= 0; i--) {
					Destroy(launchingParticles[i].gameObject);
					launchingParticles.RemoveAt(i);
				}

				// Clear the list of particles since they have all been destroyed by now
				launchingParticles.Clear( );
				*/
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

	private void Start ( ) {
		lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

	private void Update ( ) {
		// While the ship is being launched, update the positions of the particles on the trail
		if (IsLaunching) {
			Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 p2 = Position;
			Vector2 direction = (p2 - p1).normalized;
			float distance = Utils.Limit(Vector2.Distance(p1, p2), 0, MAX_LAUNCH_DISTANCE);

			// If the mouse position was moved, then recalculate the positions of the launch particles/indicator
			if (p1 != lastMousePosition) {
				// Set indicator to the midpoint between the mouse and the ship
				Vector3 indicatorPosition = new Vector2(p1.x + p2.x, p1.y + p2.y) / 2;
				launchingIndicator.localPosition = Utils.SetZ(Utils.LimitVector3(Position, indicatorPosition, 0, MAX_LAUNCH_DISTANCE / 2), 1);

				// Calculate rotation angle of this transform relative to the indicator
				launchingIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, Utils.GetRotation2D(p2, p1)));

				// Set the size of the indicator based on the distance of the mouse from the ship
				float height = launchingIndicator.GetComponent<SpriteRenderer>( ).size.y;
				launchingIndicator.GetComponent<SpriteRenderer>( ).size = new Vector2(distance, height);

				// ------------------------------------------------------------------------------------------

				// Get the current force that would be applied to the ship if it was launched right now
				Vector2 currForce = direction * (distance / MAX_LAUNCH_DISTANCE);
				Vector2 currPosition = Position;

				// Calculate the initial velocity of the ship
				// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
				Vector2 currVelocity = currForce / Mass;

				// Calculate the positions of each of the particles along the ships path
				for (int i = 0; i < launchingParticles.Count; i++) {
					// Make sure the particle that is being positioned is active
					launchingParticles[i].gameObject.SetActive(true);

					// Run multiple iterations of this calculation, simulating each frame of the physics update
					for (int j = 0; j < launchParticleDensity; j++) {
						// Calculate the gravity that the ship will experience at the current position
						Vector2 gravityForce = levelManager.CalculateGravityForce(currPosition, Mass);
						// Calculate the acceleration due to the gravity force
						Vector2 gravityAcc = gravityForce / Mass;

						// Increment the velocity by the acceleration
						currVelocity += gravityAcc * Time.fixedDeltaTime;
						// Increment the position by the velocity
						currPosition += currVelocity * Time.fixedDeltaTime;

						// My Forum Post: https://forum.unity.com/threads/need-help-predicting-the-path-of-an-object-in-a-2d-gravity-simulation.1170098/
					}

					// Once a certain amount of iterations have been done, set the particle to the current position
					// If the current position is on a planet, do not draw the rest of the particles to show that the ship will crash into
					//	the planet
					RaycastHit2D hit = Physics2D.Raycast(Utils.SetZ(currPosition, -10), Vector3.forward);
					if (hit && hit.transform.tag.Equals("Planet")) {
						// Disable all particles later in the list
						for (int j = i; j < launchingParticles.Count; j++) {
							launchingParticles[j].gameObject.SetActive(false);
						}

						// Break out of the for loop and stop calculating the position for the rest of the particles
						i = launchingParticles.Count;
					} else {
						// If there was no planet collision, then just set the position of the current particle and move on to the next one
						launchingParticles[i].Position = currPosition;
					}
				}
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				IsLaunching = false;

				// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
				IsLocked = false;
				rigidBody.AddForce(direction * (distance / MAX_LAUNCH_DISTANCE), ForceMode2D.Impulse);

				// Spawn launch explosion particles
				float angleModifier = 90 + launchParticleSystem.transform.rotation.eulerAngles.z;
				Instantiate(launchParticleSystem, transform.position, Quaternion.Euler(new Vector3(0, 0, angleModifier + Utils.GetRotation2D(Position, direction))));
			}

			// Update the last mouse position
			lastMousePosition = p1;
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
