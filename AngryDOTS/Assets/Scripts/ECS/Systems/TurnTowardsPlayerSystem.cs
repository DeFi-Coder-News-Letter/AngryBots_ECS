using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(MoveForwardSystem))]
public class TurnTowardsPlayerSystem : JobComponentSystem
{
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct TurnJob : IJobForEach<Translation, Rotation>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<float3> playerPositions;

		public void Execute([ReadOnly] ref Translation pos, ref Rotation rot)
		{
			int minIdx = 0;
			var dir = playerPositions[0] - pos.Value;
			var minDist = math.dot(dir, dir);
			for (int i=1; i < playerPositions.Length ; ++i)
			{
				var dirPlayer = playerPositions[i] - pos.Value;
				var dist = math.dot(dirPlayer, dirPlayer);
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

