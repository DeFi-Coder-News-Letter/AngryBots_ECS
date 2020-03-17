using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Rendering;
using UnityEngine;
using static Unity.Networking.Transport.NetworkEndPoint;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class GameSystem : ComponentSystem
{
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData
    {
    }

    protected override void OnCreate()
    {
        // Just one InitGameComponent component
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    [System.Serializable]
    public class NetworkInfo
    {
        public ConnectionConfig client;
        public ConnectionConfig server;
    }

    [System.Serializable]
    public class ConnectionConfig
    {
        public string type;
        public ushort port;
        public uint ipv4;
    }

    protected override void OnUpdate()
    {
        var jsonConfig = Resources.Load<TextAsset>("connectionConfig");
        NetworkInfo config = JsonUtility.FromJson<NetworkInfo>(jsonConfig.text);

        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.AllWorlds)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();// Network system
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)// Is it a client?
            {
                // Client worlds automatically connect to localhost
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;// Default
                ep.Port = 7979;
                if (config.client.type == "LoopbackIpv4")
                {
                    ep = NetworkEndPoint.LoopbackIpv4;
                    ep.Port = config.client.port;
                }else if (config.client.type == "RawConnection")
                {
                    RawNetworkAddress address = default(RawNetworkAddress);
                    address.ipv4 = config.client.ipv4;
                    address.port = config.client.port;
                    ep.rawNetworkAddress = address;
                }
                network.Connect(ep);
            }
#if UNITY_EDITOR
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)// Is it a server?
            {
                // Server world automatically listens for connections from any host
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;// Default
                ep.Port = 7979;
                if (config.server.type == "LoopbackIpv4")
                {
                    ep = NetworkEndPoint.LoopbackIpv4;
                    ep.Port = config.client.port;
                }
                else if (config.server.type == "AnyIpv4")
                {
                    ep = NetworkEndPoint.AnyIpv4;
                    ep.Port = config.client.port;
                }
                else if (config.server.type == "RawConnection")
                {
                    RawNetworkAddress address = default(RawNetworkAddress);
                    address.ipv4 = config.client.ipv4;
                    address.port = config.client.port;
                    ep.rawNetworkAddress = address;
                }
                network.Listen(ep);
            }
#endif
        }
    }
}

// HACK for Unity error :(
public class HacksSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        /* Suppresses the error: "ArgumentException: A component with type:BoneIndexOffset has not been added to the entity.", until the Unity bug is fixed. */
        World.GetOrCreateSystem<CopySkinnedEntityDataToRenderEntity>().Enabled = false;
    }
}