using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public static float MIN_CAMERA_FOV = 4.5f;
	public static float CAMERA_DEFAULT_FOV = 5f;
	public static float MAX_CAMERA_FOV = 5.5f;

	[Header("--- Camera Controller Class ---")]
	[SerializeField] public Transform Target;
	[Header("--- Camera Controller Constants ---")]
	[SerializeField] [Range(0f, 1f)] private float STIFFNESS;

	public float FOV {
		get {
			return Camera.main.orthographicSize;
		}

		set {
			Camera.main.orthographicSize = value;
		}
	}

	private Vector3 trackingVelocity;
	private Vector3 lastTargetPosition;

	private float fovVelocity;
	private float targetFOV;

	private void Start ( ) {
		ResetFOV( );	
	}

	private void FixedUpdate ( ) {
		// If the camera is currently tracking a target, get its current position
		// The reason for this is that if the target is destoryed, I want the camera to continue to its last location
		//	before it was destroyed.
		if (Target != null) {
			lastTargetPosition = Utils.SetVectZ(Target.position, transform.position.z);
		}

		// If the right mouse button is pressed, I want the camera to move according to the mouse
		Vector3 moveToLocation = lastTargetPosition;
		if (Input.GetMouseButton(1)) {
			moveToLocation = Utils.SetVectZ(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position.z);
		}

		// Move the camera smoothly to the location of the target (or the mouse if the right mouse button is held)
		transform.position = Vector3.SmoothDamp(transform.position, moveToLocation, ref trackingVelocity, STIFFNESS);

		// Smoothly move the current FOV to the target FOV
		if (FOV != targetFOV) {
			FOV = Mathf.SmoothDamp(FOV, targetFOV, ref fovVelocity, STIFFNESS);
		}
	}

	public void SetTargetFOV (float targetFOV) {
		this.targetFOV = Utils.Limit(targetFOV, MIN_CAMERA_FOV, MAX_CAMERA_FOV);
	}

	public void ResetFOV () {
		targetFOV = CAMERA_DEFAULT_FOV;
	}
}
