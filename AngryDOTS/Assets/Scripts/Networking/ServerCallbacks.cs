using UnityEngine;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "ECS Shooter")]
public class TutorialServerCallbacks : Bolt.GlobalEventListener
{
    void Awake()
    {
        
        MainPlayerObjectRegistry.CreateServerPlayer();
    }

    public override void Connected(BoltConnection connection)
    {
        MainPlayerObjectRegistry.CreateClientPlayer(connection);
    }

    public override void SceneLoadLocalDone(string map)
    {
        var entity = BoltNetwork.Instantiate(BoltPrefabs.ECSSyncProxy);
        entity.TakeControl();

        MainPlayerObjectRegistry.ServerPlayer.Spawn();
    }

    public override void SceneLoadRemoteDone(BoltConnection connection)
    {
        MainPlayerObjectRegistry.GetMainPlayer(connection).Spawn();
    }
}