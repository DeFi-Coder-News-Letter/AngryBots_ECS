using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

public struct EnemySpawnerComponent : IComponentData
{ }

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class EnemySpawnerSystem : ComponentSystem
{
    private float cooldown = 0.0f;

    // Query to obtain all the players
    EntityQuery playerGroup;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<EnemySpawnerComponent>();
        playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PlayerTag>());
    }

    protected override void OnUpdate()
    {
        Unity.Mathematics.Random rnd = default(Unity.Mathematics.Random);
        rnd.InitState();

        bool spawnEnemies = Settings.SpawnEnemies;
        //bool anyPlayerAlive = Settings.AnyPlayerAlive();
        //bool anyPlayerAlive = true;
        float dt = Time.DeltaTime;

        if (!spawnEnemies)
        {
            return;
        }

        var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
        bool anyPlayerAlive = healths.Any(h => h.Value > 0);
        if (!anyPlayerAlive)
        {
            healths.Dispose();
            return;
        }
        healths.Dispose();

        Entities.ForEach((ref EnemySpawnerComponent spawner) =>
        {
            // Already tested if spawnEnemies and anyPlayerAlive
            cooldown -= dt;

            if (cooldown <= 0.0f)
            {
                cooldown += Settings.SpawnInterval;
                Spawn(ref rnd);
            }
        });
    }

    private void Spawn(ref Unity.Mathematics.Random rnd)
    {
        for (int i = 0; i < Settings.SpawnsPerInterval; ++i)
        {
            //GameState state = GetSingleton<GameState>();
            //int playerIdx = rnd.NextInt(0, state.playersCount);
            int playerIdx = rnd.NextInt(0, Settings.PlayersCount);
            //float3 pos = Settings.GetPositionAroundPlayer(playerIdx, Settings.EnemySpawnRadius);
            float3 pos = GetPositionAroundPlayer(playerIdx, Settings.EnemySpawnRadius);

            SpawnEnemyECS(ref pos);
        }
    }

    public float3 GetPositionAroundPlayer(int idx, float radius)
    {
        //GameState state = GetSingleton<GameState>();
        //float3 playerPos = state.playerPositions[idx];
        //float3 playerPos = float3(state.playerPositionsX[idx], state.playerPositionsY[idx], state.playerPositionsZ[idx]);
        float3 playerPos = Settings.PlayerPositions[idx];

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

    private void SpawnEnemyECS(ref float3 pos)
    {
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<EnemySnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var enemy = EntityManager.Instantiate(prefab);

        EntityManager.SetComponentData(enemy, new Translation { Value = pos });
    }
}