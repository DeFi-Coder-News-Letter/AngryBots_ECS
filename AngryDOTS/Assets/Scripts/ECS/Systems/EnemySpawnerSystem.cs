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

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<EnemySpawnerComponent>();
    }

    protected override void OnUpdate()
    {
        Unity.Mathematics.Random rnd = default(Unity.Mathematics.Random);
        rnd.InitState();

        var group = World.GetExistingSystem<ServerSimulationSystemGroup>();
        var tick = group.ServerTick;
        bool spawnEnemies = Settings.SpawnEnemies;
        bool anyPlayerAlive = Settings.AnyPlayerAlive();
        //bool anyPlayerAlive = true;
        float dt = Time.DeltaTime;

        Entities.ForEach((ref EnemySpawnerComponent spawner) =>
        {
            if (!spawnEnemies || !anyPlayerAlive)
            {
                return;
            }

            cooldown -= dt;

            if (cooldown <= 0f)
            {
                cooldown += Settings.SpawnInterval;
                Spawn(ref rnd);
            }
        });
    }

    private void Spawn(ref Unity.Mathematics.Random rnd)
    {
        for (int i = 0; i < Settings.SpawnsPerInterval; i++)
        {
            int playerIdx = rnd.NextInt(0, Settings.GetPlayerAliveCount());
            float3 pos = Settings.GetPositionAroundPlayer(playerIdx, Settings.EnemySpawnRadius);

            SpawnEnemyECS(ref pos);
        }
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