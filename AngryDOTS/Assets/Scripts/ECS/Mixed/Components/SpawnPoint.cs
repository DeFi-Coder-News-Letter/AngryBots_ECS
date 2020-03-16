using Unity.Entities;
using Unity.Mathematics;

public struct SpawnPoint : IComponentData
{
    public int PlayerNumber;
    public float3 Position;
    public quaternion Rotation;
}
