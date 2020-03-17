using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(CollisionSystem))]
public class PlayerTransformUpdateSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		if (!Settings.AnyPlayerAlive())
			return;


		Entities.WithAll<PlayerTag>().ForEach((ref Translation pos, ref PlayerTag tag) =>
		{
			pos = new Translation { Value = Settings.PlayerPositions[tag.playerIdx] };

		});
	}
}