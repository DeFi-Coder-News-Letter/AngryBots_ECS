using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//We update after all the server simulation as taken place
[UpdateAfter(typeof(MoveForwardSystem)), UpdateAfter(typeof(TurnTowardsPlayerSystem))]
public class SyncECSSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		if (BoltNetwork.IsServer)
		{
			int i = 0;
			Entities.WithAll<EnemyTag>().ForEach((Entity e, ref Translation pos) =>
			{
				ECSSyncObject.Instance.EelTransforms[i] = pos.Value;
				++i;

			});

			if (ECSSyncObject.Instance)
				ECSSyncObject.Instance.Count = i;
		}
		else
		{
			int i = 0;
			Entities.WithAll<EnemyTag>().ForEach((Entity e, ref Translation pos, ref Health health) =>
			{
				//Kill this entity
				if (i > ECSSyncObject.Instance.Count)
				{
					health.Value = -1;
				}

				pos.Value = ECSSyncObject.Instance.EelTransforms[i];
				++i;
			});			
		}
	}
}

