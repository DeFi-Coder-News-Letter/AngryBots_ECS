using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Settings : MonoBehaviour
{
	static Settings instance;

	[Header("Game Object References")]
	public List<Transform> players;

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
				bool alive = instance.players[i] != null;
				//positions[arrayIdx] = alive ? (float3)instance.players[i].position : new float3(0.0f, 0.0f, 0.0f);
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
	}

	public static Vector3 GetPlayerPosition(int idx)
	{
		return (instance.players[idx] != null) ? instance.players[idx].position : new Vector3(0.0f, 0.0f, 0.0f);
	}

	public static Vector3 GetPositionAroundPlayer(int idx, float radius)
	{
		Vector3 playerPos = Vector3.zero;
		int alive = 0;
		for (int i = 0; i < instance.players.Count; ++i)
		{
			if (instance.players[idx] != null)
				++alive;
			if (alive == idx)
			{
				playerPos = instance.players[i].position;
			}
		}
		//Vector3 playerPos = instance.players[idx].position;

		float angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
		float s = Mathf.Sin(angle);
		float c = Mathf.Cos(angle);
		
		return new Vector3(c * radius, 1.1f, s * radius) + playerPos;
	}

	public static void PlayerDied(int idx)
	{
		if (instance.players[idx] == null)
			return;

		PlayerMovementAndLook playerMove = instance.players[idx].GetComponent<PlayerMovementAndLook>();
		playerMove.PlayerDied();

		instance.players[idx] = null;
	}

	public static bool IsPlayerDead(int idx)
	{
		return instance.players[idx] == null;
	}

	public static bool AnyPlayerAlive()
	{
		for (int i=0; i < instance.players.Count; ++i)
		{
			if (instance.players[i] != null)
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
			if (instance.players[i] != null)
			{
				++count;
			}
		}
		return count;
	}
}
