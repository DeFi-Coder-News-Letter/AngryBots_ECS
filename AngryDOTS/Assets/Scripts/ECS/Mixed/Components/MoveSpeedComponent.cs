using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct MoveSpeed : IComponentData
{
	[GhostDefaultField]
	public float Value;
}