using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	private Wormhole wormhole;
	public Wormhole Wormhole {
		get {
			return wormhole;
		}

		set {
			wormhole = value;

			// Save the velocity that the ship has when entering the wormhole
			startingVelocity = rigidBody.velocity;
			endingVelocity = startingVelocity.normalized * angularSpiralSpeed;

			// Current velocity forms a line between these two points
			// Given the starting point of the line and the direction, find the other point that the line intersects with the circle
			// https://www.quora.com/How-would-we-find-the-point-of-intersection-of-a-circle-and-a-line

			// Go to https://www.desmos.com/calculator/m4g6ewpo93 to see all the math for this
			// Declare all variables
			float pX = Position.x;
			float pY = Position.y;
			float r = wormhole.Size / 2;
			float rise = rigidBody.velocity.y;
			float run = rigidBody.velocity.x;
			float h = wormhole.transform.position.x;
			float k = wormhole.transform.position.y;

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

			List<Particle> debugParticles = levelManager.SpawnParticles(transform, 3, Color.blue, meshType: MeshType.Circle, giveRandomForce: false, disableColliders: true);
			debugParticles[0].Position = startingPoint;
			debugParticles[1].Position = spiralPoint;
			debugParticles[2].Position = endingPoint;
			foreach (Particle particle in debugParticles) {
				particle.IsLocked = true;
			}

			// Calculate whether the ship should spiral clockwise or counterclockwise
			// If the angle of the spiral point around the center of the circle is less than the angle of the entry point, then it is going clockwise
			float spiralPointAngle = Utils.GetRotation2D(wormhole.transform.position, spiralPoint);
			float shipAngle = Utils.GetRotation2D(wormhole.transform.position, Position);

			// Because math sucks, make sure the spiral angle and ship angle can be compared. If the angle is greater than 180 degrees, its negative.
			//	So, just make sure they can be compared
			if (shipAngle < 0) {
				shipAngle += 360;
			}
			if (spiralPointAngle < 0) {
				spiralPointAngle += 360;
			}

			isGoingClockwise = (spiralPointAngle < shipAngle);

			Debug.Log($"{spiralPointAngle} < {shipAngle}");
			Debug.Log($"ST: {startingPoint} | SP: {spiralPoint} | EN: {endingPoint}");
		}
	}

	private Vector2 startingPoint;
	private Vector2 endingPoint;
	private Vector2 spiralPoint;
	private Vector2 startingVelocity;
	private Vector2 endingVelocity;
	private float spiralRadius = 0;
	private float currentAngle = 0;
	private float angularSpiralSpeed = 10f; // 5 degrees per frame
	private bool isGoingClockwise = false;
	private bool isSpiraling = false;

	protected void FixedUpdate ( ) {
		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked) {
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

						spiralRadius = Vector2.Distance(Position, Wormhole.transform.position);
						// The angle is in degrees, not radians
						currentAngle = Utils.GetRotation2D(Wormhole.transform.position, Position);

						Debug.Log($"CURR SP: {Position}");
					}
				} else {
					spiralRadius -= 0.005f;
					if (spiralRadius <= 0) {
						spiralRadius = 0;
					}

					Vector2 circleCoords = new Vector2(Mathf.Cos(Mathf.Deg2Rad * currentAngle), Mathf.Sin(Mathf.Deg2Rad * currentAngle));
					Position = (Vector2) Wormhole.transform.position + (spiralRadius * circleCoords);

					currentAngle += angularSpiralSpeed * ((isGoingClockwise) ? -1 : 1);
				}

				// If the ship has reached the center and is stopped, then the player has won the level
				if (Utils.CloseEnough(Position, Wormhole.transform.position)) {
					Position = Wormhole.transform.position;

					Wormhole = null;
					IsLocked = true;
				}
			} else {
				// Calculate the gravity that the ship will experience at the current position
				rigidBody.AddForce(levelManager.CalculateGravityForce(this), ForceMode2D.Force);
			}
		}
	}
}
