using Cinemachine;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Settings : MonoBehaviour
{
	static Settings instance;

	[Header("Collision Info")]
	public float playerCollisionRadius = .5f;
	public static float PlayerCollisionRadius
	{
		get { return instance.playerCollisionRadius; }
	}

	[Header("Bullets")]
	public bool bulletSpread;
	public static bool BulletSpread
	{
		get { return instance.bulletSpread; }
	}

	public int bulletSpreadAmount = 5;
	public static int BulletSpreadAmount
	{
		get { return instance.bulletSpreadAmount; }
	}

	[Header("Camera")]
	public CinemachineVirtualCamera virtualCamera;
	public static CinemachineVirtualCamera Camera
	{
		get { return instance.virtualCamera; }
	}

	[Header("Enemy Spawning")]
	public bool spawnEnemies = true;
	public static bool SpawnEnemies
	{
		get { return instance.spawnEnemies; }
	}

	[Range(.1f, 2f)] public float spawnInterval = 1.0f;
	public static float SpawnInterval
	{
		get { return instance.spawnsPerInterval; }
	}

	[Range(1, 100)] public int spawnsPerInterval = 1;
	public static int SpawnsPerInterval
	{
		get { return instance.spawnsPerInterval; }
	}

	public float enemySpawnRadius = 10.0f;
	public static float EnemySpawnRadius
	{
		get { return instance.enemySpawnRadius; }
	}

	public float enemyCollisionRadius = .3f;
	public static float EnemyCollisionRadius
	{
		get { return instance.enemyCollisionRadius; }
	}

	public static int PlayersAlive
	{
		set;
		get;
	}

	public static int PlayersCount
	{
		set;
		get;
	}

	private List<float3> playerPositions = new List<float3>();
	public static List<float3> PlayerPositions
	{
		set { instance.playerPositions = value; }
		get { return instance.playerPositions; }
	}

	void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
		}
	}
}
