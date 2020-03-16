using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ShootPlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<ServerSimulationSystemGroup>();
        var tick = group.ServerTick;
        bool bulletSpread = Settings.BulletSpread;

        // Update each player entity shooting
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation trans, ref Rotation rot) =>
        {
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            if (input.fire == 1)// Did he fire?
            {
                var position = trans.Value;
                position.y = 0.5f;
                if (bulletSpread)
                {
                    SpawnBulletSpreadECS(ref position, ref rot.Value);
                }
                else
                {
                    SpawnBulletECS(ref position, ref rot.Value);
                }
            }
        });
    }

    private void SpawnBulletECS(ref float3 pos, ref quaternion rot)
    {
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<BulletSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var bullet = EntityManager.Instantiate(prefab);

        EntityManager.SetComponentData(bullet, new Translation { Value = pos });
        EntityManager.SetComponentData(bullet, new Rotation { Value = rot });
    }

    private void SpawnBulletSpreadECS(ref float3 pos, ref quaternion rot)
    {
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<BulletSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;

        int max = Settings.BulletSpreadAmount / 2;
        int min = -max;
        int totalAmount = Settings.BulletSpreadAmount * Settings.BulletSpreadAmount;

        float3 tempRot = Unity.Mathematics.float3.zero;
        int index = 0;

        NativeArray<Entity> bullets = new NativeArray<Entity>(totalAmount, Allocator.TempJob);
        EntityManager.Instantiate(prefab, bullets);

        for (int x = min; x <= max; x++)
        {
            tempRot.x = Unity.Mathematics.math.radians((3 * x) % 360);

            for (int y = min; y <= max; y++)
            {
                tempRot.y = Unity.Mathematics.math.radians((3 * y) % 360);

                quaternion spreadRot = Unity.Mathematics.math.mul(Unity.Mathematics.quaternion.EulerXYZ(tempRot), rot);

                EntityManager.SetComponentData(bullets[index], new Translation { Value = pos });
                EntityManager.SetComponentData(bullets[index], new Rotation { Value = spreadRot });

                index++;
            }
        }
        bullets.Dispose();
    }
}