using System.Collections.Generic;
using Unity.Collections;
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

	public float enemyCollisionRadius = .3f;
	public static float EnemyCollisionRadius
	{
		get { return instance.enemyCollisionRadius; }
	}
	
	public static int PlayerCount
	{
		get { return instance.players.Count; }
	}

	public static NativeArray<float3> PlayerPositions
	{
		get
		{
			NativeArray<float3> positions = new NativeArray<float3>(GetPlayerAliveCount(), Allocator.TempJob);
			int arrayIdx = 0;
			for (int i = 0; i < instance.players.Count; ++i)
			{
				bool alive = !instance.playersComp[i].IsDead;
				if (alive)
				{
					positions[arrayIdx] = (float3)instance.players[i].position;
					++arrayIdx;
				}
			}
			return positions;
		}
	}

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

	public static Vector3 GetPlayerPosition(int idx)
	{
		return instance.players[idx].position;
	}

	public static Vector3 GetPositionAroundPlayer(int idx, float radius)
	{
		Vector3 playerPos = instance.players[idx].position;

		float angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
		float s = Mathf.Sin(angle);
		float c = Mathf.Cos(angle);
		
		return new Vector3(c * radius, 1.1f, s * radius) + playerPos;
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
		if(instance == null)
		{
			return false;
		}

		for (int i=0; i < instance.players.Count; ++i)
		{
			if (!instance.playersComp[i].IsDead)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetPlayerAliveCount()
	{
		int count = 0;
		for (int i = 0; i < instance.players.Count; ++i)
		{
			if (!instance.playersComp[i].IsDead)
			{
				++count;
			}
		}
		return count;
	}
}
