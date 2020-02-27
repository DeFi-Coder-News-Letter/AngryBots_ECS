using Unity.Entities;
using UnityEngine;

public class PlayerToEntityConversion : MonoBehaviour, IConvertGameObjectToEntity
{
	public int playerIdxValue;
	public float healthValue = 1f;


	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		//manager.AddComponent(entity, typeof(PlayerTag));
		PlayerTag tag = new PlayerTag { playerIdx = playerIdxValue };
		manager.AddComponentData(entity, tag);

		Health health = new Health { Value = healthValue };
		manager.AddComponentData(entity, health);
	}
}