using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : GravityObject {
	[Header("--- Asteroid Class ---")]
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private List<MeshObject> parents = new List<MeshObject>( );
	[Space]
	[SerializeField] private bool recalculateTrajectory = false;
	[SerializeField] private bool showTrajectory;
	[SerializeField] private Vector2 initialForce;
	[SerializeField] private int trajectoryIterations = 50;
	[SerializeField] private int trajectoryDensity = 5;

	protected new void OnValidate ( ) {
		base.OnValidate( );

		if (lineRenderer == null) {
			lineRenderer = GetComponent<LineRenderer>( );
		}

		if (recalculateTrajectory) {
			CreateTrajectory( );

			recalculateTrajectory = false;
		}

		lineRenderer.enabled = showTrajectory;
	}

	protected void Start ( ) {
		rigidBody.AddForce(initialForce, ForceMode2D.Impulse);
	}

	protected new void FixedUpdate ( ) {
		// Calculate the gravity that the ship will experience at the current position
		rigidBody.AddForce(levelManager.CalculateGravityForce(this, parents), ForceMode2D.Force);
	}

	private void CreateTrajectory ( ) {
		// Get the current force that would be applied to the ship if it was launched right now
		Vector2 currPosition = Position;

		// Calculate the initial velocity of the ship
		// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
		Vector2 currVelocity = initialForce / Mass;

		// Clear the line renderer's current positions
		lineRenderer.positionCount = 0;

		for (int i = 0; i < trajectoryIterations; i++) {
			lineRenderer.positionCount++;
			lineRenderer.SetPosition(lineRenderer.positionCount - 1, currPosition);

			for (int j = 0; j < trajectoryDensity; j++) {
				// Calculate the gravity that the ship will experience at the current position
				Vector2 gravityForce = levelManager.CalculateGravityForce(currPosition, Mass, parents);
				// Calculate the acceleration due to the gravity force
				Vector2 gravityAcc = gravityForce / Mass;

				// Increment the velocity by the acceleration
				currVelocity += gravityAcc * Time.fixedDeltaTime;
				// Increment the position by the velocity
				currPosition += currVelocity * Time.fixedDeltaTime;

				// My Forum Post: https://forum.unity.com/threads/need-help-predicting-the-path-of-an-object-in-a-2d-gravity-simulation.1170098/

				// If the current position is on a planet, do not draw the rest of the trajectory
				RaycastHit2D[ ] hits = Physics2D.RaycastAll(Utils.SetVectZ(currPosition, -10), Vector3.forward);
				for (int k = 0; k < hits.Length; k++) {
					if (hits[k].transform.tag.Equals("Obstacle") && hits[k].transform != transform) {
						// Break out of the for loop and stop calculating the rest of the trajectory
						i = trajectoryIterations;
						j = trajectoryDensity;
						break;
					}
				}
			}
		}
	}
}
