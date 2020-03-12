using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RemoveDeadSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity entity, ref Health health, ref Translation pos, ref PlayerTag tag) =>
		{
			if (health.Value <= 0)
			{
				//Settings.PlayerDied(tag.playerIdx);
				PostUpdateCommands.DestroyEntity(entity);
			}
		});

		Entities.ForEach((Entity entity, ref Health health, ref Translation pos, ref EnemyTag tag) =>
		{
			if (health.Value <= 0)
			{
				PostUpdateCommands.DestroyEntity(entity);
				BulletImpactPool.PlayBulletImpact(pos.Value);
			}
		});
	}
}