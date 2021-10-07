using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour {
	[Separator("Trajectory")]
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private LevelManager levelManager;
	[Space]
	public Vector2 InitialPosition;
	public Vector2 InitialForce;
	public float Mass;
	[HideInInspector] public List<Vector2> positions;
	[HideInInspector] public List<Vector2> velocities;

	public int TotalFrames {
		get {
			return positions.Count;
		}
	}

	protected void OnValidate ( ) {
		if (levelManager == null) {
			levelManager = FindObjectOfType<LevelManager>( );
		}
		if (lineRenderer == null) {
			lineRenderer = GetComponent<LineRenderer>( );
		}

		transform.position = Utils.SetVectZ(transform.position, (int) LayerType.Back);
	}

	protected void Start ( ) {
		gameObject.SetActive(false);
	}

	public void CreateTrajectory (GravityObject gravityObject, Vector2 initialVelocity, List<MeshObject> parents) {
		InitialPosition = gravityObject.Position;
		Mass = gravityObject.Mass;
		InitialForce = initialVelocity / Mass;

		// Get the current force that would be applied to the ship if it was launched right now
		Vector2 currPosition = InitialPosition;

		// Calculate the initial velocity of the ship
		// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
		Vector2 currVelocity = InitialForce;

		// Clear the line renderer's current positions
		lineRenderer.positionCount = 0;
		lineRenderer.loop = false;

		for (int i = 0; TotalFrames < Constants.MAX_TRAJECTORY_ITERATIONS; i++) {
			lineRenderer.positionCount++;
			lineRenderer.SetPosition(lineRenderer.positionCount - 1, Utils.SetVectZ(currPosition, transform.position.z));
			positions.Add(currPosition);
			velocities.Add(currVelocity);

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
			RaycastHit2D[ ] hits = Physics2D.CircleCastAll(Utils.SetVectZ(currPosition, -10), gravityObject.Size, Vector3.forward);
			for (int k = 0; k < hits.Length; k++) {
				if (hits[k].transform != gravityObject.transform) {
					if (hits[k].transform.tag.Equals("Space Object")) {
						Debug.LogWarning($"Trajectory hits an object!");

						return;
					}
				}
			}

			if (i > 50 && Utils.CloseEnough(currPosition, InitialPosition, checkValue: 0.02f) && Utils.CloseEnough(currVelocity, InitialForce)) {
				Debug.Log($"Trajectory is an orbit! [{TotalFrames} total frames]");

				lineRenderer.loop = true;
				break;
			}

			if (i == Constants.MAX_TRAJECTORY_ITERATIONS - 1) {
				Debug.LogWarning($"Trajectory is incomplete!");
			}
		}
	}
}
