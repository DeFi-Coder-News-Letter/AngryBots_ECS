using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct TimeToLive : IComponentData
{
	[GhostDefaultField]
	public float Value;
}
