using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct AngryDOTSGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PlayerLightGhostSerializer",
            "BulletGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curPlayerLightGhostSpawnSystem = world.GetOrCreateSystem<PlayerLightGhostSpawnSystem>();
        m_PlayerLightSnapshotDataNewGhostIds = curPlayerLightGhostSpawnSystem.NewGhostIds;
        m_PlayerLightSnapshotDataNewGhosts = curPlayerLightGhostSpawnSystem.NewGhosts;
        curPlayerLightGhostSpawnSystem.GhostType = 0;
        var curBulletGhostSpawnSystem = world.GetOrCreateSystem<BulletGhostSpawnSystem>();
        m_BulletSnapshotDataNewGhostIds = curBulletGhostSpawnSystem.NewGhostIds;
        m_BulletSnapshotDataNewGhosts = curBulletGhostSpawnSystem.NewGhosts;
        curBulletGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_PlayerLightSnapshotDataFromEntity = system.GetBufferFromEntity<PlayerLightSnapshotData>();
        m_BulletSnapshotDataFromEntity = system.GetBufferFromEntity<BulletSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<AngryDOTSGhostDeserializerCollection>.InvokeDeserialize(m_PlayerLightSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 1:
                return GhostReceiveSystem<AngryDOTSGhostDeserializerCollection>.InvokeDeserialize(m_BulletSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_PlayerLightSnapshotDataNewGhostIds.Add(ghostId);
                m_PlayerLightSnapshotDataNewGhosts.Add(GhostReceiveSystem<AngryDOTSGhostDeserializerCollection>.InvokeSpawn<PlayerLightSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 1:
                m_BulletSnapshotDataNewGhostIds.Add(ghostId);
                m_BulletSnapshotDataNewGhosts.Add(GhostReceiveSystem<AngryDOTSGhostDeserializerCollection>.InvokeSpawn<BulletSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<PlayerLightSnapshotData> m_PlayerLightSnapshotDataFromEntity;
    private NativeList<int> m_PlayerLightSnapshotDataNewGhostIds;
    private NativeList<PlayerLightSnapshotData> m_PlayerLightSnapshotDataNewGhosts;
    private BufferFromEntity<BulletSnapshotData> m_BulletSnapshotDataFromEntity;
    private NativeList<int> m_BulletSnapshotDataNewGhostIds;
    private NativeList<BulletSnapshotData> m_BulletSnapshotDataNewGhosts;
}
public struct EnableAngryDOTSGhostReceiveSystemComponent : IComponentData
{}
public class AngryDOTSGhostReceiveSystem : GhostReceiveSystem<AngryDOTSGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableAngryDOTSGhostReceiveSystemComponent>();
    }
}
