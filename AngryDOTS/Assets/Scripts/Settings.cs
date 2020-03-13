using Cinemachine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Settings : MonoBehaviour
{
	static Settings instance;

	[Header("Game Object References")]
	public List<PlayerMovementAndLook> playersComp ;
	private List<Transform> players = new List<Transform>();

	[Header("Collision Info")]
	public float playerCollisionRadius = .5f;
	public static float PlayerCollisionRadius
	{
		get { return instance.playerCollisionRadius; }
	}

	[Header("Bullets")]
	public GameObject bulletPrefab;
	private Entity bulletEntityPrefab;
	public static Entity BulletEntityPrefab
	{
		get { return instance.bulletEntityPrefab; }
	}

	private int bulletSpreadAmount = 5;
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

	List<float3> playerPositions = new List<float3>();
	public static List<float3> PlayerPositions
	{
		set { instance.playerPositions = value; }
		get { return instance.playerPositions; }
	}

	//public static int PlayerCount
	//{
	//	get { return instance.players.Count; }
	//}

	//public static NativeArray<float3> PlayerPositions
	//{
	//	get
	//	{
	//		NativeArray<float3> positions = new NativeArray<float3>(GetPlayerAliveCount(), Allocator.TempJob);
	//		int arrayIdx = 0;
	//		for (int i = 0; i < instance.players.Count; ++i)
	//		{
	//			bool alive = !instance.playersComp[i].IsDead;
	//			if (alive)
	//			{
	//				positions[arrayIdx] = (float3)instance.players[i].position;
	//				++arrayIdx;
	//			}
	//		}
	//		return positions;
	//	}
	//}

	void Awake()
	{
		if (instance != null && instance != this)
			Destroy(gameObject);
		else
			instance = this;

		players.Capacity = playersComp.Count;
		for (int i=0; i < playersComp.Count; ++i)
		{
			players.Add(playersComp[i].transform);
		}
	}

	private Dictionary<int, float3> playerPosition = new Dictionary<int, float3>();

	public static void SetPlayerPosition(int playerId, ref float3 position)
	{
		instance.playerPosition[playerId] = position;
	}

	void Start()
	{
		//var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
		//bulletEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);
	}

	public static Vector3 GetPlayerPosition(int idx)
	{
		return instance.players[idx].position;
	}

	public static float3 GetPositionAroundPlayer(int idx, float radius)
	{
		//Vector3 playerPos = instance.players[idx].position;
		if (!instance.playerPosition.ContainsKey(idx + 1))
		{
			return float3.zero;
		}
		float3 playerPos = instance.playerPosition[idx + 1];

		//float angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
		Unity.Mathematics.Random rnd = default(Unity.Mathematics.Random);
		rnd.InitState();
		float angle = rnd.NextFloat(0f, 2 * Unity.Mathematics.math.PI);
		//float s = Mathf.Sin(angle);
		float s = Unity.Mathematics.math.sin(angle);
		float c = Unity.Mathematics.math.cos(angle);

		//return new Vector3(c * radius, 1.1f, s * radius) + playerPos;
		return new float3(c * radius, 1.1f, s * radius) + playerPos;
	}

	public static void PlayerDied(int idx)
	{
		if (instance.playersComp[idx].IsDead)
			return;

		PlayerMovementAndLook playerMove = instance.playersComp[idx];
		playerMove.PlayerDied();

		//instance.players[idx] = null;
	}

	public static bool IsPlayerDead(int idx)
	{
		return instance.playersComp[idx].IsDead;
	}

	public static bool AnyPlayerAlive()
	{
		return instance.playerPosition.Count > 0;
		//for (int i=0; i < instance.players.Count; ++i)
		//{
		//	if (!instance.playersComp[i].IsDead)
		//	{
		//		return true;
		//	}
		//}
		//return false;
	}

	public static int GetPlayerAliveCount()
	{
		return instance.playerPosition.Count;
		//int count = 0;
		//for (int i = 0; i < instance.players.Count; ++i)
		//{
		//	if (!instance.playersComp[i].IsDead)
		//	{
		//		++count;
		//	}
		//}
		//return count;
	}
}
