using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
	[Separator("Menu Manager")]
	[SerializeField] private GameObject asteroidPrefab;
	[Space]
	[SerializeField] private float spawnTime;
	[SerializeField] private int maxAsteroids;

	private List<GameObject> asteroids = new List<GameObject>( );
	private float spawnTimer;

	protected void Update ( ) {
		/*
		spawnTimer += Time.unscaledDeltaTime;
		if (spawnTimer >= spawnTime) {
			float angle = Random.Range(0, Mathf.PI * 2);
			Vector2 position = Camera.main.orthographicSize * 2 * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			Vector2 force = -position.normalized * Random.Range(5, 15);

			GameObject asteroid = Instantiate(asteroidPrefab, position, Quaternion.identity);
			asteroid.GetComponent<Rigidbody2D>( ).AddForce(force, ForceMode2D.Impulse);
			asteroids.Insert(0, asteroid);

			if (asteroids.Count > maxAsteroids) {
				Destroy(asteroids[maxAsteroids]);
				asteroids.RemoveAt(maxAsteroids);
			}

			spawnTimer -= spawnTime;
		}
		*/
	}
}
