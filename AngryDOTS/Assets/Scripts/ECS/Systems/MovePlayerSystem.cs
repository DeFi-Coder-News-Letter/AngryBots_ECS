using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class MoveCameraSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var networkId = GetSingleton<NetworkIdComponent>();

        Entities.ForEach((ref Translation trans, ref PlayerComponent player) =>
        {
            if (player.playerId == networkId.Value)
            {
                Settings.Camera.Follow.position = trans.Value;
            }
        });
    }
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MovePlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;

        // Get camera frame of reference for movement
        var camForward = Camera.main.transform.forward;
        camForward.y = 0.0f;
        camForward.Normalize();
        var camUp = Vector3.up;
        var camRight = Vector3.Cross(camUp, camForward);

        float3 forward = float3(camForward);
        var up = float3(0.0f, 1.0f, 0.0f);
        float3 right = float3(camRight);

        // Update each player entity movement
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation trans, ref Rotation rot, ref MoveSpeed speed, ref PlayerComponent player, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
            {
                return;
            }

            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            // Movement
            var movement = forward * input.vertical + right * input.horizontal;
            movement = movement * speed.Value * deltaTime;
            trans.Value += movement;

            // Look at
            var mousePos = float3(input.mousePosX, 0f, input.mousePosZ);
            float3 playerToMouse = mousePos - trans.Value;
            playerToMouse.y = 0f;
            playerToMouse = normalize(playerToMouse);
            rot.Value = Unity.Mathematics.quaternion.LookRotation(playerToMouse, up);

            //Settings.SetPlayerPosition(player.playerId, ref trans.Value);
        });
    }
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ShootPlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<ServerSimulationSystemGroup>();
        var tick = group.ServerTick;

        // Update each player entity shooting
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation trans, ref Rotation rot) =>
        {
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            if (input.fire == 1)// Did he fire?
            {
                var position = trans.Value;
                position.y = 0.5f;
                SpawnBulletECS(ref position, ref rot.Value);
                //SpawnBulletSpreadECS(ref position, ref rot.Value);
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

        for (int x = min; x < max; x++)
        {
            tempRot.x = Unity.Mathematics.math.radians((3 * x) % 360);

            for (int y = min; y < max; y++)
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