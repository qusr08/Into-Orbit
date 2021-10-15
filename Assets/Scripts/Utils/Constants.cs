using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The type of mesh (shape) that the object will be
public enum MeshType {
	Circle,
	RoughCircle,
	Square,
	RightTriangle,
	EquilateralTriangle
}

public enum LayerType {
	Back = 100,
	EnvironmentGlowBack = 95,
	EnvironmentGlowFront = 94,
	EnvironmentDetailOutside = 82,
	EnvironmentDetailMiddle = 81,
	EnvironmentDetailInside = 80,
	Trail = 75,
	Environment = 70,
	ShipDetail = 40,
	Ship = 30,
	Particles = 15,
	Front = 0
}

public enum ParticleSystemType {
	Collision,
	Launch,
	Explosion,
	Teleport
}

public static class Constants {
	public const float MIN_CAMERA_FOV = 4.5f;
	public const float CAMERA_DEFAULT_FOV = 5f;
	public const float MAX_CAMERA_FOV = 6.5f;

	public const int CRASH_PARTICLE_COUNT = 8;
	public const float MAX_LAUNCH_DISTANCE = 5;
	public const float MIN_LAUNCH_DISTANCE = 0.2f;

	public const float TELEPORT_BUFFER_TIME = 1f;
	public const float TELEPORT_SPEED = 10f;
	public const float SEGMENT_OVERLAP = 0.13f;
	public const float MAX_SEGMENT_OFFSET = 0.3f;
	public const float MIN_SEGMENT_MOVETIME = 4f;
	public const float MAX_SEGMENT_MOVETIME = 10f;
	public const float MAX_SEGMENT_STARTLENGTH = 1f;

	public const int MAX_TRAJECTORY_ITERATIONS = 2000;

	public const float PROPERTY_CHANGE_TIME = 0.3f;

	public const float MIN_GRAVITY_INFLUENCE = 0.005f;
	public const float MAX_CENTER_DISTANCE = 30f;
}
