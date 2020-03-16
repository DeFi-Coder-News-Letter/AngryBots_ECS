using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct AngryDOTSGhostSerializerCollection : IGhostSerializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PlayerLightGhostSerializer",
            "BulletGhostSerializer",
            "EnemyGhostSerializer",
        };
        return arr;
    }

    public int Length => 3;
#endif
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        if (typeof(T) == typeof(PlayerLightSnapshotData))
            return 0;
        if (typeof(T) == typeof(BulletSnapshotData))
            return 1;
        if (typeof(T) == typeof(EnemySnapshotData))
            return 2;
        return -1;
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
        m_PlayerLightGhostSerializer.BeginSerialize(system);
        m_BulletGhostSerializer.BeginSerialize(system);
        m_EnemyGhostSerializer.BeginSerialize(system);
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
            case 0:
                return m_PlayerLightGhostSerializer.CalculateImportance(chunk);
            case 1:
                return m_BulletGhostSerializer.CalculateImportance(chunk);
            case 2:
                return m_EnemyGhostSerializer.CalculateImportance(chunk);
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_PlayerLightGhostSerializer.SnapshotSize;
            case 1:
                return m_BulletGhostSerializer.SnapshotSize;
            case 2:
                return m_EnemyGhostSerializer.SnapshotSize;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(ref DataStreamWriter dataStream, SerializeData data)
    {
        switch (data.ghostType)
        {
            case 0:
            {
                return GhostSendSystem<AngryDOTSGhostSerializerCollection>.InvokeSerialize<PlayerLightGhostSerializer, PlayerLightSnapshotData>(m_PlayerLightGhostSerializer, ref dataStream, data);
            }
            case 1:
            {
                return GhostSendSystem<AngryDOTSGhostSerializerCollection>.InvokeSerialize<BulletGhostSerializer, BulletSnapshotData>(m_BulletGhostSerializer, ref dataStream, data);
            }
            case 2:
            {
                return GhostSendSystem<AngryDOTSGhostSerializerCollection>.InvokeSerialize<EnemyGhostSerializer, EnemySnapshotData>(m_EnemyGhostSerializer, ref dataStream, data);
            }
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    private PlayerLightGhostSerializer m_PlayerLightGhostSerializer;
    private BulletGhostSerializer m_BulletGhostSerializer;
    private EnemyGhostSerializer m_EnemyGhostSerializer;
}

public struct EnableAngryDOTSGhostSendSystemComponent : IComponentData
{}
public class AngryDOTSGhostSendSystem : GhostSendSystem<AngryDOTSGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableAngryDOTSGhostSendSystemComponent>();
    }
}
