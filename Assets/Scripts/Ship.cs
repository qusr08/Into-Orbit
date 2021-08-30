using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : GravityObject {
	[Header("--- Ship Class ---")]
	[SerializeField] private Collider2D launchingCollider;

	protected new void Awake ( ) {
		base.Awake( );

		rigidBody.AddForce(Utils.RandNormVect2( ) * 0.5f, ForceMode2D.Impulse);
	}

	protected void OnCollisionEnter2D (Collision2D collision2D) {
		if (collision2D.transform.tag.Equals("Planet")) {
			Death( );
		}
	}

	private void OnMouseOver ( ) {

	}

	private void Death ( ) {
		levelManager.SpawnParticles(transform.position, Constants.SHIP_PARTICLE_COUNT);

		Destroy(gameObject);
	}
}
