using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Header("Enemy Spawn Info")]
	
	public bool useECS = false;
	public float enemySpawnRadius = 10f;
	public GameObject enemyPrefab;
	   	
	EntityManager manager;
	Entity enemyEntityPrefab;
	[Range(1, 100)] public int spawnsPerInterval = 1;
		
	void Start()
	{
		if (useECS)
		{
			manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
			enemyEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
		}
	}
		
	public void Spawn()
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
