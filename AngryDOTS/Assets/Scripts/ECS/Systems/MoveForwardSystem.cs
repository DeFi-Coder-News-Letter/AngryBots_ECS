using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Transforms
{
	[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
	public class MoveForwardSystem : JobComponentSystem
	{
		[BurstCompile]
		[RequireComponentTag(typeof(MoveForward))]
		struct MoveForwardRotation : IJobForEach<Translation, Rotation, MoveSpeed, PredictedGhostComponent>
		{
			[ReadOnly] public float dt;
			[ReadOnly] public uint tick;

			public void Execute(ref Translation pos, [ReadOnly] ref Rotation rot, [ReadOnly] ref MoveSpeed speed, [ReadOnly] ref PredictedGhostComponent prediction)
			{
				if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
				{
					return;
				}

				pos.Value = pos.Value + (dt * speed.Value * math.forward(rot.Value));
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
			var tick = group.PredictingTick;
			var deltaTime = Time.DeltaTime;

			var moveForwardRotationJob = new MoveForwardRotation
			{
				dt = deltaTime,
				tick = tick
			};

			return moveForwardRotationJob.Schedule(this, inputDeps);
		}
	}
}