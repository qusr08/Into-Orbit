using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemObject : MonoBehaviour {
	[Separator("Particle System Object")]
	[SerializeField] [ConditionalField("SetLifespanFromParticleSystem", true)] private bool hasLifespan;
	[SerializeField] [ConditionalField("hasLifespan")] private float lifespan;

	protected void Update ( ) {
		if (hasLifespan) {
			lifespan -= Time.deltaTime;

			if (lifespan <= 0) {
				Destroy(gameObject);
			}
		}
	}
}
