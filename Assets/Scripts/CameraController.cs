using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Header("--- Camera Controller Class ---")]
	[SerializeField] public Transform Target;
	[Header("--- Camera Controller Constants ---")]
	[SerializeField] [Range(0f, 1f)] private float STIFFNESS;

	private Vector3 velocity;
	private Vector3 lastTargetPosition;

	private void FixedUpdate ( ) {
		// If the camera is currently tracking a target, get its current position
		// The reason for this is that if the target is destoryed, I want the camera to continue to its last location
		//	before it was destroyed.
		if (Target != null) {
			lastTargetPosition = Utils.SetZ(Target.position, transform.position.z);
		}

		// If the right mouse button is pressed, I want the camera to move according to the mouse
		Vector3 moveToLocation = lastTargetPosition;
		if (Input.GetMouseButton(1)) {
			moveToLocation = Utils.SetZ(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position.z);
		}

		// Move the camera smoothly to the location of the target (or the mouse if the right mouse button is held)
		transform.position = Vector3.SmoothDamp(transform.position, moveToLocation, ref velocity, STIFFNESS);
	}
}
