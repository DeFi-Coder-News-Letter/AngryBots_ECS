using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The spawner only runs on the server
[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class AuthoritativeSpawner : MonoBehaviour
{
	[Header("Enemy Spawn Timing")]
	[Range(.1f, 2f)] public float spawnInterval = 1f;
	public bool spawnEnemies = true;
	float cooldown;

	// Update is called once per frame
	void Update()
	{
		var spawner = GameObject.FindObjectOfType<EnemySpawner>();
		if (spawner == null)
			return;

		if (!spawnEnemies || !Settings.AnyPlayerAlive())
			return;

		cooldown -= Time.deltaTime;

		if (cooldown <= 0f)
		{
			cooldown += spawnInterval;
	
			spawner.Spawn();
		}
	}
}
