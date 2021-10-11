using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : GravityObject {
	[Separator("Asteroid")]
	[SerializeField] private GameObject trajectoryPrefab;
	[Space]
	[SerializeField] private bool createTrajectoryObject;
	[SerializeField] private Trajectory trajectory;
	[SerializeField] [ReadOnly] private Trajectory trajectoryData;
	[SerializeField] [ConditionalField(nameof(trajectoryData))] private bool clearTrajectoryData;
	[SerializeField] [ConditionalField(nameof(trajectory))] private bool updateFromTrajectory;
	[Space]
	[SerializeField] private Vector2 initialVelocity;
	[SerializeField] [Range(0f, 1f)] [ConditionalField(nameof(trajectory))] private float trajectoryReposition;

	private int trajectoryFrameCounter = 0;

	private Vector2 initialPosition {
		get {
			return trajectory.positions[(int) (trajectoryReposition * trajectory.TotalFrames)];
		}
	}
	private Vector2 initialForce {
		get {
			return trajectory.velocities[(int) (trajectoryReposition * trajectory.TotalFrames)] * Mass;
		}
	}

	protected new void OnValidate ( ) {
		base.OnValidate( );

		if (createTrajectoryObject) {
			if (trajectory == null) {
				trajectoryData = trajectory = Instantiate(trajectoryPrefab).GetComponent<Trajectory>( );

				Debug.Log("Created new trajectory object.");
			}

			trajectory.CreateTrajectory(this, initialVelocity, parents);

			createTrajectoryObject = false;
			updateFromTrajectory = true;
		}

		if (updateFromTrajectory) {
			if (Mass != trajectory.Mass) {
				Debug.LogWarning("Trajectory was created with a different mass value!");
			} else {
				trajectoryData = trajectory;
				Position = initialPosition;
				initialVelocity = initialForce;

				Debug.Log("Updated variables from current trajectory.");
			}

			updateFromTrajectory = false;
		}

		if (clearTrajectoryData) {
			trajectoryData = null;

			clearTrajectoryData = false;
		}
	}

	protected new void Start ( ) {
		base.Start( );

		rigidBody.AddForce(initialVelocity, ForceMode2D.Impulse);
	}

	protected new void FixedUpdate ( ) {
		if (!hasCollided) {
			if (trajectoryData != null) {
				if (trajectoryFrameCounter == trajectoryData.TotalFrames + 1) {
					Position = initialPosition;
					rigidBody.velocity = initialForce / Mass;

					trajectoryFrameCounter -= trajectoryData.TotalFrames;
				}
			}

			trajectoryFrameCounter++;
		}

		base.FixedUpdate( );
	}

	protected override void Death ( ) {
	}
}
