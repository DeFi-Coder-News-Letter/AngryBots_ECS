using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct Player : IComponentData
{
    [GhostDefaultField]
    public int playerId;
}