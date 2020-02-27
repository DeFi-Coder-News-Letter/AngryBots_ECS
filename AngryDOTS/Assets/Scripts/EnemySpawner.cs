using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Header("Enemy Spawn Info")]
	public bool spawnEnemies = true;
	public bool useECS = false;
	public float enemySpawnRadius = 10f;
	public GameObject enemyPrefab;

	[Header("Enemy Spawn Timing")]
	[Range(1, 100)] public int spawnsPerInterval = 1;
	[Range(.1f, 2f)] public float spawnInterval = 1f;
	
	EntityManager manager;
	Entity enemyEntityPrefab;

	float cooldown;


	void Start()
	{
		if (useECS)
		{
			manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
			enemyEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
		}
	}

	void Update()
    {
		if (!spawnEnemies || !Settings.AnyPlayerAlive())
			return;

		cooldown -= Time.deltaTime;

		if (cooldown <= 0f)
		{
			cooldown += spawnInterval;
			Spawn();
		}
    }

	void Spawn()
	{
		for (int i = 0; i < spawnsPerInterval; i++)
		{
			int playerIdx = Random.Range(0, Settings.GetPlayerAliveCount());
			Vector3 pos = Settings.GetPositionAroundPlayer(playerIdx, enemySpawnRadius);

			if (!useECS)
			{
				Instantiate(enemyPrefab, pos, Quaternion.identity);
			}
			else
			{
				Entity enemy = manager.Instantiate(enemyEntityPrefab);
				manager.SetComponentData(enemy, new Translation { Value = pos });
			}
		}
	}
}
