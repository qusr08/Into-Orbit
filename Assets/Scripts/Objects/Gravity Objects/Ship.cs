using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship : GravityObject {
	[Separator("Ship")]
	[SerializeField] private Transform launchingIndicator;
	[SerializeField] private int launchDotCount = 20;
	[SerializeField] private int launchDotDensity = 5;

	private Vector2 lastMousePosition;

	// Whether or not the player is currently launching the ship
	private bool canLaunch;
	private bool IsLaunching {
		get {
			return launchingIndicator.gameObject.activeSelf;
		}

		set {
			launchingIndicator.gameObject.SetActive(value);

			Time.timeScale = (value && !IsLocked) ? Constants.LAUNCHING_TIMESCALE : 1f;
			Time.fixedDeltaTime = Constants.DEFAULT_FIXED_DELTA_TIME * Time.timeScale;

			if (value) {
				// Create meshPieces for the trail
				launchingDots = levelManager.SpawnStationaryParticles(Position, launchDotCount, Utils.Hex2Color("EDEDED"), size: 0.1f, layerType: LayerType.ShipDetail);
				// Make sure to lock all of the meshPieces because the ones for the trail should not move
				foreach (MeshParticle meshPiece in launchingDots) {
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
	private List<MeshParticle> launchingDots = new List<MeshParticle>( ); // All of the meshPieces that make up the trail while launching

	protected void OnMouseOver ( ) {
		// If the mouse is hovered over the ship and the left mouse button is pressed and it is not currently launching,
		// begin to launch the ship
		if (canLaunch && Input.GetMouseButtonDown(0) && !IsLaunching && !uiManager.IsPaused && uiManager.IsPlaying) {
			IsLaunching = true;
			canLaunch = false;
		}
	} 

	protected new void Start ( ) {
		base.Start( );

		lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		ResetLaunch( );
	}

	protected new void Update ( ) {
		base.Update( );

		// While the ship is being launched, update the positions of the meshPieces on the trail
		if (IsLaunching) {
			Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 p2 = Position;
			Vector2 launchDirection = (p2 - p1).normalized;
			float launchMagnitude = Utils.Limit(Vector2.Distance(p1, p2), 0, Constants.MAX_LAUNCH_DISTANCE);

			// If the mouse position was moved, then recalculate the positions of the launch meshPieces/indicator
			if (p1 != lastMousePosition) {
				PositionIndicator(p1, p2, launchMagnitude);
				CreateDots(launchDirection, launchMagnitude);

				// Update the camera FOV based on the distance
				cameraController.SetTargetFOV(Utils.Map(launchMagnitude, 0, Constants.MAX_LAUNCH_DISTANCE, Constants.MIN_CAMERA_FOV, Constants.MAX_CAMERA_FOV));
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				IsLaunching = false;

				if (launchMagnitude >= Constants.MIN_LAUNCH_DISTANCE) {
					// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
					IsLocked = false;

					rigidBody.velocity = Vector2.zero;
					rigidBody.AddForce(launchDirection * (launchMagnitude / Constants.MAX_LAUNCH_DISTANCE), ForceMode2D.Impulse);

					// Reset camera FOV
					cameraController.ResetFOV( );

					// Spawn launch explosion meshPieces
					levelManager.SpawnParticleSystem(ParticleSystemType.Launch, Position, angle: 90 + Utils.GetAngleBetween(Position, launchDirection));
				} else {
					canLaunch = true;
				}
			}

			// Update the last mouse position
			lastMousePosition = p1;
		} else {
			transform.rotation = Quaternion.Euler(0, 0, Utils.GetAngleBetween(Position, Position + rigidBody.velocity) - 90);
		}
	}

	private void PositionIndicator (Vector2 p1, Vector2 p2, float distance) {
		// Update the direction the ship is facing
		transform.rotation = Quaternion.Euler(0, 0, Utils.GetAngleBetween(p1, p2) - 90);

		// Set indicator to the midpoint between the mouse and the ship
		Vector3 indicatorPosition = new Vector2(p1.x + p2.x, p1.y + p2.y) / 2;
		launchingIndicator.position = Utils.SetVectZ(Utils.LimitVect3(Position, indicatorPosition, 0, Constants.MAX_LAUNCH_DISTANCE / 2), (int) LayerType + 1);

		// Calculate rotation angle of this transform relative to the indicator
		launchingIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, Utils.GetAngleBetween(p2, p1)));

		// Set the size of the indicator based on the distance of the mouse from the ship
		float height = launchingIndicator.GetComponent<SpriteRenderer>( ).size.y;
		launchingIndicator.GetComponent<SpriteRenderer>( ).size = new Vector2(distance, height);
	}

	private void CreateDots (Vector2 direction, float magnitude) {
		// Get the current force that would be applied to the ship if it was launched right now
		Vector2 currForce = direction * (magnitude / Constants.MAX_LAUNCH_DISTANCE);
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
				currVelocity += gravityAcc * Constants.DEFAULT_FIXED_DELTA_TIME;
				// Increment the position by the velocity
				currPosition += currVelocity * Constants.DEFAULT_FIXED_DELTA_TIME;

				// My Forum Post: https://forum.unity.com/threads/need-help-predicting-the-path-of-an-object-in-a-2d-gravity-simulation.1170098/

				// If the current position is on a planet, do not draw the rest of the meshPieces to show that the ship will crash into the planet
				RaycastHit2D[ ] hits = Physics2D.RaycastAll(Utils.SetVectZ(currPosition, -10), Vector3.forward);
				for (int k = 0; k < hits.Length; k++) {
					if (hits[k].transform != transform) {
						if (hits[k].transform.tag.Equals("Space Object")) {
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
			}

			// Once a certain amount of iterations have been done, set the meshPiece to the current position
			// If there was no planet collision, then just set the position of the current meshPiece and move on to the next one
			if (!hitPlanet) {
				launchingDots[i].Position = currPosition;
			}
		}
	}

	protected override void Death ( ) {
		// Create meshPiece pieces of the ship as it gets destroyed to make for a cool effect
		levelManager.SpawnGravityParticles(Position, Constants.CRASH_PARTICLE_COUNT, Color, layerType: LayerType.Ship);

		// Spawn explosion
		levelManager.SpawnParticleSystem(ParticleSystemType.Explosion, Position);

		uiManager.HasCrashed = true;

		// Destroy this ship gameobject
		Destroy(gameObject);
	}

	public void ResetLaunch (bool forceStartLaunching = false) {
		canLaunch = true;
		IsLaunching = forceStartLaunching;
	}
}
