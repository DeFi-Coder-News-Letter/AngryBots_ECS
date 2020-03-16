using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;
using System.Linq;

// The system that makes the RPC request component transfer
public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest>
{
}

[BurstCompile]
public struct GoInGameRequest : IRpcCommand
{
    // Unused integer for demonstration
    public int value;
    public void Deserialize(ref DataStreamReader reader)
    {
        value = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(value);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<GoInGameRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

// When client has a connection with network id, go in game and tell server to also go in game
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class GoInGameClientSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        // Requiere ghost for sending system
        RequireSingletonForUpdate<EnableAngryDOTSGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, ref NetworkIdComponent id) =>
        {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(ent);
            // Send the GoInGameRequest RPC
            var req = PostUpdateCommands.CreateEntity();
            PostUpdateCommands.AddComponent<GoInGameRequest>(req);
            PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
        });
    }
}

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem
{
    EntityQuery spawnPointGroup;

    protected override void OnCreate()
    {
        // Requiere ghost for sending system
        RequireSingletonForUpdate<EnableAngryDOTSGhostSendSystemComponent>();
        spawnPointGroup = GetEntityQuery(ComponentType.ReadOnly<SpawnPoint>());
    }

    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            var spawnPoints = spawnPointGroup.ToComponentDataArray<SpawnPoint>(Allocator.TempJob);

            PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            int id = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;
            Debug.Log(String.Format("Server setting connection {0} to in game", id));
            
            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
            var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<PlayerLightSnapshotData>();
            //var ghostId = AngryDOTSGhostSerializerCollection.FindGhostType<PlayerSnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            var player = EntityManager.Instantiate(prefab);
            EntityManager.SetComponentData(player, new Player { playerId = id });

            // Select correct player id spawner
            var spawnPoint = spawnPoints.FirstOrDefault(sp => sp.PlayerNumber == id);
            if (spawnPoint.Equals(default(SpawnPoint)))
            {
                var orderedSP = spawnPoints.OrderBy(sp => sp.PlayerNumber);
                spawnPoint = orderedSP.ElementAt(id % spawnPoints.Length);
                Debug.LogWarning($"There isn't a spawn point for player id {id}. The spawn point {spawnPoint.PlayerNumber} will be used");
            }
            EntityManager.SetComponentData(player, new Translation { Value = spawnPoint.Position });
            EntityManager.SetComponentData(player, new Rotation { Value = spawnPoint.Rotation });
            spawnPoints.Dispose();
            PostUpdateCommands.AddBuffer<PlayerInput>(player);// Player input buffer

            PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent { targetEntity = player });
            PostUpdateCommands.DestroyEntity(reqEnt);

            // Create the spawner once the first player connects
            if (id == 1)
            {
                var spawner = EntityManager.CreateEntity();
                EnemySpawnerComponent spawnerComp = new EnemySpawnerComponent { };
                EntityManager.AddComponentData(spawner, spawnerComp);
            }
        });
    }
}
