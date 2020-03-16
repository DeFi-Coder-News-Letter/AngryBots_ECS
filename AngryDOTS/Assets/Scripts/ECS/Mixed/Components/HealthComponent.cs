using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct Health : IComponentData
{
	[GhostDefaultField]
	public float Value;
}