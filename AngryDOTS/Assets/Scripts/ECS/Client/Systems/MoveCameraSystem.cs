using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class MoveCameraSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var networkId = GetSingleton<NetworkIdComponent>();

        Entities.ForEach((ref Translation trans, ref Player player) =>
        {
            if (player.playerId == networkId.Value)
            {
                Settings.Camera.Follow.position = trans.Value;
            }
        });
    }
}