using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The type of mesh (shape) that the object will be
public enum MeshType {
	Circle,
	RoughCircle,
	Square,
	Triangle
}

public enum LayerType {
	Back = 100,
	EnvironmentGlowBack = 95,
	EnvironmentGlowFront = 94,
	WormholeOutside = 82,
	WormholeMiddle = 81,
	WormholeInside = 80,
	Environment = 70,
	ShipDetail = 40,
	Ship = 30,
	Front = 0
}

public static class Constants {
	public const float MIN_CAMERA_FOV = 4.5f;
	public const float CAMERA_DEFAULT_FOV = 5f;
	public const float MAX_CAMERA_FOV = 5.5f;

	public const int CRASH_PARTICLE_COUNT = 8;
	public const float MAX_LAUNCH_DISTANCE = 5;
	public const float MIN_LAUNCH_DISTANCE = 0.2f;
}
