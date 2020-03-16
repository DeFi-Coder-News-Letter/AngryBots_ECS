using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
[UpdateBefore(typeof(MoveForwardSystem))]
public class TurnTowardsPlayerSystem : JobComponentSystem
{
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct TurnJob : IJobForEach<Translation, Rotation, PredictedGhostComponent>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> playerPositions;
		[ReadOnly] public uint tick;

		public void Execute([ReadOnly] ref Translation pos, ref Rotation rot, [ReadOnly] ref PredictedGhostComponent prediction)
		{
			if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
			{
				return;
			}

			// We can avoid checking if playerPositions if empty because we did that before calling the jobs
			int minIdx = 0;
			var dir = playerPositions[0].Value - pos.Value;
			var minDistSqr = math.dot(dir, dir);
			for (int i=1; i < playerPositions.Length; ++i)
			{
				dir = playerPositions[i].Value - pos.Value;
				var distSqr = math.dot(dir, dir);
				if (distSqr < minDistSqr)
				{
					minDistSqr = distSqr;
					minIdx = i;
				}
			}

			var heading = playerPositions[minIdx].Value - pos.Value;
			heading.y = 0f;
			rot.Value = quaternion.LookRotation(heading, math.up());
		}
	}

	// Query to obtain all the players
	EntityQuery playerGroup;

	protected override void OnCreate()
	{
		playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Player>());
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
		bool anyPlayerAlive = healths.Any(h => h.Value > 0);
		if (!anyPlayerAlive)
		{
			healths.Dispose();
			return inputDeps;
		}
		healths.Dispose();

		var players = playerGroup.ToComponentDataArray<Translation>(Allocator.TempJob);
		if (players.Length == 0)// Should never happen since there were at least one Health, but just to be sure :)
		{
			players.Dispose();
			return inputDeps;
		}

		var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
		var tick = group.PredictingTick;

		var job = new TurnJob
		{
			playerPositions = players,
			tick = tick
		};
		// Note that players.Dispose() should not be called, because it will automatically be disposed after all the jobs finished running

		return job.Schedule(this, inputDeps);
	}
}