using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship : GravityObject {
	[Header("--- Ship Class ---")]
	[SerializeField] private CameraController cameraController;
	[Space]
	[SerializeField] private GameObject explosionParticleSystem;
	[SerializeField] private GameObject launchParticleSystem;
	[Space]
	[SerializeField] private Transform launchingIndicator;
	[SerializeField] private int launchDotCount = 20;
	[SerializeField] private int launchDotDensity = 5;

	private Wormhole wormhole;
	public Wormhole Wormhole {
		get {
			return wormhole;
		}

		set {
			wormhole = value;
			if (wormhole == null) {
				return;
			}

			// Set the rigidBody velocity to 0 because setting the position over time would just be better here
			refVelocity = rigidBody.velocity;
			rigidBody.velocity = Vector2.zero;
		}
	}

	private Vector2 refVelocity;
	private Vector2 lastMousePosition;

	// Whether or not the player is currently launching the ship
	private bool IsLaunching {
		get {
			return launchingIndicator.gameObject.activeSelf;
		}

		set {
			launchingIndicator.gameObject.SetActive(value);

			if (value) {
				// Create meshPieces for the trail
				launchingDots = levelManager.SpawnParticles(transform, launchDotCount, Utils.Hex2Color("EDEDED"),
					size: 0.1f, meshType: MeshType.Circle, layerType: LayerType.ShipDetail, giveRandomForce: false, showTrail: false, disableColliders: true);
				// Make sure to lock all of the meshPieces because the ones for the trail should not move
				foreach (MeshPiece meshPiece in launchingDots) {
					meshPiece.IsLocked = true;
				}
			} else {
				// Destroy all of the launching meshPieces
				for (int i = launchingDots.Count - 1; i >= 0; i--) {
					Destroy(launchingDots[i].gameObject);
					launchingDots.RemoveAt(i);
				}

				// Clear the list of meshPieces since they have all been destroyed by now
				launchingDots.Clear( );
			}
		}
	}
	private List<MeshPiece> launchingDots; // All of the meshPieces that make up the trail while launching

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		// If the ship collides with a planet, it should be destroyed
		if (collision2D.transform.tag.Equals("Planet")) {
			Death( );
		}
	}

	protected void OnMouseOver ( ) {
		// If the mouse is hovered over the ship and the left mouse button is pressed and it is not currently launching,
		// begin to launch the ship
		if (Input.GetMouseButtonDown(0) && !IsLaunching) {
			IsLaunching = true;
		}
	}

	protected void Start ( ) {
		lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

	protected void Update ( ) {
		// While the ship is being launched, update the positions of the meshPieces on the trail
		if (IsLaunching) {
			Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 p2 = Position;
			Vector2 direction = (p2 - p1).normalized;
			float distance = Utils.Limit(Vector2.Distance(p1, p2), 0, Constants.MAX_LAUNCH_DISTANCE);

			// If the mouse position was moved, then recalculate the positions of the launch meshPieces/indicator
			if (p1 != lastMousePosition) {
				PositionIndicator(p1, p2, distance);
				CreateDots(direction, distance);

				// Update the camera FOV based on the distance
				cameraController.SetTargetFOV(Utils.Map(distance, 0, Constants.MAX_LAUNCH_DISTANCE, Constants.MIN_CAMERA_FOV, Constants.MAX_CAMERA_FOV));
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				IsLaunching = false;

				if (distance >= Constants.MIN_LAUNCH_DISTANCE) {
					// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
					IsLocked = false;
					rigidBody.AddForce(direction * (distance / Constants.MAX_LAUNCH_DISTANCE), ForceMode2D.Impulse);

					// Reset camera FOV
					cameraController.ResetFOV( );

					// Spawn launch explosion meshPieces
					float angleModifier = 90 + launchParticleSystem.transform.rotation.eulerAngles.z;
					Instantiate(launchParticleSystem, Position, Quaternion.Euler(new Vector3(0, 0, angleModifier + Utils.GetAngleBetween(Position, direction))));
				}
			}

			// Update the last mouse position
			lastMousePosition = p1;
		}
	}

	protected new void FixedUpdate ( ) {
		if (Wormhole != null) {
			// Make the ship smoothly transition to the center of the wormhole
			Position = Vector2.SmoothDamp(Position, Wormhole.Position, ref refVelocity, 0.5f);

			// If the ship has reached the center and is stopped, then the player has won the level
			if (Utils.CloseEnough(Position, Wormhole.Position)) {
				Position = Wormhole.Position;

				Wormhole = null;
				IsLocked = true;
			}
		} else {
			base.FixedUpdate( );
		}
	}

	private void PositionIndicator (Vector2 p1, Vector2 p2, float distance) {
		// Set indicator to the midpoint between the mouse and the ship
		Vector3 indicatorPosition = new Vector2(p1.x + p2.x, p1.y + p2.y) / 2;
		launchingIndicator.localPosition = Utils.SetVectZ(Utils.LimitVect3(Position, indicatorPosition, 0, Constants.MAX_LAUNCH_DISTANCE / 2), 1);

		// Calculate rotation angle of this transform relative to the indicator
		launchingIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, Utils.GetAngleBetween(p2, p1)));

		// Set the size of the indicator based on the distance of the mouse from the ship
		float height = launchingIndicator.GetComponent<SpriteRenderer>( ).size.y;
		launchingIndicator.GetComponent<SpriteRenderer>( ).size = new Vector2(distance, height);
	}

	private void CreateDots (Vector2 direction, float distance) {
		// Get the current force that would be applied to the ship if it was launched right now
		Vector2 currForce = direction * (distance / Constants.MAX_LAUNCH_DISTANCE);
		Vector2 currPosition = Position;

		// Calculate the initial velocity of the ship
		// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
		Vector2 currVelocity = currForce / Mass;

		// Calculate the positions of each of the meshPieces along the ships path
		for (int i = 0; i < launchingDots.Count; i++) {
			// Make sure the meshPiece that is being positioned is active
			launchingDots[i].gameObject.SetActive(true);

			bool hitPlanet = false;

			// Run multiple iterations of this calculation, simulating each frame of the physics update
			for (int j = 0; j < launchDotDensity; j++) {
				// Calculate the gravity that the ship will experience at the current position
				Vector2 gravityForce = levelManager.CalculateGravityForce(currPosition, Mass);
				// Calculate the acceleration due to the gravity force
				Vector2 gravityAcc = gravityForce / Mass;

				// Increment the velocity by the acceleration
				currVelocity += gravityAcc * Time.fixedDeltaTime;
				// Increment the position by the velocity
				currPosition += currVelocity * Time.fixedDeltaTime;

				// My Forum Post: https://forum.unity.com/threads/need-help-predicting-the-path-of-an-object-in-a-2d-gravity-simulation.1170098/

				// If the current position is on a planet, do not draw the rest of the meshPieces to show that the ship will crash into the planet
				RaycastHit2D[ ] hits = Physics2D.RaycastAll(Utils.SetVectZ(currPosition, -10), Vector3.forward);
				for (int k = 0; k < hits.Length; k++) {
					if (hits[k].transform.tag.Equals("Planet")) {
						// Disable all meshPieces later in the list
						for (; i < launchingDots.Count; i++) {
							launchingDots[i].gameObject.SetActive(false);
						}

						// Break out of the for loop and stop calculating the position for the rest of the meshPieces
						hitPlanet = true;
						j = launchDotDensity;
						break;
					}
				}
			}

			// Once a certain amount of iterations have been done, set the meshPiece to the current position
			// If there was no planet collision, then just set the position of the current meshPiece and move on to the next one
			if (!hitPlanet) {
				launchingDots[i].Position = currPosition;
			}
		}
	}

	private void Death ( ) {
		// Create meshPiece pieces of the ship as it gets destroyed to make for a cool effect
		levelManager.SpawnParticles(transform, Constants.CRASH_PARTICLE_COUNT, Color, layerType: LayerType.Ship);

		// Spawn explosion
		Instantiate(explosionParticleSystem, Utils.SetVectZ(transform.position, (int) LayerType.Front), Quaternion.identity);

		// Destroy this ship gameobject
		Destroy(gameObject);
	}
}
