using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

public struct PlayerLightGhostSerializer : IGhostSerializer<PlayerLightSnapshotData>
{
    private ComponentType componentTypeHealth;
    private ComponentType componentTypeMoveSpeed;
    private ComponentType componentTypePlayer;
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypeRenderBounds;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Player> ghostPlayerType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<PlayerLightSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeHealth = ComponentType.ReadWrite<Health>();
        componentTypeMoveSpeed = ComponentType.ReadWrite<MoveSpeed>();
        componentTypePlayer = ComponentType.ReadWrite<Player>();
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypeRenderBounds = ComponentType.ReadWrite<RenderBounds>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostPlayerType = system.GetArchetypeChunkComponentType<Player>(true);
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref PlayerLightSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataPlayer = chunk.GetNativeArray(ghostPlayerType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetPlayerplayerId(chunkDataPlayer[ent].playerId, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
