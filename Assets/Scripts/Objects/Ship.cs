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
	[SerializeField] private int launchDotCount = 20;
	[SerializeField] private float launchDotDensity = 5;

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

			// Save the velocity that the ship has when entering the wormhole
			startingVelocity = rigidBody.velocity;
			endingVelocity = startingVelocity.normalized * angularSpiralSpeed;

			// Current velocity forms a line between these two points
			// Given the starting point of the line and the direction, find the other point that the line intersects with the circle
			// https://www.quora.com/How-would-we-find-the-point-of-intersection-of-a-circle-and-a-line

			// Go to https://www.desmos.com/calculator/wyqaqbojqk to see all the math for this
			// Declare all variables
			float pX = Position.x;
			float pY = Position.y;
			float r = wormhole.Radius;
			float rise = rigidBody.velocity.y;
			float run = rigidBody.velocity.x;
			float h = wormhole.Position.x;
			float k = wormhole.Position.y;

			// Set the rigidBody velocity to 0 because setting the position over time would just be better here
			rigidBody.velocity = Vector2.zero;

			// The points that the ship will intersect on the wormhole circle
			Vector2 p1, p2 = Vector2.zero;

			// If run = 0, then the slope of the ship is undefined (that means the ship is either travelling straight up or straight down)
			// Run a different calculation if that is the case
			if (run != 0) {
				float m = rise / run;

				float a = 1 + Mathf.Pow(m, 2);
				float b = -2 * (h + (Mathf.Pow(m, 2) * pX) - (m * pY) + (m * k));
				float c = (2 * m * k * pX) - (2 * k * pY) - (2 * m * pX * pY) + (Mathf.Pow(m, 2) * Mathf.Pow(pX, 2)) + Mathf.Pow(pY, 2) + Mathf.Pow(h, 2) + Mathf.Pow(k, 2) - Mathf.Pow(r, 2);

				float x1 = (-b + Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c))) / (2 * a);
				float x2 = (-b - Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c))) / (2 * a);

				p1 = new Vector2(x1, m * (x1 - pX) + pY);
				p2 = new Vector2(x2, m * (x2 - pX) + pY);
			} else {
				// float a = 1;
				float b = -2 * k;
				float c = Mathf.Pow(pX, 2) - (2 * h * pX) + Mathf.Pow(h, 2) + Mathf.Pow(k, 2) - Mathf.Pow(r, 2);

				float y1 = (-b + Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * c))) / 2;
				float y2 = (-b - Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * c))) / 2;

				p1 = new Vector2(pX, y1);
				p2 = new Vector2(pX, y2);
			}

			// Make sure the correct starting positions and ending positions are known
			if (Vector2.Distance(p1, Position) < Vector2.Distance(p2, Position)) {
				startingPoint = p1;
				endingPoint = p2;
			} else {
				startingPoint = p2;
				endingPoint = p1;
			}

			// The sprial point is just the midpoint between the starting point and the endpoint
			spiralPoint = (endingPoint + startingPoint) / 2;

			// Calculate whether the ship should spiral clockwise or counterclockwise
			// If the angle of the spiral point around the center of the circle is less than the angle of the entry point, then it is going clockwise
			float spiralPointAngle = Utils.GetRotation2D(wormhole.Position, spiralPoint);
			float shipAngle = Utils.GetRotation2D(wormhole.Position, Position);

			// Because math sucks, make sure the spiral angle and ship angle can be compared. If the angle is greater than 180 degrees, its negative.
			//	So, just make sure they can be compared
			if (shipAngle < 0) {
				shipAngle += 360;
			}
			if (spiralPointAngle < 0) {
				spiralPointAngle += 360;
			}

			isGoingClockwise = (spiralPointAngle < shipAngle);
		}
	}

	private Vector2 startingPoint; // The point that the ship enters the wormhole
	private Vector2 endingPoint; // The point at which the ship would leave the wormhole if it travelled in a straight line through it
	private Vector2 spiralPoint; // The point inside the wormhole that the ship should start spiraling at
	private Vector2 startingVelocity; // The velocity of the ship as it enters the wormhole
	private Vector2 endingVelocity; // The velocity that the ship should be at when it reaches the spiral point
	private float spiralRadius = 0; // The radius that the ship is away from the center of the wormhole
	private float currentAngle = 0; // The current angle of the ship as it spirals around the wormhole
	private float angularSpiralSpeed = 10f; // degrees per frame
	private bool isGoingClockwise = false; // Whether or not the ship is spinning clockwise or counterclockwise around the wormhole
	private bool isSpiraling = false; // Whether or not the ship has reached the spiral point or not

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
				launchingDots = levelManager.SpawnParticles(transform, launchDotCount, Utils.Hex2Color("EDEDED"),
					size: 0.1f, meshType: MeshType.Circle, layerType: LayerType.ShipDetail, giveRandomForce: false, showTrail: false, disableColliders: true);
				// Make sure to lock all of the particles because the ones for the trail should not move
				foreach (Particle particle in launchingDots) {
					particle.IsLocked = true;
				}
			} else {
				// Destroy all of the launching particles
				for (int i = launchingDots.Count - 1; i >= 0; i--) {
					Destroy(launchingDots[i].gameObject);
					launchingDots.RemoveAt(i);
				}

				// Clear the list of particles since they have all been destroyed by now
				launchingDots.Clear( );
			}
		}
	}
	private List<Particle> launchingDots; // All of the particles that make up the trail while launching

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
				PositionIndicator(p1, p2, distance);
				CreateDots(direction, distance);
			}

			// If the left mouse button is unpressed, disable launching
			if (Input.GetMouseButtonUp(0)) {
				IsLaunching = false;

				// Unlock the ship and add a force the is proportional to the distance the player dragged the mouse
				IsLocked = false;
				rigidBody.AddForce(direction * (distance / MAX_LAUNCH_DISTANCE), ForceMode2D.Impulse);

				// Spawn launch explosion particles
				float angleModifier = 90 + launchParticleSystem.transform.rotation.eulerAngles.z;
				Instantiate(launchParticleSystem, Position, Quaternion.Euler(new Vector3(0, 0, angleModifier + Utils.GetRotation2D(Position, direction))));
			}

			// Update the last mouse position
			lastMousePosition = p1;
		}
	}

	private new void FixedUpdate ( ) {
		if (Wormhole != null) {
			// Make the ship smoothly transition to the center of the wormhole

			// Current velocity forms a line between these two points
			// There is a point on that line that creates a right angle with an imaginary line drawn from the center of the wormhole to the velocity line
			// Once ship is at (or passes) the point, begin slowly decreasing the distance that the ship is away from the center
			// move the ship around the center of the wormhole in a circle as it gets closer (use sin and cos to change the position)
			// Since we are no longer using the velocity of the rigidbody, it can be set to 0
			// Once the ship is close enough or at the center, set its position to the center and lock it

			if (!isSpiraling) {
				// Update the velocity of the ship so it slows down a bit to match the angular speed of it sprialing into the wormhole
				float progressToSpiralPoint = Vector2.Distance(Position, spiralPoint) / Vector2.Distance(startingPoint, spiralPoint);
				Vector2 currVelocity = Utils.LinearInterpolation(progressToSpiralPoint, startingVelocity, endingVelocity);
				Position += currVelocity * Time.fixedDeltaTime;

				// Once the ship gets close enough to the ending point (as in, once it gets past the spiral point), have the ship start spiraling
				if (Vector2.Distance(Position, endingPoint) <= Vector2.Distance(Position, startingPoint)) {
					isSpiraling = true;
					Position = spiralPoint;

					spiralRadius = Vector2.Distance(Position, Wormhole.Position);
					// The angle is in degrees, not radians
					currentAngle = Utils.GetRotation2D(Wormhole.Position, Position);
				}
			} else {
				spiralRadius -= 0.005f;
				if (spiralRadius <= 0) {
					spiralRadius = 0;
				}

				Vector2 circleCoords = new Vector2(Mathf.Cos(Mathf.Deg2Rad * currentAngle), Mathf.Sin(Mathf.Deg2Rad * currentAngle));
				Position = (Vector2) Wormhole.Position + (spiralRadius * circleCoords);

				currentAngle += angularSpiralSpeed * ((isGoingClockwise) ? -1 : 1);
			}

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
		launchingIndicator.localPosition = Utils.SetZ(Utils.LimitVector3(Position, indicatorPosition, 0, MAX_LAUNCH_DISTANCE / 2), 1);

		// Calculate rotation angle of this transform relative to the indicator
		launchingIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, Utils.GetRotation2D(p2, p1)));

		// Set the size of the indicator based on the distance of the mouse from the ship
		float height = launchingIndicator.GetComponent<SpriteRenderer>( ).size.y;
		launchingIndicator.GetComponent<SpriteRenderer>( ).size = new Vector2(distance, height);
	}

	private void CreateDots (Vector2 direction, float distance) {
		// Get the current force that would be applied to the ship if it was launched right now
		Vector2 currForce = direction * (distance / MAX_LAUNCH_DISTANCE);
		Vector2 currPosition = Position;

		// Calculate the initial velocity of the ship
		// Time can be ignored here because the ship will be launched with an impulse (instantanious) force
		Vector2 currVelocity = currForce / Mass;

		// Calculate the positions of each of the particles along the ships path
		for (int i = 0; i < launchingDots.Count; i++) {
			// Make sure the particle that is being positioned is active
			launchingDots[i].gameObject.SetActive(true);

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
			}

			// Once a certain amount of iterations have been done, set the particle to the current position
			// If the current position is on a planet, do not draw the rest of the particles to show that the ship will crash into
			//	the planet
			RaycastHit2D hit = Physics2D.Raycast(Utils.SetZ(currPosition, -10), Vector3.forward);
			if (hit && hit.transform.tag.Equals("Planet")) {
				// Disable all particles later in the list
				for (int j = i; j < launchingDots.Count; j++) {
					launchingDots[j].gameObject.SetActive(false);
				}

				// Break out of the for loop and stop calculating the position for the rest of the particles
				i = launchingDots.Count;
			} else {
				// If there was no planet collision, then just set the position of the current particle and move on to the next one
				launchingDots[i].Position = currPosition;
			}
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
