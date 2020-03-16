using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

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
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation trans, ref Rotation rot, ref MoveSpeed speed, ref Player player, ref PredictedGhostComponent prediction) =>
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
        });
    }
}