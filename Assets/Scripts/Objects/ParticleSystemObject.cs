using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleSystemObject : MonoBehaviour {
	[Separator("Particle System Object")]
	[SerializeField] private ParticleSystem particleSystemComponent;
	[Space]
	[SerializeField] private LayerType layerType;
	[SerializeField] [ConditionalField("SetLifespanFromParticleSystem", true)] private bool hasLifespan;
	[SerializeField] [ConditionalField("hasLifespan")] private float lifespan;

	public bool IsEmitting {
		get {
			return particleSystemComponent.emission.enabled;
		}

		set {
			EmissionModule emission = particleSystemComponent.emission;
			emission.enabled = value;
		}
	}


	protected void OnValidate ( ) {
		if (particleSystemComponent == null) {
			particleSystemComponent = GetComponent<ParticleSystem>( );
		}
	}

	protected void Start ( ) {
		transform.position = Utils.SetVectZ(transform.position, (int) layerType);
	}

	protected void Update ( ) {
		if (hasLifespan) {
			lifespan -= Time.deltaTime;

			if (lifespan <= 0) {
				Destroy(gameObject);
			}
		}
	}

	public void SetLifespan (float lifespan) {
		hasLifespan = true;
		this.lifespan = lifespan;
	}
}
