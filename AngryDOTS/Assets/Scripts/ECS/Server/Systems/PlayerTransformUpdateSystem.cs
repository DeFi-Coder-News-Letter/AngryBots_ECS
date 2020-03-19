using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(CollisionSystem))]
public class PlayerTransformUpdateSystem : ComponentSystem
{
	// Query to obtain all the players
	EntityQuery playerGroup;

	protected override void OnCreate()
	{
		playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Player>());
	}

	protected override void OnUpdate()
	{
		var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
		Settings.PlayersAlive = healths.Length;
		healths.Dispose();

		var playersComp = playerGroup.ToComponentDataArray<Player>(Allocator.TempJob);
		Settings.PlayersCount = playersComp.Length;
		playersComp.Dispose();

		Settings.PlayerPositions.Clear();
		int i = 0;
		Entities.WithAll<Player>().ForEach((ref Translation pos, ref Player player) =>
		{
			Settings.PlayerPositions.Add(pos.Value);
			++i;
		});
	}
}