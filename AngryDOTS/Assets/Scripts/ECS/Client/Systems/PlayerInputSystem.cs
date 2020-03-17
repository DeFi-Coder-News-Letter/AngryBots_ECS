using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PlayerInputSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        // This system won't work if there isn't 1) a network id and 2) a ghost receive system for this data
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableAngryDOTSGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        // Just send input for the current enabled client
        if (!World.GetExistingSystem<ClientPresentationSystemGroup>().Enabled)
        {
            return;
        }

        // Get the command target to know to which entity we are attached to
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)// Do we know the target?
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;// Get the network id
            Entities.WithNone<PlayerInput>().ForEach((Entity ent, ref Player player) =>
            {
                if (player.playerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<PlayerInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        // Create the player input command data
        var input = default(PlayerInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        // Movement direction
        input.horizontal = Input.GetAxis("Horizontal");
        input.vertical = Input.GetAxis("Vertical");

        // Which direction is looking? Mouse raycast
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        var rayInput = new Unity.Physics.RaycastInput();
        rayInput.Start = ray.origin;
        rayInput.End = ray.origin + ray.direction * 100.0f;
        rayInput.Filter = Unity.Physics.CollisionFilter.Default;

        Unity.Physics.RaycastHit hit2 = new Unity.Physics.RaycastHit();
        bool haveHit = collisionWorld.CastRay(rayInput, out hit2);
        if (haveHit)
        {
            input.mousePosX = hit2.Position.x;
            input.mousePosZ = hit2.Position.z;
        }
        
        // Is he firing?
        input.fire = Input.GetMouseButtonDown(0) ? 1 : 0;

        // Append the command data
        if (localInput != Entity.Null)// Do we know the target?
        {
            try
            {
                var inputBuffer = EntityManager.GetBuffer<PlayerInput>(localInput);
                inputBuffer.AddCommandData(input);
            }catch(Exception)// If the player died remove the target 
            {
                var target = GetSingleton<CommandTargetComponent>();
                target.targetEntity = Entity.Null;
                SetSingleton<CommandTargetComponent>(target);
            }
        }
    }
}