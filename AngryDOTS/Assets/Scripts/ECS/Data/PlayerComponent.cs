using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct PlayerComponent : IComponentData
{
    [GhostDefaultField]
    public int playerId;
}