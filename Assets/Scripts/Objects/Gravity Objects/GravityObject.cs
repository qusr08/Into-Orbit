using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityObject : MeshObject {
	[Separator("Gravity Object")]
	[SerializeField] protected List<MeshObject> parents = new List<MeshObject>( );

	private bool updateGravity = true;
	protected bool hasCollided;

	private Vector2 refWormholePositionVelocity;
	private Vector2 refWormholeScaleVelocity;
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

			// Set the rigidBody velocity to 0 because setting the position over time would just be better here
			refWormholePositionVelocity = rigidBody.velocity;
			rigidBody.velocity = Vector2.zero;

			updateGravity = false;
			hasCollided = true;
			DisableSolidColliders = true;
			ShowTrail = false;
		}
	}

	private float teleportBufferTimer;
	private bool teleportInitialShowTrail;
	private Vector2 teleportInitialVelocity;
	private Vector2 travelToPortalPoint;
	private ParticleSystemObject teleportParticleSystem;
	private Teleportal teleportal;
	public Teleportal Teleportal {
		get {
			return teleportal;
		}

		set {
			teleportal = value;
			if (teleportal == null) {
				updateGravity = true;
				EnableCollisions = true;
				ShowTrail = teleportInitialShowTrail;
				rigidBody.velocity = teleportInitialVelocity;
				meshRenderer.enabled = true;
				teleportBufferTimer = Constants.TELEPORT_BUFFER_TIME;

				teleportParticleSystem.SetLifespan(5f);
				teleportParticleSystem.IsEmitting = false;

				return;
			}

			updateGravity = false;
			hasCollided = true;
			EnableCollisions = false;
			teleportInitialShowTrail = ShowTrail;
			ShowTrail = false;
			teleportInitialVelocity = rigidBody.velocity;
			rigidBody.velocity = Vector2.zero;
			meshRenderer.enabled = false;

			teleportParticleSystem = levelManager.SpawnParticleSystem(ParticleSystemType.Teleport, transform).GetComponent<ParticleSystemObject>( );
		}
	}

	protected void OnCollisionEnter2D (Collision2D collision) {
		string collisionTag = collision.transform.tag;

		// If the object collides with another object, it should be destroyed
		if (collisionTag.Equals("Space Object")) {
			hasCollided = true;

			// levelManager.SpawnParticleSystem(ParticleSystemType.Collision, Position);

			Death( );
		}
	}

	protected void OnTriggerEnter2D (Collider2D collision) {
		string collisionTag = collision.transform.tag;

		if (collisionTag.Equals("Wormhole")) {
			if (Wormhole == null) {
				Wormhole = collision.transform.parent.GetComponent<Wormhole>( );
			}
		} else if (teleportBufferTimer <= 0 && collisionTag.Equals("Teleportal")) {
			if (Teleportal == null) {
				Transform portal = collision.transform.parent;
				Teleportal = portal.parent.GetComponent<Teleportal>( );
				Position = portal.position;
				travelToPortalPoint = Teleportal.GetTeleportPosition(Position, portal);
			}
		} else if (collisionTag.Equals("Button")) {
			collision.transform.parent.GetComponent<ButtonObject>( ).OnObjectCollision( );
		} else if (collisionTag.Equals("Boost Recharge")) {
			collision.transform.parent.GetComponent<BoostRechargeObject>( ).OnObjectCollision( );
		}
	}

	protected void Start ( ) {
		parents = new List<MeshObject>( );
	}

	protected void Update ( ) {
		if (teleportBufferTimer > 0) {
			teleportBufferTimer -= Time.deltaTime;
		}
	}

	protected new void FixedUpdate ( ) {
		// Update the trail after the position has changed
		base.FixedUpdate( );

		// As long as the object is not locked, calculate the force that should be applied to it
		if (!IsLocked && updateGravity) {
			// Calculate the gravity that the ship will experience at the current position
			rigidBody.AddForce(levelManager.CalculateGravityForce(this, onlyParents: parents), ForceMode2D.Force);
		}

		if (Wormhole != null) {
			// Make the ship smoothly transition to the center of the wormhole
			Position = Vector2.SmoothDamp(Position, Wormhole.Position, ref refWormholePositionVelocity, 0.5f);
			Scale = Vector2.SmoothDamp(Scale, Vector2.zero, ref refWormholeScaleVelocity, 0.5f);

			// If the ship has reached the center and is stopped, then the player has won the level
			if (Utils.CloseEnough(Position, Wormhole.Position) && Utils.CloseEnough(Scale, Vector2.zero)) {
				Position = Wormhole.Position;
				Scale = Vector2.zero;

				IsLocked = true;
			}
		}

		if (Teleportal != null) {
			Position = Vector2.MoveTowards(Position, travelToPortalPoint, Time.fixedDeltaTime * Constants.TELEPORT_SPEED);

			if (Utils.CloseEnough(Position, travelToPortalPoint)) {
				Position = travelToPortalPoint;

				Teleportal = null;
			}
		}
	}

	protected abstract void Death ( );
}
