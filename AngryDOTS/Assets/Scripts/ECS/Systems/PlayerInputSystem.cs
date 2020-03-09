using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
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
        // Get the command target to know to which entity we are attached to
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)// Do we know the target?
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;// Get the network id
            Entities.WithNone<PlayerInput>().ForEach((Entity ent, ref PlayerComponent player) =>
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

        input.horizontal = Input.GetAxis("Horizontal");
        input.vertical = Input.GetAxis("Vertical");

        //Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        LayerMask whatIsGround = LayerMask.NameToLayer("Default");

        //if (Physics.Raycast(ray, out hit, whatIsGround))
        if (Physics.Raycast(ray, out hit))
        {
            input.mousePosX = hit.point.x;
            input.mousePosZ = hit.point.z;
        }
        //input.mousePosX = Input.mousePosition.;
        //input.mousePosZ = hit.point.z;

        //input.fire = Input.GetMouseButton(0) ? 1 : 0;
        input.fire = Input.GetMouseButtonDown(0) ? 1 : 0;

        // Append the command data
        var inputBuffer = EntityManager.GetBuffer<PlayerInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}