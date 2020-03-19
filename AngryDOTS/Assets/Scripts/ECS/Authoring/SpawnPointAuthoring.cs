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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position, "SpawnerGizmoLogo.png", true);
    }
}
