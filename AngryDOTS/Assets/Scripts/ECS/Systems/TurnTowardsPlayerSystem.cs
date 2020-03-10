using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
[UpdateBefore(typeof(MoveForwardSystem))]
public class TurnTowardsPlayerSystem : JobComponentSystem
{
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct TurnJob : IJobForEach<Translation, Rotation, PredictedGhostComponent>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<float3> playerPositions;
		[ReadOnly] public uint tick;

		public void Execute([ReadOnly] ref Translation pos, ref Rotation rot, [ReadOnly] ref PredictedGhostComponent prediction)
		{
			if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
				return;

			if (playerPositions.Length == 0)
			{
				return;
			}

			int minIdx = 0;
			var dir = playerPositions[0] - pos.Value;
			var minDist = math.dot(dir, dir);
			for (int i=1; i < playerPositions.Length; ++i)
			{
				dir = playerPositions[i] - pos.Value;
				var dist = math.dot(dir, dir);
				if (dist < minDist)
				{
					minDist = dist;
					minIdx = i;
				}
			}

			float3 heading = playerPositions[minIdx] - pos.Value;
			heading.y = 0f;
			rot.Value = quaternion.LookRotation(heading, math.up());
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		if (!Settings.AnyPlayerAlive())
			return inputDeps;

		var job = new TurnJob
		{
			playerPositions = Settings.PlayerPositions
		};

		return job.Schedule(this, inputDeps);
	}
}

