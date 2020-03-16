using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SpawnPointAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int PlayerNumber;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        SpawnPoint sp = new SpawnPoint { PlayerNumber = PlayerNumber, Position = transform.position, Rotation = transform.rotation };
        manager.AddComponentData(entity, sp);
    }
}
