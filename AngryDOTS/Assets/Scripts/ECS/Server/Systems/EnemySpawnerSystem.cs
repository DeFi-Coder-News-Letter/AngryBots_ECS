using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;

public struct EnemySpawnerComponent : IComponentData
{ }

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class EnemySpawnerSystem : ComponentSystem
{
    private float cooldown = 0.0f;

    // Query to obtain all the players
    EntityQuery playerGroup;

    // Random number generator for spawning
    Random rnd = default(Unity.Mathematics.Random);

    protected override void OnCreate()
    {
        // Requires a spawner
        RequireSingletonForUpdate<EnemySpawnerComponent>();
        // Get a query for all the players
        playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Player>());
        // Init the random number generator
        rnd.InitState();
    }

    protected override void OnUpdate()
    {
        // Should we spawn enemies?
        bool spawnEnemies = Settings.SpawnEnemies;
        if (!spawnEnemies)
        {
            return;
        }

        // There is at least one player alive?
        var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
        bool anyPlayerAlive = healths.Any(h => h.Value > 0);
        if (!anyPlayerAlive)
        {
            healths.Dispose();
            return;
        }
        healths.Dispose();

        float dt = Time.DeltaTime;
        Entities.ForEach((ref EnemySpawnerComponent spawner) =>
        {
            // Already tested if spawnEnemies and anyPlayerAlive
            cooldown -= dt;

            // Should we spawn another enemy?
            if (cooldown <= 0.0f)
            {
                cooldown += Settings.SpawnInterval;
                Spawn();
            }
        });
    }

    private void Spawn()
    {
        // Spawn Settings.SpawnsPerInterval enemies
        for (int i = 0; i < Settings.SpawnsPerInterval; ++i)
        {
            int playerIdx = rnd.NextInt(0, Settings.PlayersCount);
            float3 pos = GetPositionAroundPlayer(playerIdx, Settings.EnemySpawnRadius);

            SpawnEnemyECS(ref pos);
        }
    }

    public float3 GetPositionAroundPlayer(int idx, float radius)
    {
        float3 playerPos = Settings.PlayerPositions[idx];

        float angle = rnd.NextFloat(0f, 2 * Unity.Mathematics.math.PI);
        float s = math.sin(angle);
        float c = math.cos(angle);

        return new float3(c * radius, 1.1f, s * radius) + playerPos;
    }

    private void SpawnEnemyECS(ref float3 pos)
    {
        // Spawn enemy
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<EnemySnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var enemy = EntityManager.Instantiate(prefab);
        // Setup position
        EntityManager.SetComponentData(enemy, new Translation { Value = pos });
    }
}